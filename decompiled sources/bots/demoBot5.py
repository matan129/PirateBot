import random

def do_turn(game):
    if len(game.islands()) == 0:
        return 
    if len(game.my_pirates()) == 0:
        return 
    capturer = game.my_pirates()[0]
    if not capturer.is_lost:
        island = None
        min_dist = 9999
        for i in game.not_my_islands():
            d = game.distance(i, capturer)
            if d < min_dist:
                min_dist = d
                island = i

        directions = game.get_directions(capturer, island)
        game.set_sail(capturer, directions[0])
    if len(game.enemy_pirates()) == 0:
        return 
    target1 = game.enemy_pirates()[0]
    if not target1.is_lost:
        for i in range(1, 4):
            if len(game.my_pirates()) > i:
                pirate = game.my_pirates()[i]
                directions = game.get_directions(pirate, target1)
                game.set_sail(pirate, directions[0])

    if len(game.enemy_pirates()) == 1:
        return 
    target2 = game.enemy_pirates()[1]
    if not target2.is_lost:
        for i in range(4, 6):
            if len(game.my_pirates()) > i:
                pirate = game.my_pirates()[i]
                directions = game.get_directions(pirate, target2)
                game.set_sail(pirate, directions[-1])




