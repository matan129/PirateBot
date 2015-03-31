import random

def do_turn(game):
    if len(game.islands()) == 0:
        return 
    not_mine = game.islands()
    for i in range(len(game.my_pirates())):
        pirate = game.my_pirates()[i]
        directions = game.get_directions(pirate, not_mine[(i % len(not_mine))])
        random.shuffle(directions)
        game.set_sail(pirate, directions[0])




