def do_turn(game):
    '''
    In this example we will see how to use all the API that deals with the pirate ships
    First, we will see functions for getting pirates (enemy pirates, my pirates, etc.. )
    Then, we will use the properties/members of each pirate
    Finally, we will show how to make your own function for returning a list of pirates.
    '''
    
    game.debug("You have %s Pirates in the game", len(game.all_my_pirates()))
    game.debug("You have %s Pirates on the map this turn", len(game.my_pirates()))

    game.debug("The enemy has %s Pirates in the game", len(game.all_enemy_pirates()))
    game.debug("The enemy has %s Pirates on the map this turn", len(game.enemy_pirates()))

    # check if we have no more pirates on the map
    if len(game.my_pirates()) == 0:
        game.debug("We have no pirates! nothing to do..")
        return

    # check if we have less pirates than the enemy
    if len(game.enemy_pirates()) > len(game.my_pirates()):
        game.debug("Oh no! There are more enemies on the map!")

    
    # choose some pirate
    # game.get_my_pirate(id) gets the pirate that has that id. id is a number.
    my_pirate = game.get_my_pirate(1)
    # choose some enemy pirate
    enemy = game.get_enemy_pirate(2)
    # choose some island
    target_island = game.get_island(2)

    if my_pirate.is_lost:
        # our pirate is lost... we shouldn't do anything
        game.debug("Pirate is lost :( ")
        return

    # print some information about our pirate:
    game.debug("Us >> Id: %s, Owner: %s, Location %s, InitialLocation: %s" , my_pirate.id, my_pirate.owner, my_pirate.location, my_pirate.initial_loc)
    # print some information about enemy pirate:
    game.debug("Enemy >> Id: %s, Owner: %s, Location %s, InitialLocation: %s", enemy.id, enemy.owner, enemy.location, enemy.initial_loc)
    
    # if the enemy isn't lost we will go to it - otherwise we will go to the island
    if not enemy.is_lost:
        # enemy is in the game! go to it.
        game.debug("Enemy isnt lost - going to enemy. distance is: %s", game.distance(my_pirate, enemy))
        dir = game.get_directions(my_pirate, enemy)[-1]
    else:
        # enemy isn't in the game. go to the island.
        game.debug("Enemy is lost! and will return in %s turns. Going to island", enemy.turns_to_revive)
        dir = game.get_directions(my_pirate, target_island)[-1]

    game.set_sail(my_pirate, dir)

    # check if enemy is on the island
    if game.get_pirate_on(target_island) == enemy:
        game.debug("Enemy is on the island!")

    # check if we are on the island
    if game.get_pirate_on(target_island) == my_pirate:
        game.debug("My pirate is on the island!")

    # print how many lost pirates we have with the function we write
    game.debug("We have %s lost pirates", len(my_lost_pirates(game)))



# This function will return us a list of our lost pirates
# For advanced programmers - you can implement this function in one line only!
# Hint: use the list comprehension
def my_lost_pirates(game):

    # Create an empty list
    lost_pirates = []

    # for loop - go through all the pirates in all_my_pirates()
    for pirate in game.all_my_pirates():
        if pirate.is_lost:
            lost_pirates.append(pirate)
    
    # return the list
    return lost_pirates
