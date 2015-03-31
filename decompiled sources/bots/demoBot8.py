"""
    set bot destinations by a strategy (a few conquerer bots, and the rest attack a single target)
"""
import itertools
import random

class Mover:

    def __init__(self, game):
        self.game = game
        self.assigned_pirates = set()
        self.assigned_locations = set()



    def play(self):
        self.send_capturing_pirate()
        self.send_capturing_pirate()
        self.send_capturing_pirate()
        other_pirates = self.get_available_pirates()
        attack_target = self.find_attack_target(other_pirates)
        if attack_target != None:
            for pirate in other_pirates:
                self.sail_to(pirate, attack_target)




    def get_available_pirates(self):
        return filter(lambda pirate: pirate not in self.assigned_pirates, self.game.my_pirates())



    def min_distance(self, pair):
        (pirate, island,) = pair
        return self.game.distance(pirate.location, island.location)



    def send_capturing_pirate(self):
        (pirate, island,) = self.find_nearest_pirate_island_pair()
        if pirate != None:
            self.sail_to(pirate, island)



    def find_nearest_pirate_island_pair(self):
        pirates = self.get_available_pirates()
        islands = self.game.not_my_islands()
        islands = filter(lambda island: island.location not in self.assigned_locations, islands)
        if len(pirates) == 0 or len(islands) == 0:
            return (None, None)
        pirates_islands = itertools.product(pirates, islands)
        nearest = min(pirates_islands, key=self.min_distance)
        return nearest



    def find_attack_target(self, pirates):
        if len(pirates) == 0:
            return None
        enemies = filter(lambda e: self.game.is_passable(e.location), self.game.enemy_pirates())
        if len(enemies) == 0:
            return None
        pirates_enemies = itertools.product(pirates, enemies)
        nearest = min(pirates_enemies, key=self.min_distance)
        return nearest[1]



    def valid_direction(self, pirate, direction):
        destination = self.game.destination(pirate, direction)
        (row, col,) = destination
        is_available = destination not in self.assigned_locations
        is_on_board = row >= 0 and row < self.game.rows and col >= 0 and col < self.game.cols
        is_passable = is_on_board and self.game.is_passable(destination)
        is_safe = self.get_location_safety(destination) > 0
        return is_passable and is_available and is_safe



    def get_location_safety(self, location):
        is_in_range = lambda l: self.game.in_range(location, l) or l.location == location
        defenders = reduce(lambda x, y: x + (1 if is_in_range(y) else 0), self.game.my_pirates(), 0)
        attackers = reduce(lambda x, y: x + (1 if is_in_range(y) else 0), self.game.enemy_pirates(), 0)
        return defenders - attackers



    def sail_to(self, pirate, location):
        directions = self.game.get_directions(pirate, location)
        free_locations = filter(lambda d: self.valid_direction(pirate, d), directions)

        def do_sail(direction):
            self.game.set_sail(pirate, direction)
            self.assigned_pirates.add(pirate)
            self.assigned_locations.add(self.game.destination(pirate, direction))


        if len(free_locations) > 0:
            direction = random.choice(free_locations)
            do_sail(direction)
        else:
            valid_random_directions = filter(lambda d: self.valid_direction(pirate, d), 'nesw-')
            if len(valid_random_directions) > 0:
                direction = random.choice(valid_random_directions)
                do_sail(direction)




def do_turn(game):
    mover = Mover(game)
    mover.play()



