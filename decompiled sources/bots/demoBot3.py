""" Demo 3 sends two pirates to go get islands """
pirates = [0, 1]
islands = [3, 2]

def do_turn(game):
    global islands
    global pirates
    if len(game.not_my_islands()) < 2:
        return 
    for (index, p,) in enumerate(pirates):
        if game.get_my_pirate(p).is_lost:
            newp = [ pir.id for pir in game.my_pirates() if pir.id not in pirates ]
            if newp:
                pirates[index] = newp[0]

    for (index, i,) in enumerate(islands):
        if game.get_island(i).owner == game.ME:
            newi = [ islnd.id for islnd in game.not_my_islands() if islnd.id not in islands ]
            if newi:
                islands[index] = newi[0]

    for i in range(len(pirates)):
        pir = game.get_my_pirate(pirates[i])
        islnd = game.get_island(islands[i])
        directions = game.get_directions(pir, islnd)[0]
        game.set_sail(pir, directions)




