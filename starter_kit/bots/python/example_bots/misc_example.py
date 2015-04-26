import time

def do_turn(game):
    '''
    In this example we will see a few useful utility functions
    which will help you create more powerful Bots
    '''

    # Use these functions to see the scores of each team and how many points we got on the last turn
    if game.get_enemy_score() > game.get_my_score():
        game.debug("We are losing! Enemy has %s points and we have %s", game.get_enemy_score(), game.get_my_score())
    game.debug("Last turn we got %s points", game.get_last_turn_points()[game.ME])
    game.debug("Last turn enemy got %s points", game.get_last_turn_points()[game.ENEMY])

    game.debug("To win we need %s points", game.get_max_points())

    # We can check the size of the map
    game.debug("The map has %s rows and %s columns", game.get_rows(), game.get_cols())

    # A turn may only take up till one second - you can check the time if your code is very slow
    game.debug("We have %s milliseconds remaining for our turn", game.time_remaining())
    # put a little sleep here in milliseconds and see that time changed! (maybe even consider a loop)
    time.sleep(0.01);
    game.debug("Now we have %s milliseconds remaining for our turn", game.time_remaining())

    # We can see what turn it is and how many there will be
    game.debug("It is turn #%s of max %s", game.get_turn(), game.get_max_turns())

    # This is the attack radius - if the distance between ships is equal or less than this number then they will attack
    game.debug("Attack radius is %s", game.get_attack_radius())

    # This is the number of turns it will take lost ships to return to the game
    game.debug("Lost ships return in %s turns", game.get_spawn_turns())


