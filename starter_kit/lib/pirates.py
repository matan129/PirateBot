#!/usr/bin/env python
from __future__ import print_function

from random import randrange, choice, shuffle, randint, seed, random
from math import sqrt
from collections import deque, defaultdict

import base64
from fractions import Fraction
import operator
import itertools
from game import Game
from copy import deepcopy
try:
    from sys import maxint
except ImportError:
    from sys import maxsize as maxint

PIRATES = 0
DEAD = -1
LAND = -2
FOOD = -3
WATER = -4
UNSEEN = -5
ISLAND = -6

PLAYER_PIRATE = 'abcdefghij'
ISLAND_PIRATE = 'ABCDEFGHIJ'
PLAYER_ISLAND = '0123456789'
MAP_OBJECT = '$?%*.!'
MAP_RENDER = PLAYER_PIRATE + ISLAND_PIRATE + PLAYER_ISLAND + MAP_OBJECT

HILL_POINTS = 2
RAZE_POINTS = -1
NEUTRAL_ATTACKER = None

# possible directions an pirate can move
AIM = {'n': (-1, 0),
       'e': (0, 1),
       's': (1, 0),
       'w': (0, -1)}

# precalculated sqrt
SQRT = [int(sqrt(r)) for r in range(101)]

class Pirates(Game):
    def __init__(self, options=None):
        # setup options
        map_text = options['map']
        map_data = self.parse_map(map_text)
        # override parameters with params we got from map
        for key, val in map_data['params'].items():
            # only get valid keys - keys that already exist
            if key in options:
                options[key] = val

        self.max_turns = int(options['turns'])
        self.loadtime = int(options['loadtime'])
        self.turntime = int(options['turntime'])
        self.viewradius = int(options["viewradius2"])
        self.attackradius = int(options["attackradius2"])
        self.engine_seed = options.get('engine_seed', randint(-maxint-1, maxint))
        seed(self.engine_seed)
        self.player_seed = options.get('player_seed', randint(-maxint-1, maxint))
        self.cyclic = options.get('cyclic', False)
        self.cutoff_percent = options.get('cutoff_percent', 0.85)
        self.cutoff_turn = options.get('cutoff_turn', 150)

        self.do_attack = {
            'focus':   self.do_attack_focus,
            'closest': self.do_attack_closest,
            'support': self.do_attack_support,
            'damage':  self.do_attack_damage
        }.get(options.get('attack'))
        
        self.maxpoints = options.get("maxpoints")
        self.spawnturns = options.get("spawnturns")
        self.captureturns = options.get("captureturns")
        self.linear_points = options.get("linear_points")
        self.exp_points = options.get("exp_points")
        self.scenario = options.get('scenario', False)
        
        self.turn = 0
        self.num_players = map_data['num_players']
                        
        self.current_pirates = {} # pirates that are currently alive
        self.dead_pirates = []    # pirates that are currently dead
        self.all_pirates = []     # all pirates that have been created

        self.all_food = []     # all food created
        self.current_food = {} # food currently in game
        self.pending_food = defaultdict(int)

        self.hills = {}        # all hills
        self.hive_food = [0]*self.num_players # food waiting to spawn for player
        self.hive_history = [[0] for _ in range(self.num_players)]

        self.islands = []
        self.zones = dict([(player, []) for player in range(self.num_players)])
        self.enemy_zones = dict([(player, []) for player in range(self.num_players)])
        
        # used to cutoff games early
        self.cutoff = None
        self.cutoff_bot = LAND # Can be pirate owner, FOOD or LAND
        self.cutoff_turns = 0
        # used to calculate the turn when the winner took the lead
        self.winning_bot = []
        self.winning_turn = 0
        # used to calculate when the player rank last changed
        self.ranking_bots = None
        self.ranking_turn = 0

        # initialize size
        self.height, self.width = map_data['size']
        self.land_area = self.height*self.width - len(map_data['water'])

        # initialize map
        # this matrix does not track hills, just pirates
        self.map = [[LAND]*self.width for _ in range(self.height)]

        # initialize water
        for row, col in map_data['water']:
            self.map[row][col] = WATER
            
        # cache used by neighbourhood_offsets() to determine nearby squares
        self.offsets_cache = {}

        # the map file is followed exactly
        for id, (loc, owner, attack_duration) in enumerate(map_data['islands']):
            # TODO: correct the attacker param instead of num
            self.add_island(id, loc, owner, None, attack_duration)

        # initialize pirates
        for player, player_pirates in map_data['pirates'].items():
            for id,pirate_loc in enumerate(player_pirates):
                self.add_initial_pirate(pirate_loc, player, id)

        # initialize zones and create enemy_zone lists
        for player, zone_data in enumerate(map_data['zones']):
            self.zones[player] = self.get_zone_locations(zone_data[0], zone_data[1:])
        #self.print_zone()
        
        for player in range(len(self.zones)):
            # select all zones appart from current player
            enemy_zones = [z for p,z in self.zones.items() if p != player]
            # flatten list
            self.enemy_zones[player] = [loc for zone in enemy_zones for loc in zone] 
                
        # initialize scores
        self.score = [0]*self.num_players
        self.score_history = [[s] for s in self.score]

        # used to track dead players, pirates may still exist, but orders are not processed
        self.killed = [False for _ in range(self.num_players)]

        # used to give a different ordering of players to each player
        #   initialized to ensure that each player thinks they are player 0
        self.switch = [[None]*self.num_players + list(range(-5,0)) for i in range(self.num_players)]
        for i in range(self.num_players):
            self.switch[i][i] = 0
            
        # used to track water and land already reveal to player
        self.revealed = [[[False for col in range(self.width)]
                          for row in range(self.height)]
                         for _ in range(self.num_players)]
                         
        # used to track what a player can see
        self.init_vision()

        # the engine may kill players before the game starts and this is needed to prevent errors
        self.orders = [[] for i in range(self.num_players)]
        

    def distance(self, a_loc, b_loc):
        """ Returns distance between x and y squared """
        d_row = abs(a_loc[0] - b_loc[0])
        d_col = abs(a_loc[1] - b_loc[1])
        if self.cyclic:
            d_row = min(d_row, self.height - d_row)
            d_col = min(d_col, self.width - d_col)
        return d_row**2 + d_col**2

    def parse_map(self, map_text):
        """ Parse the map_text into a more friendly data structure """
        pirate_list = None
        island_data = []
        zone_data = []
        width = height = None
        water = []
        food = []
        pirates = defaultdict(list)
        islands = []
        row = 0
        score = None
        num_players = None
        params = {}

        for line in map_text.split('\n'):
            line = line.strip()

            # ignore blank lines and comments
            if not line or line[0] == '#':
                continue

            key, value = line.split(' ', 1)
            key = key.lower()
            if key == 'cols':
                width = int(value)
            elif key == 'rows':
                height = int(value)
            elif key == 'players':
                num_players = int(value)
                if num_players < 2 or num_players > 10:
                    raise Exception("map",
                                    "player count must be between 2 and 10")
            elif key == 'score':
                score = list(map(int, value.split()))
            elif key == 'island':
                island_data.append(value.split())
            elif key == 'zone':
                zone_data.append(value.split())
            elif key == 'm':
                if pirate_list is None:
                    if num_players is None:
                        raise Exception("map",
                                        "players count expected before map lines")
                    pirate_list = [chr(97 + i) for i in range(num_players)]
                    island_pirate = [chr(65 + i) for i in range(num_players)]
                if len(value) != width:
                    raise Exception("map",
                                    "Incorrect number of cols in row %s. "
                                    "Got %s, expected %s."
                                    %(row, len(value), width))
                for col, c in enumerate(value):
                    if c in pirate_list:
                        pirates[pirate_list.index(c)].append((row,col))
                    elif c == MAP_OBJECT[ISLAND]:
                        owner, turns_captured = island_data.pop(0)
                        if owner == MAP_OBJECT[ISLAND]:
                            owner = None
                        else:
                            owner = pirate_list.index(owner)
                        turns_captured = int(turns_captured)
                        islands.append([(row, col), owner, turns_captured])
                    elif c in island_pirate:
                        pirates[island_pirate.index(c)].append((row,col))
                        owner, turns_captured = island_data.pop(0)
                        if owner == MAP_OBJECT[ISLAND]:
                            owner = None
                        else:
                            owner = pirate_list.index(owner)
                        turns_captured = int(turns_captured)
                        islands.append([(row, col), owner, turns_captured])
                    elif c == MAP_OBJECT[FOOD]:
                        food.append((row,col))
                    elif c == MAP_OBJECT[WATER]:
                        water.append((row,col))
                    elif c != MAP_OBJECT[LAND]:
                        raise Exception("map",
                                        "Invalid character in map: %s" % c)
                row += 1
            else:
                # default collect all other parameters
                params[key] = value

        if score and len(score) != num_players:
            raise Exception("map",
                            "Incorrect score count.  Expected %s, got %s"
                            % (num_players, len(score)))
        if height != row:
            raise Exception("map",
                            "Incorrect number of rows.  Expected %s, got %s"
                            % (height, row))

        return {
            'size':         (height, width),
            'num_players':  num_players,
            'islands':      islands,
            'pirates':      pirates,
            'water':        water,
            'zones':        zone_data,
            'params':       params
        }

    def neighbourhood_offsets(self, max_dist):
        """ Return a list of squares within a given distance of loc

            Loc is not included in the list
            For all squares returned: 0 < distance(loc,square) <= max_dist

            Offsets are calculated so that:
              -height <= row+offset_row < height (and similarly for col)
              negative indicies on self.map wrap thanks to python
        """
        if max_dist not in self.offsets_cache:
            offsets = []
            mx = int(sqrt(max_dist))
            for d_row in range(-mx,mx+1):
                for d_col in range(-mx,mx+1):
                    d = d_row**2 + d_col**2
                    if 0 < d <= max_dist:
                        offsets.append((
                            d_row%self.height-self.height,
                            d_col%self.width-self.width
                        ))
            self.offsets_cache[max_dist] = offsets
        return self.offsets_cache[max_dist]

    def init_vision(self):
        """ Initialise the vision data """
        # calculate and cache vision offsets
        cache = {}
        # all offsets that an pirate can see
        locs = set(self.neighbourhood_offsets(self.viewradius))
        locs.add((0,0))
        cache['new'] = list(locs)
        cache['-'] = [list(locs)]

        for d in AIM:
            # determine the previous view
            p_r, p_c = -AIM[d][0], -AIM[d][1]
            p_locs = set(
                (((p_r+r)%self.height-self.height),
                 ((p_c+c)%self.width-self.width))
                for r,c in locs
            )
            cache[d] = [list(p_locs), list(locs-p_locs), list(p_locs-locs)]
        self.vision_offsets_cache = cache

        # create vision arrays
        self.vision = []
        for _ in range(self.num_players):
            self.vision.append([[0]*self.width for __ in range(self.height)])
        # initialise the data based on the initial pirates
        self.update_vision()
        self.update_revealed()

    def update_vision(self):
        """ Incrementally updates the vision data """
        for pirate in self.current_pirates.values():
            if not pirate.orders:
                # new pirate
                self.update_vision_pirate(pirate, self.vision_offsets_cache['new'], 1)
            else:
                order = pirate.orders[-1]
                if order in AIM:
                    # pirate moved
                    self.update_vision_pirate(pirate, self.vision_offsets_cache[order][1], 1)
                    self.update_vision_pirate(pirate, self.vision_offsets_cache[order][-1], -1)
                # else: pirate stayed where it was
        for pirate in self.killed_pirates():
            order = pirate.orders[-1]
            self.update_vision_pirate(pirate, self.vision_offsets_cache[order][0], -1)

    def update_vision_pirate(self, pirate, offsets, delta):
        """ Update the vision data for a single pirate

            Increments all the given offsets by delta for the vision
              data for pirate.owner
        """
        a_row, a_col = pirate.loc
        vision = self.vision[pirate.owner]
        for v_row, v_col in offsets:
            # offsets are such that there is never an IndexError
            vision[a_row+v_row][a_col+v_col] += delta

    def update_revealed(self):
        """ Make updates to state based on what each player can see

            Update self.revealed to reflect the updated vision
            Update self.switch for any new enemies
            Update self.revealed_water
        """
        self.revealed_water = []
        for player in range(self.num_players):
            water = []
            revealed = self.revealed[player]
            switch = self.switch[player]

            for row, squares in enumerate(self.vision[player]):
                for col, visible in enumerate(squares):
                    if not visible:
                        continue

                    value = self.map[row][col]

                    # if this player encounters a new enemy then
                    #   assign the enemy the next index
                    if value >= PIRATES and switch[value] is None:
                        switch[value] = self.num_players - switch.count(None)

                    # mark square as revealed and determine if we see any
                    #   new water
                    if not revealed[row][col]:
                        revealed[row][col] = True
                        if value == WATER or (row, col) in self.enemy_zones[player]:
                            water.append((row,col))
                            
            # update the water which was revealed this turn
            self.revealed_water.append(water)

    def get_perspective(self, player=None):
        """ Get the map from the perspective of the given player

            If player is None, the map is return unaltered.
            Squares that are outside of the player's vision are
               marked as UNSEEN.
            Enemy identifiers are changed to reflect the order in
               which the player first saw them.
        """
        if player is not None:
            v = self.vision[player]
        result = []
        for row, squares in enumerate(self.map):
            map_row = []
            for col, square in enumerate(squares):
                if player is None or v[row][col]:
                    if (row,col) in self.hills:
                        if (row,col) in self.current_pirates:
                            # assume pirate is hill owner
                            # numbers should be divisible by the length of PLAYER_PIRATE
                            map_row.append(square+10)
                        else:
                            map_row.append(square+20)
                    else:
                        map_row.append(square)
                else:
                    map_row.append(UNSEEN)
            result.append(map_row)
        return result

    def render_changes(self, player):
        """ Create a string which communicates the updates to the state

            Water which is seen for the first time is included.
            All visible transient objects (pirates, food) are included.
        """
        updates = self.get_state_changes()
        v = self.vision[player]
        visible_updates = []
        # first add unseen water
        for row, col in self.revealed_water[player]:
            visible_updates.append(['w', row, col])

        # next list all transient objects
        for update in updates:
            ilk, id, row, col, owner = update[0:5]

            # only include updates to squares which are (visible) or (where a player ant just died) or (a fort)
            if v[row][col] or ((ilk == 'd') and update[4] == player) or (ilk == 'f'):
                visible_updates.append(update)

                # switch player perspective of player numbers
                if ilk in ['a', 'd', 'f']:
                    # if fort owner is None - leave it at that
                    if ilk is 'f':
                        # forts have the 'attacker' which should also be switched
                        if update[5] is not None:
                            update[5] = self.switch[player][update[5]]
                        # if the owner of the island is neutral - no need to switch
                        if owner is None:
                            continue
                    update[4] = self.switch[player][owner]

        visible_updates.append([]) # newline
        return '\n'.join(' '.join(map(str,s)) for s in visible_updates)

    def get_state_changes(self):
        """ Return a list of all transient objects on the map.

            Food, living pirates, pirates killed this turn
            Changes are sorted so that the same state will result in the same output
        """

        changes = []

        # all islands on map
        changes.extend(sorted(
            ['f', island.id, island.loc[0], island.loc[1], island.get_owner(),   island.attacker, island.attack_duration]
            for island in self.islands
        ))

        # current pirates
        changes.extend(sorted(
            ['a', pirate.id, pirate.loc[0], pirate.loc[1], pirate.owner,         pirate.initial_loc[0], pirate.initial_loc[1]]
            for pirate in self.current_pirates.values()
        ))
        
        # dead pirates
        changes.extend(sorted(
            ['d', pirate.id, pirate.loc[0], pirate.loc[1], pirate.owner,         pirate.initial_loc[0], pirate.initial_loc[1], self.turns_till_revive(pirate)]
            for pirate in self.dead_pirates
        ))
        return changes

    def get_zone_locations(self, mode, params):
        """ Returns a list of locations that are in a zone.
            Modes may be rect or radius to specify different types of zones.
            Zones do not change throughout the game. Each zone belongs to a player.
        """
        zone_locations = []
        if mode == 'rect':
            assert len(params) == 4, 'Requires 4 parameters for rect zone'
            # in this line the rows/cols get modulated by width/height appropriately so zone selection is easy
            fromrow, fromcol, torow, tocol = [int(param) % [self.height,self.width][i % 2] for i,param in enumerate(params)]
            for r in range(fromrow, torow+1):
                for c in range(fromcol, tocol+1):
                    zone_locations.append((r,c))
        if mode == 'radius':
            assert len(params) == 3, 'Requires 4 parameters for radius zone'
            row, col, rad = [int(i) for i in params]
            row = row % self.height
            col = col % self.width
            pirates = []
            zone_locations.append((row, col))
            for d_row, d_col in self.neighbourhood_offsets(rad):
                new_loc = ((row+d_row) % self.height, (col+d_col) % self.width)
                if self.cyclic or self.distance(new_loc,(row,col)) <= rad:
                    n_loc = self.destination((row, col), (d_row, d_col))
                    zone_locations.append(n_loc)
        return zone_locations

    def get_map_output(self, player=None, replay=False):
        """ Render the map from the perspective of the given player.
    
            If player is None, then no squares are hidden and player ids
              are not reordered.
            TODO: get this function working
        """
        result = []
        if replay and self.scenario:
            for row in self.original_map:
                result.append(''.join([MAP_RENDER[col] for col in row]))
        else:
            for row in self.get_perspective(player):
                result.append(''.join([MAP_RENDER[col] for col in row]))
        return result

    def nearby_pirates(self, loc, max_dist, exclude=None):
        """ Returns pirates where 0 < dist to loc <= sqrt(max_dist)

            If exclude is not None, pirates with owner == exclude
              will be ignored.
        """
        # TODO
        pirates = []
        row, col = loc

        for d_row, d_col in self.neighbourhood_offsets(max_dist):
            # this if will prevent finding enemies through the side of the map if the self.cyclic option is set to false. May make game slower if max_dist is very big and suggest thinking of a way to improve performance in some smarter way.
            # quick tip - the indices of (row+d_row) for example are sometimes negative and sometimes positive and use pythons negative indexing to work well.
            new_loc = ((row+d_row) % self.height, (col+d_col) % self.width)
            if self.cyclic or self.distance(new_loc,(row,col)) <= self.attackradius:
                if PIRATES <= self.map[row+d_row][col+d_col] != exclude:
                    n_loc = self.destination(loc, (d_row, d_col))
                    pirates.append(self.current_pirates[n_loc])
        return pirates

    def parse_orders(self, player, lines):
        """ Parse orders from the given player

            Orders must be of the form: o row col direction
            row, col must be integers
            direction must be in (n,s,e,w)
            Messages must be of the form: m message
        """
        orders = []
        valid = []
        ignored = []
        invalid = []
        for line in lines:
            line = line.strip()
            # ignore blank lines and comments
            if not line or line[0] == '#':
                continue

            line = line.lower()
            data = line.split()

            # validate data format
            if data[0] == 'm':
                # was a debug message - printed in engine
                continue                            
            if data[0] != 'o':
                invalid.append((line, 'unknown action'))
                continue
            if len(data) != 4:
                invalid.append((line, 'incorrectly formatted order'))
                continue

            row, col, direction = data[1:]
            loc = None

            # validate the data types
            try:
                loc = int(row), int(col)
            except ValueError:
                invalid.append((line,'invalid row or col'))
                continue
            if direction not in AIM:
                invalid.append((line,'invalid direction'))
                continue

            # this order can be parsed
            orders.append((loc, direction))
            valid.append(line)

        return orders, valid, ignored, invalid

    def validate_orders(self, player, orders, lines, ignored, invalid):
        """ Validate orders from a given player

            Location (row, col) must be pirate belonging to the player
            direction must not be blocked
            may not enter other team's zone
            Can't multiple orders to one pirate
        """
        valid = []
        valid_orders = []
        seen_locations = set()
        for line, (loc, direction) in zip(lines, orders):
            # validate orders
            if loc in seen_locations:
                invalid.append((line,'duplicate order'))
                continue
            try:
                if self.map[loc[0]][loc[1]] != player:
                    invalid.append((line,'You tried to move a pirate but dont have one at this location'))
                    continue
            except IndexError:
                invalid.append((line,'out of bounds'))
                continue
            if loc[0] < 0 or loc[1] < 0:
                invalid.append((line,'out of bounds'))
                continue
            dest = self.destination(loc, AIM[direction])
            if self.map[dest[0]][dest[1]] in (FOOD, WATER):
                ignored.append((line,'move blocked'))
                continue
            if self.distance(loc,dest) > 1 and not self.cyclic:
                ignored.append((line,'move blocked - cant move out of map'))
                continue
            if dest in self.enemy_zones[player]:
                ignored.append((line,'move blocked - entering enemy zone'))
                continue

            # this order is valid!
            valid_orders.append((loc, direction))
            valid.append(line)
            seen_locations.add(loc)

        return valid_orders, valid, ignored, invalid

    def do_orders(self):
        """ Execute player orders and handle conflicts

            All pirates are moved to their new positions.
            Any pirates which occupy the same square are killed.
        """
        # set old pirate locations to land
        for pirate in self.current_pirates.values():
            row, col = pirate.loc
            self.map[row][col] = LAND

        # determine the direction that each pirate moves
        #  (holding any pirates that don't have orders)
        move_direction = {}
        for orders in self.orders:
            for loc, direction in orders:
                move_direction[self.current_pirates[loc]] = direction
        for pirate in self.current_pirates.values():
            if pirate not in move_direction:
                move_direction[pirate] = '-'

        # move all the pirates
        next_loc = defaultdict(list)
        for pirate, direction in move_direction.items():
            pirate.loc = self.destination(pirate.loc, AIM.get(direction, (0,0)))
            pirate.orders.append(direction)
            next_loc[pirate.loc].append(pirate)

        # if pirate is sole occuppirate of a new square then it survives
        self.current_pirates = {}
        colliding_pirates = []
        for loc, pirates in next_loc.items():
            if len(pirates) == 1:
                self.current_pirates[loc] = pirates[0]
            else:
                for pirate in pirates:
                    self.kill_pirate(pirate, True)
                    colliding_pirates.append(pirate)

        # set new pirate locations
        for pirate in self.current_pirates.values():
            row, col = pirate.loc
            self.map[row][col] = pirate.owner

    def do_spawn(self):
        # handles the reviving of dead pirates
        pirates_to_revive = []
        for pirate in self.dead_pirates:
            # calculate if the tunr has come to revive
            if self.turn - pirate.die_turn >= self.spawnturns:
                # verify no one standing in the pirate's location
                if pirate.initial_loc not in self.current_pirates:
                    pirates_to_revive.append(pirate)

        # remove pirate from dead list and make new one in the alive
        for pirate in pirates_to_revive:
            self.dead_pirates.remove(pirate)
            owner = pirate.owner
            loc = pirate.initial_loc
            new_pirate = Pirate(loc, owner, pirate.id, self.turn)
            row, col = loc
            self.map[row][col] = owner
            self.all_pirates.append(new_pirate)
            self.current_pirates[loc] = new_pirate

    def killed_pirates(self):
        """ Return pirates that were killed this turn """
        return [dead for dead in self.dead_pirates if dead.die_turn == self.turn]
    
    def turns_till_revive(self, pirate):
        return self.spawnturns - (self.turn - pirate.die_turn)

    def add_island(self, id, loc, owner = None, attacker = None, occupied_for = 0):
        island = Island(id, loc, owner, attacker, occupied_for)
        self.islands.append(island)
        return island

    def add_pirate(self, hill):
        """ Spawn an pirate on a hill
        """
        loc = hill.loc
        owner = hill.owner
        pirate = Pirate(loc, owner, self.turn)
        row, col = loc
        self.map[row][col] = owner
        self.all_pirates.append(pirate)
        self.current_pirates[loc] = pirate
        hill.last_touched = self.turn
        return pirate

    def add_initial_pirate(self, loc, owner, id):
        pirate = Pirate(loc, owner, id, self.turn)
        row, col = loc
        self.map[row][col] = owner
        self.all_pirates.append(pirate)
        self.current_pirates[loc] = pirate
        return pirate
    
    def kill_pirate(self, pirate, ignore_error=False):
        """ Kill the pirate at the given location

            Raises an error if no pirate is found at the location
              (if ignore error is set to False)
        """
        try:
            loc = pirate.loc
            self.map[loc[0]][loc[1]] = LAND
            self.dead_pirates.append(pirate)
            pirate.die_turn = self.turn
            return self.current_pirates.pop(loc)
        except KeyError:
            if not ignore_error:
                raise Exception("Kill pirate error",
                                "Pirate not found at %s" %(loc,))

    def player_pirates(self, player):
        """ Return the current and dead pirates belonging to the given player """
        current = [pirate for pirate in self.current_pirates.values() if player == pirate.owner]
        dead = [pirate for pirate in self.dead_pirates if player == pirate.owner]
        return current + dead

    def do_attack_damage(self):
        """ Kill pirates which take more than 1 damage in a turn

            Each pirate deals 1/#nearby_enemy damage to each nearby enemy.
              (nearby enemies are those within the attackradius)
            Any pirate with at least 1 damage dies.
            Damage does not accumulate over turns
              (ie, pirates heal at the end of the battle).
        """
        damage = defaultdict(Fraction)
        nearby_enemies = {}

        # each pirate damages nearby enemies
        for pirate in self.current_pirates.values():
            enemies = self.nearby_pirates(pirate.loc, self.attackradius, pirate.owner)
            if enemies:
                nearby_enemies[pirate] = enemies
                strenth = 10 # dot dot dot
                if pirate.orders[-1] == '-':
                    strenth = 10
                else:
                    strenth = 10
                damage_per_enemy = Fraction(strenth, len(enemies)*10)
                for enemy in enemies:
                    damage[enemy] += damage_per_enemy

        # kill pirates with at least 1 damage
        for pirate in damage:
            if damage[pirate] >= 1:
                self.kill_pirate(pirate)

    def do_attack_support(self):
        """ Kill pirates which have more enemies nearby than friendly pirates

            An pirate dies if the number of enemy pirates within the attackradius
            is greater than the number of friendly pirates within the attackradius.
            The current pirate is not counted in the friendly pirate count.

            1 point is distributed evenly among the enemies of the dead pirate.
        """
        # map pirates (to be killed) to the enemies that kill it
        pirates_to_kill = {}
        for pirate in self.current_pirates.values():
            enemies = []
            friends = []
            # sort nearby pirates into friend and enemy lists
            # TODO: this line was bugged. neatby_pirates got pirate.owner as third param and didnt work. why???
            for nearby_pirate in self.nearby_pirates(pirate.loc, self.attackradius):
                if nearby_pirate.owner == pirate.owner:
                    friends.append(nearby_pirate)
                else:
                    enemies.append(nearby_pirate)
            # add the support an pirate has
            pirate.supporters.append(len(friends))
            # add pirate to kill list if it doesn't have enough support
            if len(friends) < len(enemies):
                pirates_to_kill[pirate] = enemies

        # actually do the killing and score distribution
        for pirate, enemies in pirates_to_kill.items():
            self.kill_pirate(pirate)

    def do_attack_focus(self):
        """ Kill pirates which are the most surrounded by enemies

            For a given pirate define: Focus = 1/NumOpponents
            An pirate's Opponents are enemy pirates which are within the attackradius.
            Pirate alive if its Focus is greater than Focus of any of his Opponents.
            If an pirate dies 1 point is shared equally between its Opponents.
        """
        # maps pirates to nearby enemies
        nearby_enemies = {}
        for pirate in self.current_pirates.values():
            nearby_enemies[pirate] = self.nearby_pirates(pirate.loc, self.attackradius, pirate.owner)

        # determine which pirates to kill
        pirates_to_kill = []
        for pirate in self.current_pirates.values():
            # determine this pirates weakness (1/focus)
            weakness = len(nearby_enemies[pirate])
            # an pirate with no enemies nearby can't be attacked
            if weakness == 0:
                continue
            # determine the most focused nearby enemy
            min_enemy_weakness = min(len(nearby_enemies[enemy]) for enemy in nearby_enemies[pirate])
            # pirate dies if it is weak as or weaker than an enemy weakness
            if min_enemy_weakness <= weakness:
                pirates_to_kill.append(pirate)

        # kill pirates and distribute score
        for pirate in pirates_to_kill:
            self.kill_pirate(pirate)

    def do_attack_closest(self):
        """ Iteratively kill neighboring groups of pirates """
        # maps pirates to nearby enemies by distance
        pirates_by_distance = {}
        for pirate in self.current_pirates.values():
            # pre-compute distance to each enemy in range
            dist_map = defaultdict(list)
            for enemy in self.nearby_pirates(pirate.loc, self.attackradius, pirate.owner):
                dist_map[self.distance(pirate.loc, enemy.loc)].append(enemy)
            pirates_by_distance[pirate] = dist_map

        # create helper method to find pirate groups
        pirate_group = set()
        def find_enemy(pirate, distance):
            """ Recursively finds a group of pirates to eliminate each other """
            # we only need to check pirates at the given distance, because closer
            #   pirates would have been eliminated already
            for enemy in pirates_by_distance[pirate][distance]:
                if not enemy.killed and enemy not in pirate_group:
                    pirate_group.add(enemy)
                    find_enemy(enemy, distance)

        # setup done - start the killing
        for distance in range(1, self.attackradius):
            for pirate in self.current_pirates.values():
                if not pirates_by_distance[pirate] or pirate.killed:
                    continue

                pirate_group = set([pirate])
                find_enemy(pirate, distance)

                # kill all pirates in groups with more than 1 pirate
                #  this way of killing is order-independent because the
                #  the pirate group is the same regardless of which pirate
                #  you start looking at
                if len(pirate_group) > 1:
                    for pirate in pirate_group:
                        self.kill_pirate(pirate)

    def do_islands(self):
        """ Calculates island logic

            Increments the captured_for counter per island if it is still being attacked by the same team as last turn
            Otherwise we reset the counter
            If the capture duration is higher than self.captureturns then we switch owner and reset counter
            Consider refactoring a bit since there are many options here
        """
        # Iterate over islands and check attack status
        for island in self.islands:
            # check if an pirate is on the island (this logic may change or have options in the future)
            # if island occupied by pirate
            if island.loc in self.current_pirates.keys():
                attacker = self.current_pirates[island.loc].owner
                # prevent attacking self
                if not attacker == island.get_owner():
                    # if attack is continuing from last turn
                    if attacker == island.attacker:
                        island.attack_duration += 1
                        island.attack_history[-1][-1] += 1
                    else:
                        # this signifies a new attack - reset counter to 1
                        island.attack_duration = 1
                        island.attacker = attacker
                        island.attack_history.append([attacker, self.turn, 1])
                    # check if capture should happen
                    if island.attack_duration == self.captureturns:
                        # if island belongs to no-one - it becomes the attackers
                        if (island.get_owner() == NEUTRAL_ATTACKER):
                            island.swap_owner(self.turn, attacker)
                        # but if it belongs to someone than it becomes neutral
                        else:
                            island.swap_owner(self.turn, NEUTRAL_ATTACKER)
                        # finally resent the counter since attack is over
                        island.attack_duration = 0
            # island not occupied by pirate
            else:
                island.attack_duration = 0
                island.attacker = NEUTRAL_ATTACKER

    def destination(self, loc, d):
        """ Returns the location produced by offsetting loc by d """
        return ((loc[0] + d[0]) % self.height, (loc[1] + d[1]) % self.width)

    def find_closest_land(self, coord):
        """ Find the closest square to coord which is a land square using BFS

            Return None if no square is found
        """
        if self.map[coord[0]][coord[1]] == LAND:
            return coord

        visited = set()
        square_queue = deque([coord])

        while square_queue:
            c_loc = square_queue.popleft()

            for d in AIM.values():
                n_loc = self.destination(c_loc, d)
                if n_loc in visited: continue

                if self.map[n_loc[0]][n_loc[1]] == LAND:
                    return n_loc

                visited.add(n_loc)
                square_queue.append(n_loc)

        return None

    def get_initial_vision_squares(self):
        """ Get initial squares in bots vision that are traversable

            flood fill from each starting hill up to the vision radius
        """
        vision_squares = {}
        for hill in self.hills.values():
            squares = deque()
            squares.append(hill.loc)
            while squares:
                c_loc = squares.popleft()
                vision_squares[c_loc] = True
                for d in AIM.values():
                    n_loc = self.destination(c_loc, d)
                    if (n_loc not in vision_squares
                            and self.map[n_loc[0]][n_loc[1]] != WATER and
                            self.distance(hill.loc, n_loc) <= self.viewradius):
                        squares.append(n_loc)
        return vision_squares

    def remaining_players(self):
        """ Return the players still alive """
        return [p for p in range(self.num_players) if self.is_alive(p)]

    # Common functions for all games
    def game_over(self):
        """ Determine if the game is over

            Used by the engine to determine when to finish the game.
            A game is over when there are no players remaining, or a single
            player remaining or a player reached the point maximum.
        """
        if len(self.remaining_players()) < 1:
            self.cutoff = 'no bots left'
            self.winning_bot = []
            return True
        if len(self.remaining_players()) == 1:
            self.cutoff = 'bot crashed'
            self.winning_bot = self.remaining_players()
            return True
        if max(self.score) >= self.maxpoints:
            self.cutoff = 'maximum points'
            return True
        return False

    def get_winner(self):
        """ Returns the winner of the game
        
            The winner is defined as the player with the most points.
            In case other bots crash the remaining bot will win automatically.
            If remaining bots crash on same turn - there will be no winner.
        """
        return self.winning_bot
        
    def kill_player(self, player):
        """ Used by engine to signal that a player is out of the game """
        self.killed[player] = True

    def start_game(self):
        """ Called by engine at the start of the game """
        pass

    def finish_game(self):
        """ Called by engine at the end of the game """
        if self.cutoff is None:
            self.cutoff = 'turn limit reached'
            self.calc_significpirate_turns()
                
    def start_turn(self):
        """ Called by engine at the start of the turn """
        self.turn += 1
        #self.dead_pirates = []
        self.revealed_water = [[] for _ in range(self.num_players)]
        self.removed_food = [[] for _ in range(self.num_players)]
        self.orders = [[] for _ in range(self.num_players)]

    def finish_turn(self):
        """ Called by engine at the end of the turn """
        self.do_orders()
        self.do_attack()
        self.do_islands()
        self.do_spawn()

        # log the island control and calculate the score for history
        for player in range(self.num_players):
            player_islands = len([island for island in self.islands if island.get_owner() == player])
            island_points = 0
            if player_islands > 0:
                if self.linear_points:
                    island_points = player_islands * self.linear_points
                else:
                    island_points = self.exp_points**(player_islands - 1)
            # update the score_history = save as previous + island_points
            self.score_history[player].append(self.score_history[player][-1] + island_points)
            # update the current score
            self.score[player] = self.score_history[player][-1]

        # now that all the pirates have moved we can update the vision
        self.update_vision()
        self.update_revealed()

        self.calc_significpirate_turns()

    def calc_significpirate_turns(self):
        ranking_bots = [sorted(self.score, reverse=True).index(x) for x in self.score]
        if self.ranking_bots != ranking_bots:
            self.ranking_turn = self.turn
        self.ranking_bots = ranking_bots

        winning_bot = [p for p in range(len(self.score)) if self.score[p] == max(self.score)]
        if self.winning_bot != winning_bot:
            self.winning_turn = self.turn
        self.winning_bot = winning_bot

    def get_state(self):
        """ Get all state changes

            Used by engine for streaming playback
        """
        updates = self.get_state_changes()
        updates.append([]) # newline

        return '\n'.join(' '.join(map(str,s)) for s in updates)

    def get_player_start(self, player=None):
        """ Get game parameters visible to players

            Used by engine to send bots startup info on turn 0
        """
        result = []
        result.append(['turn', 0])
        result.append(['loadtime', self.loadtime])
        result.append(['turntime', self.turntime])
        result.append(['rows', self.height])
        result.append(['cols', self.width])
        result.append(['max_turns', self.max_turns])
        result.append(['viewradius2', self.viewradius])
        result.append(['attackradius2', self.attackradius])
        result.append(['player_seed', self.player_seed])
        # send whether map is cyclic or not
        result.append(['cyclic', int(self.cyclic)])
        result.append(['numplayers', self.num_players])
        result.append(['spawnturns', self.spawnturns])
        result.append(['captureturns', self.captureturns])
        # information hidden from players
        if player is None:
            for line in self.get_map_output():
                result.append(['m',line])
        result.append([]) # newline
        return '\n'.join(' '.join(map(str,s)) for s in result)

    def get_player_state(self, player):
        """ Get state changes visible to player

            Used by engine to send state to bots
        """
        return self.render_changes(player)

    def is_alive(self, player):
        """ Determine if player is still alive

            Used by engine to determine players still in the game
        """
        if self.killed[player]:
            return False
        else:
            return bool(self.player_pirates(player))

    def get_error(self, player):
        """ Returns the reason a player was killed

            Used by engine to report the error that kicked a player
              from the game
        """
        return ''

    def do_moves(self, player, moves):
        """ Called by engine to give latest player orders """
        orders, valid, ignored, invalid = self.parse_orders(player, moves)
        orders, valid, ignored, invalid = self.validate_orders(player, orders, valid, ignored, invalid)
        self.orders[player] = orders
        return valid, ['%s # %s' % ignore for ignore in ignored], ['%s # %s' % error for error in invalid]

    def get_scores(self, player=None):
        """ Gets the scores of all players

            Used by engine for ranking
        """
        if player is None:
            return self.score
        else:
            return self.order_for_player(player, self.score)

    def order_for_player(self, player, data):
        """ Orders a list of items for a players perspective of player #

            Used by engine for ending bot states
        """
        s = self.switch[player]
        return [None if i not in s else data[s.index(i)]
                for i in range(max(len(data),self.num_players))]

    def get_stats(self):
        """ Get current stats

            Used by engine to report stats
        """
        # in new version it is: <pirateCount> <islandCount> <Ranking/leading> <scores>
        pirate_count = [0] * self.num_players
        for pirate in self.current_pirates.values():
            pirate_count[pirate.owner] += 1
        island_count = [0] * self.num_players
        for island in self.islands:
            if island.get_owner() is not None:
                island_count[island.get_owner()] += 1
                
        stats = {}
        stats['pirates'] = pirate_count
        stats['islands'] = island_count
        stats['score'] = self.score
        return stats
        
        # in old version it was:
        pirate_count = [0 for _ in range(self.num_players+1)]
        for pirate in self.current_pirates.values():
            pirate_count[pirate.owner] += 1
        stats = {}
        stats['pirate_count'] = pirate_count
        stats['winning'] = self.winning_bot
        stats['w_turn'] = self.winning_turn
        stats['ranking_bots'] = self.ranking_bots
        stats['r_turn'] = self.ranking_turn
        stats['score'] = self.score
        stats['s_alive'] = [1 if self.is_alive(player) else 0 for player in range(self.num_players)]
        stats['s_hills'] = [1 if player in self.remaining_hills() else 0 for player in range(self.num_players)]
        stats['climb?'] = []
        for player in range(self.num_players):
            if self.is_alive(player) and player in self.remaining_hills():
                found = 0
                max_score = sum([HILL_POINTS for hill in self.hills.values()
                                 if hill.killed_by is None
                                 and hill.owner != player]) + self.score[player]
                for opponent in range(self.num_players):
                    if player != opponent:
                        min_score = sum([RAZE_POINTS for hill in self.hills.values()
                                         if hill.killed_by is None
                                         and hill.owner == opponent]) + self.score[opponent]
#                        stats['min_score_%s' % player][opponent] = min_score
                        if ((self.score[player] < self.score[opponent]
                                and max_score >= min_score)
                                or (self.score[player] == self.score[opponent]
                                and max_score > min_score)):
                            found = 1
                            #return False
                            break
                stats['climb?'].append(found)
            else:
                stats['climb?'].append(0)
        return stats

    def get_replay(self):
        """ Return a summary of the entire game

            Used by the engine to create a replay file which may be used
            to replay the game.
        """
        replay = {}
        # required params
        replay['revision'] = 3
        replay['players'] = self.num_players

        # optional params
        replay['loadtime'] = self.loadtime
        replay['turntime'] = self.turntime
        replay['turns'] = self.max_turns
        replay['viewradius2'] = self.viewradius
        replay['attackradius2'] = self.attackradius
        replay['engine_seed'] = self.engine_seed
        replay['player_seed'] = self.player_seed

        # map
        replay['map'] = {}
        replay['map']['rows'] = self.height
        replay['map']['cols'] = self.width
        replay['map']['data'] = self.get_map_output(replay=True)

        # food - deprecated
        replay['food'] = []
        
        # pirates
        replay['ants'] = []
        for pirate in self.all_pirates:
            pirate_data = [pirate.initial_loc[0], pirate.initial_loc[1], pirate.spawn_turn]
            if not pirate.die_turn:
                pirate_data.append(self.turn + 1)
            else:
                pirate_data.append(pirate.die_turn)
            pirate_data.append(pirate.owner)
            pirate_data.append(''.join(pirate.orders))
            pirate_data.append(pirate.supporters)

            replay['ants'].append(pirate_data)

        replay['hills'] = []
        replay['forts'] = []
        for island in self.islands:
            turns_and_owners = []
            for turn, owner in island.owners:
                turns_and_owners.append([turn, owner if owner is not None else NEUTRAL_ATTACKER])
            island_data = [island.loc[0],island.loc[1],turns_and_owners,island.attack_history]
            replay['forts'].append(island_data)
        replay['captureturns'] = self.captureturns
            
        # scores
        replay['scores'] = self.score_history
        replay['bonus'] = [0]*self.num_players
        replay['hive_history'] = self.hive_history
        replay['winning_turn'] = self.winning_turn
        replay['ranking_turn'] = self.ranking_turn
        replay['cutoff'] = self.cutoff
        return replay

    def get_game_statistics(self):
        ''' This will return interesting statistics and info about the game '''
        return
        
    def get_map_format(self):
        ''' Returns the map-file equivalent in order to allow pausing of games and continuing from same point '''
        
        
    def print_zone(self):
        for i,row in enumerate(self.map):
            row = ''
            for j,col in enumerate(self.map[i]):
                if (i,j) in [il.loc for il in self.islands]:
                    row += '^'
                elif (i,j) in self.current_pirates:
                    row += '0'
                elif (i,j) in self.zones[1]:
                    row += '-'
                elif (i,j) in self.zones[0]:
                    row += '|'
                else:
                    row += 'x'
            print(row)


class Island:
    # Island class
    # Owners is a list of tuples denoting (first_turn_of_ownership, owner)
    def __init__(self, id, loc, owner=None, attacker=None, attack_duration=0):
        self.id = id
        self.loc = loc
        self.owners = []
        self.owners.append((0, owner))
        self.attacker = attacker
        self.attack_duration = attack_duration
        # attack_history used for replay
        self.attack_history = []

    def get_owner(self):
        return self.owners[-1][1]

    def swap_owner(self, turn, new_owner):
        self.owners.append((turn, new_owner))
        self.attack_duration = 0
        self.attacker = NEUTRAL_ATTACKER

    def __str__(self):
        return '(%s, %s, %s)' % (self.loc, self.get_owner())

class Pirate:
    def __init__(self, loc, owner, id, spawn_turn=None):
        self.loc = loc
        self.owner = owner
        self.id = id

        self.initial_loc = loc
        self.spawn_turn = spawn_turn
        self.die_turn = None
        self.orders = []
        # this is for support mode and logs how much support an pirate had per turn
        self.supporters = []

    def __str__(self):
        return '(%s, %s, %s, %s, %s, %s)' % (self.initial_loc, self.owner, self.id, self.spawn_turn, self.die_turn, ''.join(self.orders))

