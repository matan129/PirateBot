""" Demo 2 sends a single pirate at a time to conquer an island """
cur_pirate = None

def do_turn(game):
    global cur_pirate
    if len(game.not_my_islands()) == 0:
        return 
    if cur_pirate is None or game.get_my_pirate(cur_pirate).is_lost:
        cur_pirate = game.my_pirates()[0].id
    pirate = game.get_my_pirate(cur_pirate)
    island = game.not_my_islands()[0]
    directions = game.get_directions(pirate, island)
    game.set_sail(pirate, directions[0])



