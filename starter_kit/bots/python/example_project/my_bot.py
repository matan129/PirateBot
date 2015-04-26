
import my_bot_utils

def do_turn(game):
    if my_bot_utils.is_turn_multiple_of_100(game):
        game.debug("Turn divides by 100.")
