def do_turn(game):
    kraken = game.get_kraken()
    if not kraken:
        game.debug("No Kraken!")
        return

    if (game.get_turn() == 1):
        game.debug("Kraken settings: Turns per move: %s, awake turns: %s, sleep turns: %s, vanished turns: %s" %
            (game.get_kraken_turns_per_move(),
            game.get_kraken_awake_turns(),
            game.get_kraken_sleep_turns(),
            game.get_kraken_vanished_turns()))

    if kraken:
        game.debug("Kraken: %s can_move: %s" % (kraken, kraken.can_move))

    num_pirates = len(game.my_pirates())
    total_pirates = len(game.all_my_pirates())

    if num_pirates == 0:
        return

    for pirate in game.my_pirates():
        if (pirate.id == 0):
            if kraken.is_asleep:
                if kraken.location == pirate.location:
                    game.unleash_the_kraken(pirate, (10,10))
                else:
                    directions = game.get_directions(pirate, kraken)
                    game.set_sail(pirate, directions[0])

            continue

        row = game.get_rows() / total_pirates * pirate.id
        if (pirate.id % 2 == 0):
            row = game.get_rows() - 1 - game.get_rows() / total_pirates * pirate.id
        col = game.get_cols() / total_pirates * pirate.id
        dest = (row, col)
        game.debug("sending pirate %s to %s" %(pirate.id, dest))
        directions = game.get_directions(pirate, dest)
        next_location = game.destination(pirate, directions[0])
        if not game.safe_from_kraken(next_location):
            game.debug("Sending pirate %s to possible death..." % [pirate.id])

        game.set_sail(pirate, directions[0])

