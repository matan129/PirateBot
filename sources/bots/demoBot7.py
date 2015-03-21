import random

def do_turn(game):
    if not game.my_pirates():
        return 
    stationary_pirates = [ pirate for pirate in game.my_pirates() if pirate.location in [ i.location for i in game.not_my_islands() ] ]
    pirates_to_move = [ p for p in game.my_pirates() if p not in stationary_pirates ]
    future = {pirate.id:pirate.location for pirate in stationary_pirates}
    for pirate in pirates_to_move:
        if pirate.location == pirate.initial_loc:
            if my_pirates_at_start(game) < count_enemy_largest_group(game):
                continue
        closest_island = get_closest_island(game, pirate)
        if not closest_island:
            continue
        directions = generate_directions(game, pirate, closest_island)
        for d in directions:
            dest = game.destination(pirate, d)
            if dest not in future.values():
                future[pirate.id] = dest
                game.set_sail(pirate, d)
                break





def generate_directions(game, pirate, target):
    """ smart generator of directions to give a list arrange by priority """
    alld = ['n',
     'e',
     's',
     'w',
     '-']
    random.shuffle(alld)
    in_order = [ (game.distance(game.destination(pirate, d), target), d) for d in alld ]
    in_order = sorted(in_order, key=lambda x: x[0])
    new_ds = [ d for (_, d,) in in_order ]
    return new_ds



def not_being_captured(game):
    return [ i for i in game.not_my_islands() if i.location not in [ p.location for p in game.my_pirates() ] ] + [ i for i in game.my_islands() if i.location in [ p.location for p in game.enemy_pirates() ] ]



def get_closest_island(game, pirate):
    """ returns a dictionary mapping each pirate to its closest island """
    ds = [ (island, game.distance(pirate, island)) for island in not_being_captured(game) ]
    if ds:
        closest = sorted(ds, key=lambda x: x[1])[0]
        return closest[0]
    return []



def count_enemy_largest_group(game):
    return len(game.enemy_pirates()) - len([ e for e in game.enemy_pirates() if all([ not is_with(e, o) for o in game.enemy_pirates() if not o == e ]) ])



def my_pirates_at_start(game):
    return len([ p for p in game.my_pirates() if game.distance(p.location, p.initial_loc) < 3 ])



def is_with(enemy, other):
    return abs(enemy.location[0] - other.location[0]) <= 1 and abs(enemy.location[1] - other.location[1]) <= 1



