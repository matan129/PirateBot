#!/usr/bin/env python
import sys
import traceback
import random
import base64
import time
from collections import defaultdict
from math import sqrt
import os
import imp

DEFAULT_BOT_FILE =  'my_bot.py'

ME = 0
PIRATES = 0
LOST = -1
WATER = -2
ZONE = -4

PLAYER_PIRATE = 'abcdefghij'
ISLAND_PIRATE = string = 'ABCDEFGHIJ'
PLAYER_ISLAND = string = '0123456789'
MAP_OBJECT = '?%*.!'
MAP_RENDER = PLAYER_PIRATE + ISLAND_PIRATE + PLAYER_ISLAND + MAP_OBJECT

AIM = {'n': (-1, 0),
       'e': (0, 1),
       's': (1, 0),
       'w': (0, -1),
       '-': (0, 0),
       'c': (0, 0),
       'd': (0, 0)}

def sort_by_id(list_to_sort):
    return sorted(list_to_sort, key=lambda x: x.id)

class Pirates():
    def __init__(self):
        self.cols = None
        self.rows = None
        self.map = None
        self.all_islands = []
        self.all_pirates = []
        self.turntime = 0
        self.loadtime = 0
        self.turn_start_time = None
        self.num_players = 0
        self.vision = None
        self.viewradius2 = 0
        self.attackradius2 = 0
        self.spawnradius2 = 0
        self.ghost_cooldown = 0
        self.max_turns = 0
        self.max_points = 0
        self.turn = 0
        self.cyclic = True
        self._orders = {}
        self._loc2pirate = {}
        self.directions = AIM.keys()
        self._scores = []
        self._last_turn_points = []
        self._cloaks_cooldown = []
        self.ME = ME
        # this is only true for 1 vs 1
        self.ENEMY = 1
        self.NEUTRAL = None

    def __setup(self, data):
        'parse initial input and setup starting game state'
        for line in data.split('\n'):
            line = line.strip().lower()
            if len(line) > 0:
                tokens = line.split()
                key = tokens[0]
                if key == 'cols':
                    self.cols = int(tokens[1])
                elif key == 'rows':
                    self.rows = int(tokens[1])
                elif key == 'player_seed':
                    random.seed(int(tokens[1]))
                elif key == 'cyclic':
                    self.cyclic = bool(int(tokens[1]))
                elif key == 'spawnturns':
                    self.spawnturns = int(tokens[1])
                elif key == 'captureturns':
                    self.turns_being_captured = int(tokens[1])
                elif key == 'turntime':
                    self.turntime = int(tokens[1])
                elif key == 'loadtime':
                    self.loadtime = int(tokens[1])
                elif key == 'viewradius2':
                    self.viewradius2 = int(tokens[1])
                elif key == 'attackradius2':
                    self.attackradius2 = int(tokens[1])
                elif key == 'spawnradius2':
                    self.spawnradius2 = int(tokens[1])
                elif key == 'ghost_cooldown':
                    self.ghost_cooldown = int(tokens[1])
                elif key == 'max_turns':
                    self.max_turns = int(tokens[1])
                elif key == 'maxpoints':
                    self.max_points = int(tokens[1])
                elif key == 'start_turn':
                    self.turn = int(tokens[1])
                elif key == 'numplayers':
                    self.num_players = int(tokens[1])
                    self._cloaks_cooldown = [0] * self.num_players
                    self._scores = [0] * self.num_players
                    self._last_turn_points = [0] * self.num_players
        self.map = [[WATER for col in range(self.cols)]
                    for row in range(self.rows)]


    def __update(self, data):
        'parse engine input and update the game state'
        # start timer
        self.turn_start_time = time.time()

        # reset vision
        self.vision = None
        self.all_pirates = []
        self.all_islands = []
        self._loc2pirate = {}
        self._sorted_my_pirates = []
        self._sorted_enemy_pirates = []
        self.turn += 1

        # update map and create new pirate/island lists
        for line in data.split('\n'):
            line = line.strip().lower()
            if len(line) > 0:
                tokens = line.split()
                if tokens[0] == 'w':
                    row, col = [int(token) for token in tokens[1:3]]
                    self.map[row][col] = ZONE
                elif tokens[0] == 'g':
                    if tokens[1] == 's':
                        # these are scores
                        self._scores = [int(s) for s in tokens[2:]]
                    elif tokens[1] == 'c':
                        # these are ghost-ship cooldowns
                        self._cloaks_cooldown = [int(c) for c in tokens[2:]]
                    elif tokens[1] == 'p':
                        self._last_turn_points = [int(p) for p in tokens[2:]]
                else:
                    if len(tokens) >= 4:
                        # format for island is:
                        # f <id> <row> <col> <island_owner=none> <attacker=none> <capture_turns>
                        # format for pirate is:
                        # a <id> <row> <col> <owner> <inital_row> <initial_col>
                        # format for lost pirate is:
                        # d <id> <row> <col> <owner> <inital_row> <initial_col> <turns_to_revive>
                        # where row and col are identical to initial
                        id  = int(tokens[1])
                        y = row = int(tokens[2])
                        x = col = int(tokens[3])
                        owner = None
                        if tokens[4].isdigit():
                            owner = int(tokens[4])
                        if tokens[0] == 'f':
                            team_capturing = None
                            if tokens[5].isdigit():
                                team_capturing = int(tokens[5])
                            turns_being_captured = int(tokens[6])
                            capture_turns = int(tokens[7])
                            value = int(tokens[8])
                            self.all_islands.append(Island(id, (row,col), owner, team_capturing, turns_being_captured, capture_turns, value))
                            self.map[row][col] = '$'
                        else:
                            if tokens[0] == 'a' or tokens[0] == 'd':
                                initial_loc = (int(tokens[5]), int(tokens[6]))
                                pirate = Pirate(id, (row,col), owner, initial_loc)
                                if tokens[0] == 'd':
                                    # dead pirates revive info
                                    turns_to_revive = int(tokens[7])
                                    pirate.turns_to_revive = turns_to_revive
                                    pirate.is_lost = True
                                else:
                                    # live pirates cloaked info
                                    pirate.is_cloaked = int(tokens[7]) != 0
                                    self._loc2pirate[(row,col)] = pirate
                                self.all_pirates.append(pirate)

        # create main helper members which are lists sorted by IDs
        self._sorted_my_pirates = sort_by_id([pirate for pirate in self.all_pirates
                                            if pirate.owner == ME])
        self._sorted_enemy_pirates = sort_by_id([pirate for pirate in self.all_pirates
                                            if pirate.owner != ME])
        self.all_islands = sort_by_id(self.all_islands)


    ''' Island related API '''

    def islands(self):
        return [island for island in self.all_islands]

    def my_islands(self):
        return [island for island in self.all_islands
                    if island.owner == ME]

    def not_my_islands(self):
        return [island for island in self.all_islands
                    if island.owner != ME]

    def enemy_islands(self):
        return [island for island in self.all_islands
                    if island.owner != ME and island.owner is not None]

    def neutral_islands(self):
        return [island for island in self.all_islands
                    if island.owner is None]

    def my_islands_in_danger(self):
        ''' return my islands that are being captured by the enemy '''
        return [island for island in self.my_islands()
                    if island.team_capturing is not None]

    def islands_being_captured_by_me(self):
        ''' return islands that are currently being captured by my pirates '''
        return [island for island in self.all_islands
                    if island.team_capturing == ME]

    def get_island(self, id):
        if id >= len(self.all_islands) or id < 0:
            return None
        return self.all_islands[id]

    ''' Pirate related API '''

    def all_my_pirates(self):
        'return a list of all my pirates sorted by ID'
        return self._sorted_my_pirates

    def my_pirates(self):
        ''' return my pirates that are currently in the game (on screen) '''
        return [pirate for pirate in self.all_my_pirates() if not pirate.is_lost]

    def my_lost_pirates(self):
        ''' return my pirates that are currently out of the game (lost) '''
        return [pirate for pirate in self.all_my_pirates() if pirate.is_lost]

    def all_enemy_pirates(self):
        return self._sorted_enemy_pirates

    def enemy_pirates(self):
        ''' return enemy pirates on the screen '''
        return [pirate for pirate in self.all_enemy_pirates() if not pirate.is_lost and not pirate.is_cloaked]

    def enemy_lost_pirates(self):
        ''' return enemy pirates that are currently out of the game (lost) '''
        return [pirate for pirate in self.all_enemy_pirates() if pirate.is_lost]

    def get_my_pirate(self, id):
        ''' returns and pirate from my_pirates() by id '''
        if id < 0 or id >= len(self.all_my_pirates()):
            return None
        return self.all_my_pirates()[id]

    def get_enemy_pirate(self, id):
        ''' returns and pirate from my_pirates() by id '''
        if id < 0 or id >= len(self.all_enemy_pirates()):
            return None
        return self.all_enemy_pirates()[id]

    def get_pirate_on(self, obj):
        # this will return an pirate or None if no pirate in that location
        loc = self.get_location(obj)
        return self._loc2pirate.get(loc, None)

    ''' Action API '''
  
    def cloak(self, pirate):
        row, col = self.get_location(pirate)
        self._orders[(row, col)] = 'c'

    def reveal(self, pirate):
        row, col = self.get_location(pirate)
        self._orders[(row, col)] = 'd'

    def set_sail(self, pirate, direction):
        if direction == '-':
            return
        row, col = self.get_location(pirate)

        error_string = "direction must be 'n', 's', 'e' or 'w' but was %s" % direction
        assert type(direction) == str, error_string
        assert direction in AIM.keys(), error_string
        # TODO if there is already an order for that, let the user know
        self._orders[(row, col)] = direction

    ''' Primpary helper API '''
    def get_directions(self, loc1, loc2):
        '''
            Determine the 1 or 2 fastest (closest) directions to reach a location
            This method will work for locations or instances with location members
        '''
        row1, col1 = self.get_location(loc1)
        row2, col2 = self.get_location(loc2)
        height2 = self.rows//2
        width2 = self.cols//2
        if row1 == row2 and col1 == col2:
            # return a single move of 'do nothing'
            return ['-']
        d = []
        if row1 < row2:
            if row2 - row1 >= height2 and self.cyclic:
                d.append('n')
            if row2 - row1 <= height2 or not self.cyclic:
                d.append('s')
        if row2 < row1:
            if row1 - row2 >= height2 and self.cyclic:
                d.append('s')
            if row1 - row2 <= height2 or not self.cyclic:
                d.append('n')
        if col1 < col2:
            if col2 - col1 >= width2 and self.cyclic:
                d.append('w')
            if col2 - col1 <= width2 or not self.cyclic:
                d.append('e')
        if col2 < col1:
            if col1 - col2 >= width2 and self.cyclic:
                d.append('e')
            if col1 - col2 <= width2 or not self.cyclic:
                d.append('w')
        return d

    def distance(self, loc1, loc2):
        'calculate the closest distance between to locations'
        row1, col1 = self.get_location(loc1)
        row2, col2 = self.get_location(loc2)

        if not self.cyclic:
            d_col = abs(col1 - col2)
            d_row = abs(row1 - row2)
        else:
            d_col = min(abs(col1 - col2), self.cols - abs(col1 - col2))
            d_row = min(abs(row1 - row2), self.rows - abs(row1 - row2))
        return d_row + d_col

    def destination(self, obj, direction):
        'calculate a new location given the direction and wrap correctly'
        row, col = self.get_location(obj)
        d_row, d_col = AIM[direction]
        if self.cyclic:
            return ((row + d_row) % self.rows, (col + d_col) % self.cols)
        else:
            return ((row + d_row), (col + d_col))

    def is_capturing(self, pirate):
        ''' return true is the pirate is capturing an island '''
        return pirate.location in [i.location for i in self.all_islands if i.owner != pirate.owner]

    def in_range(self, obj1, obj2):
        ''' check if two objects or locations are in attack range '''
        loc1 = self.get_location(obj1)
        loc2 = self.get_location(obj2)
        d_row, d_col = loc1[0]-loc2[0],loc1[1]-loc2[1]
        d = d_row**2 + d_col**2
        if 0 < d <= self.attackradius2:
            return True
        return False

    ''' Debug related API '''

    def debug(self, *args):
        if len(args) == 0:
            return
        message = args[0]
        if len(args) > 1:
            message = args[0] % args[1:]
        # encode to base64 to avoid people printing wierd stuffs
        sys.stdout.write('m %s\n' % base64.b64encode(str(message)))
        # this is important so we get debug messages even if bot crashed
        sys.stdout.flush()

    ''' MetaGame API '''

    def get_scores(self):
        ''' scores is given to the client-side such that it is ordered - first score is his '''
        return self._scores

    def get_my_score(self):
        return self._scores[self.ME]

    def get_enemy_score(self):
        return self._scores[self.ENEMY]

    def get_last_turn_points(self):
        ''' This list is ordered so that first place is the current player and the next is the enemy '''
        return self._last_turn_points

    def get_turn(self):
        ''' Returns the current turn'''
        return self.turn

    def get_max_turns(self):
        ''' Returns maximum number of turns in this game '''
        return self.max_turns

    def get_max_points(self):
        ''' Returns points needed to end the game '''
        return self.max_points

    def get_view_radius(self):
        return self.viewradius2

    def get_attack_radius(self):
        return self.attackradius2

    def get_max_cloak_cooldown(self):
        return self.ghost_cooldown

    def get_spawn_turns(self):
        return self.spawnturns

    def time_remaining(self):
        return self.turntime - int(1000 * (time.time() - self.turn_start_time))

    ''' Cloak API '''

    def get_my_cloaked(self):
        ''' return cloaked ship or None '''
        cloaked = [p for p in self.my_pirates() if p.is_cloaked]
        if cloaked:
            return cloaked[ME]
        return None

    def can_cloak(self):
        ''' return true if your pirate can cloak (cooldown equals 0) - false otherwise '''
        return self._cloaks_cooldown[ME] == 0

    def get_cloak_cooldowns(self):
        ''' return cloak cooldown turns for all players (your player index is 0) '''
        return self._cloaks_cooldown

    ''' Terrain API '''

    def is_passable(self, loc):
        'true if not enemy zone and in map. negative numbers are wrapped'
        row, col = loc
        return row < self.rows and col < self.cols  and row >= 0 and col >= 0 and self.map[row][col] != ZONE

    def is_occupied(self, loc):
        'true if no pirates are at the location'
        return loc in [pirate.location for pirate in self.all_pirates if not pirate.is_lost]

    def is_empty(self, loc):
        return loc not in [self.destination(loc, d) for loc, d in self._orders.items()]

    def get_rows(self):
        return self.rows

    def get_cols(self):
        return self.cols

    ''' Inner API functions '''

    def get_location(self, obj):
        # this abstracts getting an object with a 'location' member or a tuple
        # it will also work if obj is iterable (i.e. - tuple or list of something with locations)
        # assumes all objects in obj (if iterable) are of same type)
        if hasattr(obj, 'location'):
            return obj.location
        elif len(obj) > 0:
            if not hasattr(obj, 'location'):
                return obj
        return [o.location for o in obj]

    def visible(self, loc):
        ' determine which squares are visible to the given player '

        if self.vision == None:
            if not hasattr(self, 'vision_offsets_2'):
                # precalculate squares around an pirate to set as visible
                self.vision_offsets_2 = []
                mx = int(sqrt(self.viewradius2))
                for d_row in range(-mx,mx+1):
                    for d_col in range(-mx,mx+1):
                        d = d_row**2 + d_col**2
                        if d <= self.viewradius2:
                            self.vision_offsets_2.append((
                                # Create all negative offsets so vision will
                                # wrap around the edges properly
                                (d_row % self.rows) - self.rows,
                                (d_col % self.cols) - self.cols
                            ))
            # set all spaces as not visible
            # loop through pirates and set all squares around pirate as visible
            self.vision = [[False]*self.cols for row in range(self.rows)]
            for pirate in self.my_pirates():
                a_row, a_col = pirate
                for v_row, v_col in self.vision_offsets_2:
                    self.vision[a_row + v_row][a_col + v_col] = True
        row, col = loc
        return self.vision[row][col]

    def cancel_order(self, obj):
        loc = self.get_location(obj)
        if loc in self._orders:
            del self._orders[loc]

    def validate_collisions(self):
        ''' returns a list of collisions. each collision is of the following structure:
            [[dest1,[colliders]] , [dest2,colliders], ....]
            where of the colliders, if a collision is caused also by an pirate NOT moving then colliders[0] is the stationary pirate
        '''
        new_locations = dict()
        my_locs = [pirate.location for pirate in self.my_pirates()]
        # this will sort my_pirates so that ones which didn't move appear first!
        sorted_pirates = [x for (x,y) in sorted(
                                            zip(my_locs,[loc in self._orders for loc in my_locs]
                                            ),key=lambda entry: entry[1])]
        for loc in sorted_pirates:
            dest = loc
            if loc in self._orders:
                dest = self.destination(loc, self._orders[loc])
            new_locations[dest] = new_locations.get(dest, list())
            new_locations[dest].append(loc)

        collisions = []
        for dest, loc_list in new_locations.items():
            if len(loc_list) > 1:
                collisions.append([dest, loc_list])

        return collisions

    def cancel_collisions(self):
        ''' Iterate number of pirates and cancel all possible collisions '''
        collisions_canceled = 0
        for _ in range(len(self.my_pirates())):
            collisions = self.validate_collisions()
            if collisions:
                for dest,colliders in collisions:
                    collisions_canceled += len(colliders) - 1
                    [self.cancel_order(pirate) for pirate in colliders[1:]]
            else:
                break
        if collisions_canceled:
            self.debug("WARNING: was forced to cancel collisions for %d pirates", collisions_canceled)

    def __finish_turn(self):
        # write the orders to the game
        for loc, direction in self._orders.items():
            row, col = loc
            sys.stdout.write('o %s %s %s\n' % (row, col, direction))
        self._orders = {}
        # finish the turn by writing the go line
        sys.stdout.write('go\n')
        sys.stdout.flush()

    def render_text_map(self):
        'return a pretty string representing the map'
        # this is buggy since we can no longer represent PIRATE on ISLAND properly
        tmp = ''
        for row in self.map:
            tmp += '# %s\n' % ''.join([MAP_RENDER[col] for col in row])
        return tmp

    # static methods are not tied to a class and don't have self passed in
    # this is a python decorator
    @staticmethod
    def run(bot):
        'parse input, update game state and call the bot classes do_turn method'
        pirates = Pirates()
        map_data = ''
        while(True):
            try:
                current_line = sys.stdin.readline().rstrip('\r\n') # string new line char
                if current_line.lower() == 'ready':
                    pirates.__setup(map_data)
                    pirates.__finish_turn()
                    map_data = ''
                elif current_line.lower() == 'go':
                    pirates.__update(map_data)
                    # call the do_turn method of the class passed in
                    bot.do_turn(pirates)
                    pirates.__finish_turn()
                    map_data = ''
                else:
                    map_data += current_line + '\n'
            except EOFError:
                break
            except KeyboardInterrupt:
                raise
            except:
                # don't raise error or return so that bot attempts to stay alive
                traceback.print_exc(file=sys.stderr)
                sys.stderr.flush()

class Pirate():
    def __init__(self, id, location, owner, initial_loc):
        self.location = location
        self.id = id
        self.owner = owner
        self.initial_loc = initial_loc
        self.is_lost = False
        self.is_cloaked = False
        self.turns_to_revive = None

    def __eq__(self, other):
        if isinstance(other, self.__class__):
            if self.id == other.id and self.owner == other.owner:
                return True
        return False

    def __repr__(self):
        return "<Pirate ID:%d Owner:%d Loc:(%d, %d)>" % (self.id, self.owner, self.location[0], self.location[1])

    def __hash__(self):
        return self.id * 10 + self.owner

class Island():
    def __init__(self, id, location, owner, team_capturing, turns_being_captured, capture_turns, value):
        self.id = id
        self.location = location
        self.owner = owner
        self.team_capturing = team_capturing
        self.capture_turns = capture_turns
        self.turns_being_captured = turns_being_captured
        self.value = value
    def __eq__(self, other):
        if isinstance(other, self.__class__):
            if self.id == other.id:
                return True
        return False
    def __repr__(self):
        return "<Island ID:%d Owner:%s Loc:(%d, %d)>" % (self.id, self.owner, self.location[0], self.location[1])
    def __hash__(self):
        return self.id

class BotController:
    ''' Wrapper class for bot. May accept either a file or a directory and will add correct folder to path '''
    def __init__(self, botpath):
        # define class level variables, will be remembered between turns
        if botpath.endswith('.py'):
            self.bot = imp.load_source("bot", botpath)
        else:
            self.bot = imp.load_compiled("bot", botpath)

    def do_turn(self, game):
        # find the closest island to any pirate
        self.bot.do_turn(game)
        game.cancel_collisions()


if __name__ == '__main__':
    # psyco will speed up python a little, but is not needed
    try:
        import psyco
        psyco.full()
    except ImportError:
        pass

    # try to initiate bot from filepath or directory path
    try:
        # verify we got correct number of arguments
        try:
            filepath = sys.argv[1]
        except IndexError:
            sys.stderr.write('Usage: pythonRunner.py <botpath or botdirectory>\n')
            sys.exit(-1)

        # add python to path and start the BotController
        if os.path.isdir(filepath):
            sys.path.append(filepath)
            botpath = os.path.join(filepath, DEFAULT_BOT_FILE)
        else:
            sys.path.append(os.path.dirname(filepath))
            botpath = filepath

        Pirates.run(BotController(botpath))

    except KeyboardInterrupt:
        print('ctrl-c, leaving ...')
