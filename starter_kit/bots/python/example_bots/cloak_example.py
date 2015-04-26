def do_turn(game):
    '''
    In this example we will see how to use all the API regarding ghost-ships. The Bot is not very smart
    but it will show you how to use all the functions.
    Becoming a ghost ship (=cloak) makes your ship invisible to the other team but your ship can not 
    attack, support or capture islands until it reveals itself.
    Only one of your ships may be cloaked on a turn. After the ship reveals itself there is a cooldown for 
    some turns in which you cannot cloak again. The cooldown is a number which goes down by one
    every turn after you reveal.
    For example, if the cooldown is 50 - you will wait 50 turns until you can cloak again.
    Important: making a ship into a ghost or back with: 'cloak' and 'reveal' are instead of moving on that turn.
    Enjoy the Example!
    '''

    # use game.get_my_cloaked() to find your ghost-ship. It will return None if you dont have one now.
    game.debug("In this game we will wait %d turns after reveal to cloak again", game.get_max_cloak_cooldown())
    game.debug("My cooldown: %d. Other team cooldown: %d", game.get_cloak_cooldowns()[0], game.get_cloak_cooldowns()[1])
    ghost_pirate = game.get_my_cloaked()
    game.debug("get_my_cloaked returns: %s", game.get_my_cloaked())
    if ghost_pirate:
        game.debug("We have a ship that is ghost")
    else:
        game.debug("We do not have a ship that is ghost")

    # use game.can_cloak() to see if you can use cloak on this turn. What does it check?
    # it checks that you don't have a ghost ship and that the cooldown is 0
    game.debug("Can we make a ship into a ghost this turn? %s", game.can_cloak())
    if not game.can_cloak():
        # We can use game.get_cloaks_cooldowns()[0] to see in how many turns we can cloak
        # To see in how many turns the other player can cloak use game.get_cloaks_cooldowns()[1]
        game.debug("We can ghost ship in %d turns", game.get_cloak_cooldowns()[0])
    

    # pick some island to go to
    island = game.get_island(2)
    game.debug("island with id: %s , which belongs to %s", island.id, island.owner)
    
    # if we don't have a ghost ship - choose another one.
    if not ghost_pirate:
         ghost_pirate = game.get_my_pirate(3)
    
    # use is_cloaked to see if a pirate is a ghost ship or not
    game.debug("Pirate %d is ghost: %s", ghost_pirate.id, ghost_pirate.is_cloaked)
    
    # if we are on island && it doesnt belong to us 
    if game.get_pirate_on(island) == ghost_pirate and not island.owner == game.ME:
        # if we are ghost ship it is important to reveal so that we can capture the island
        if ghost_pirate.is_cloaked:
            game.debug("About to reveal the ghost ship!")
            game.reveal(ghost_pirate)
            return
        else:
            # don't do anything - we need to wait to capture the island
            return

    # if we are not ghost ship && we can become ghost.
    if not ghost_pirate.is_cloaked and game.can_cloak():
        # we should become ghost ship
        game.debug("Making our ship a ghost now!")
        game.cloak(ghost_pirate)
        return
    else:
        # we are cloaked already - just sail to the island
        game.debug("Sailing to island!")
        d = game.get_directions(ghost_pirate, island)[0]
        game.set_sail(ghost_pirate, d)
    

