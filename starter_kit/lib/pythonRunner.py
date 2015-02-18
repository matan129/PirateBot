#!/usr/bin/env python
import sys
import traceback
import random
import base64
import time
from collections import defaultdict
from math import sqrt
import imp

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
       '-': (0, 0)}

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
        self.max_turns = 0
        self.turn = 0
        self.cyclic = True
        self._orders = {}
        self._loc2pirate = {}
        self.directions = AIM.keys()
        self.ME = ME
        # this is only true for 1 vs 1
        self.ENEMY = 1
        self.NEUTRAL = None

    def setup(self, data):
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
                    self.capture_turns = int(tokens[1])
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
                elif key == 'max_turns':
                    self.max_turns = int(tokens[1])
                elif key == 'start_turn':
                    self.turn = int(tokens[1])
                elif key == 'numplayers':
                    self.num_players = int(tokens[1])
        self.map = [[WATER for col in range(self.cols)]
                    for row in range(self.rows)]
       

    def update(self, data):
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
                else:
                    if len(tokens) >= 4:
                        # format for island is:
                        # f <id> <row> <col> <island_owner=none> <attacker=none> <capture_duration>
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
                            capture_duration = int(tokens[6])
                            self.all_islands.append(Island(id, (row,col), owner, team_capturing, capture_duration))
                            self.map[row][col] = '$'
                        else:
                            if tokens[0] == 'a' or tokens[0] == 'd':
                                initial_loc = (int(tokens[5]), int(tokens[6]))
                                pirate = Pirate(id, (row,col), owner, initial_loc)
                                if tokens[0] == 'd':
                                    turns_to_revive = int(tokens[7])
                                    pirate.turns_to_revive = turns_to_revive
                                    pirate.is_lost = True
                                else:
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
        return [pirate for pirate in self.all_enemy_pirates() if not pirate.is_lost]
        
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
    
    ''' Movement related API '''

    def set_sail(self, pirate, direction):
        if direction == '-':
            return
        row, col = self.get_location(pirate)
        
        error_string = "direction must be 'n', 's', 'e' or 'w' but was %s" % direction
        assert type(direction) == str, error_string
        assert direction in AIM.keys(), error_string
        # TODO if there is already an order for that, let the user know
        self._orders[(row, col)] = direction

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
        
    ''' Debug related API '''    
        
    def get_turn(self):
        return self.turn    

    def debug(self, message, turn_to_display=None):
        # turn_to_display may be single number (Turn) or a list/tuple of them
        if turn_to_display:
            if '__iter__' in dir(turn_to_display): 
                if self.get_turn() not in turn_to_display:
                    return 
            else:
                if self.get_turn() != turn_to_display:
                    return
        # encode to base64 to avoid people printing wierd stuffs
        sys.stdout.write('m %s\n' % base64.b64encode(str(message)))
        # this is important so we get debug messages even if bot crashed
        sys.stdout.flush()       

    ''' Misc API ''' 
    def is_capturing(self, pirate):                
        # set the 'is_capturing' member of pirates
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
    
    def is_passable(self, loc):
        'true if not enemy zone and in map. negative numbers are wrapped'
        row, col = loc
        return self.map[row][col] != ZONE and row < self.rows and col < self.cols

    def is_occupied(self, loc):
        'true if no pirates are at the location'
        return loc in [pirate.location for pirate in self.all_pirates if not pirate.is_lost]

    def is_empty(loc):
        return loc not in [self.destination(loc, d) for loc, d in self._orders.items()]
            
    def get_view_radius(self):
        return self.viewradius2
        
    def time_remaining(self):
        return self.turntime - int(1000 * (time.time() - self.turn_start_time))
        
    ''' Inner API functions '''
    
    def get_location(self, obj):
        # this abstracts getting an object with a 'location' member or a tuple
        # it will also work if obj is iterable (i.e. - tuple or list of something with locations)
        # assumes all objects in obj (if iterable) are of same type)
        if 'location' in dir(obj):
            return obj.location
        elif len(obj) > 0:
            if 'location' not in dir(obj[0]):
            # it must be a location tuple (x, y) or a list of location tuples
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
        for _ in range(len(self.my_pirates())):
            collisions = self.validate_collisions()
            if collisions:
                for dest,colliders in collisions:
                    [self.cancel_order(pirate) for pirate in colliders[1:]]
            else:
                break             

    def finish_turn(self):
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
                    pirates.setup(map_data)
                    pirates.finish_turn()
                    map_data = ''
                elif current_line.lower() == 'go':
                    pirates.update(map_data)
                    # call the do_turn method of the class passed in
                    bot.do_turn(pirates)
                    pirates.finish_turn()
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
        self.turns_to_revive = None
        
    def __eq__(self, other):
        if isinstance(other, self.__class__):
            if self.id == other.id and self.owner == other.owner:
                return True
        return False
        
    def __repr__(self):
        return "Pirate ID:%d OWNER:%d LOC:(%d, %d)" % (self.id, self.owner, self.location[0], self.location[1])
        
    def __hash__(self):
        return self.id * 10 + self.owner

class Island():
    def __init__(self, id, location, owner, team_capturing, capture_duration):
        self.id = id
        self.location = location
        self.owner = owner
        self.team_capturing = team_capturing
        self.capture_duration = capture_duration
    def __eq__(self, other):
        if isinstance(other, self.__class__):
            if self.id == other.id:
                return True
        return False
    def __repr__(self):
        return "Island ID:%d OWNER:%s LOC:(%d, %d)" % (self.id, self.owner, self.location[0], self.location[1])
    def __hash__(self):
        return self.id

        
class BotController:
    '''In this bot we find the minimum distance between an pirate and island and send all the pirates going to that island'''
    '''Basic collision avoidance is then applied'''
    def __init__(self):
        # define class level variables, will be remembered between turns
        if sys.argv[1].endswith('.py'):
            self.bot = imp.load_source("bot", sys.argv[1])
        else:
            self.bot = imp.load_compiled("bot", sys.argv[1])
    
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
    
    try:
        # if run is passed a class with a do_turn method, it will do the work
        # this is not needed, in which case you will need to write your own
        # parsing function and your own game state class
        Pirates.run(BotController())
    except KeyboardInterrupt:
        print('ctrl-c, leaving ...')
