""" 2 stage bot: 
        first set bot destinations by a strategy (a few conquerer bots, and the rest attack a single target)
        then find all possible moves and evaluate their rank (needs some work).
"""
import itertools

class Strategy:

    def __init__(self, game):
        my_pirates_count = len(game.my_pirates())
        other_pirates_count = len(game.enemy_pirates())
        self.be_aggressive = my_pirates_count > other_pirates_count or len(game.my_islands()) == 0
        self.num_capturing_bots = max(len(game.not_my_islands()) / 2, my_pirates_count)




class Mover:

    def __init__(self, game):
        self.strategy = Strategy(game)
        self.game = game
        self.assigned_pirates = dict()
        self.assigned_islands = set()
        self.enemy_pirates = self.game.enemy_pirates()
        self.my_pirates = self.game.my_pirates()



    def play(self):
        for i in range(self.strategy.num_capturing_bots):
            self.send_capturing_pirate()

        other_pirates = self.get_available_pirates()
        attack_target = self.find_attack_target(other_pirates)
        if attack_target != None:
            for pirate in other_pirates:
                self.set_sail_destination(pirate, attack_target)

        self.perform_sail()



    def get_available_pirates(self):
        return filter(lambda pirate: pirate not in self.assigned_pirates, self.my_pirates)



    def min_distance(self, pair):
        (pirate, island,) = pair
        return self.game.distance(pirate.location, island.location)



    def send_capturing_pirate(self):
        (pirate, island,) = self.find_nearest_pirate_island_pair()
        if pirate != None:
            self.assigned_islands.add(island)
            self.set_sail_destination(pirate, island)



    def find_nearest_pirate_island_pair(self):
        pirates = self.get_available_pirates()
        islands = self.game.not_my_islands()
        islands = filter(lambda island: island not in self.assigned_islands, islands)
        if len(pirates) == 0 or len(islands) == 0:
            return (None, None)
        pirates_islands = itertools.product(pirates, islands)
        nearest = min(pirates_islands, key=self.min_distance)
        return nearest



    def find_attack_target(self, pirates):
        if len(pirates) == 0:
            return None
        enemies = filter(lambda e: self.game.is_passable(e.location), self.enemy_pirates)
        if len(enemies) == 0:
            return None
        pirates_enemies = itertools.product(pirates, enemies)
        nearest = min(pirates_enemies, key=self.min_distance)
        return nearest[1]



    def in_range(self, loc1, loc2, include_move):
        ret = False
        if loc1 == loc2:
            ret = True
        (d_row, d_col,) = (loc1[0] - loc2[0], loc1[1] - loc2[1])
        if include_move:
            d_row = abs(d_row) - 1
            d_col = abs(d_col) - 1
        d = d_row ** 2 + d_col ** 2
        if 0 < d <= self.game.attackradius2:
            ret = True
        return ret



    def valid_direction(self, pirate, direction):
        destination = self.game.destination(pirate, direction)
        return self.valid_destination(pirate, destination)



    def valid_destination(self, pirate, destination):
        (row, col,) = destination
        is_on_board = row >= 0 and row < self.game.rows and col >= 0 and col < self.game.cols
        is_passable = is_on_board and self.game.is_passable(destination)
        return is_passable



    def get_location_safety(self, location, my_locations, enemy_locations):
        is_in_range = lambda l, include_move: self.in_range(location, l, include_move)
        defenders = reduce(lambda x, y: x + (1 if is_in_range(y.location, False) else 0), my_locations, 0)
        attackers = reduce(lambda x, y: x + (1 if is_in_range(y.location, True) else 0), enemy_locations, 0)
        return defenders - attackers



    def perform_sail(self):
        pirates_data = []
        pirates = []
        for (pirate, props,) in self.assigned_pirates.iteritems():
            all_directions = []
            preferred_directions = props['directions']
            valid_random_directions = filter(lambda d: self.valid_direction(pirate, d), 'nesw-')
            for direction in valid_random_directions:
                is_preffered = direction in preferred_directions
                location = self.game.destination(pirate, direction)
                safety = self.get_location_safety(location, self.my_pirates, self.enemy_pirates)
                if safety >= (0 if self.strategy.be_aggressive else 1):
                    all_directions.append((direction,
                     location,
                     is_preffered,
                     safety))

            pirates_data.append(all_directions)
            pirates.append(pirate)

        combinations = list(itertools.product(*pirates_data))

        def no_double_location(combination):
            locations = set(map(lambda l: l[1], combination))
            return len(locations) == len(combination)


        valid_combinations = filter(no_double_location, combinations)
        if len(valid_combinations) == 0:
            return 

        def rank(combination):
            rank = 0
            for (direction, location, is_preffered, safety,) in combination:
                rank = rank + (1 if is_preffered else 0)

            return rank


        best_combination = max(valid_combinations, key=rank)
        for (pirate, navigation,) in zip(pirates, best_combination):
            self.game.set_sail(pirate, navigation[0])




    def set_sail_destination(self, pirate, location):
        directions = self.game.get_directions(pirate, location)
        valid_directions = filter(lambda d: self.valid_direction(pirate, d), directions)
        self.assigned_pirates[pirate] = {'directions': valid_directions}




def do_turn(game):
    mover = Mover(game)
    mover.play()



