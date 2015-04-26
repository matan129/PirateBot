def do_turn(game):
	pirate = game.all_my_pirates()[0]

	if game.get_turn() < 5:
		# Try to sail to another pirate
		other_pirate = game.all_my_pirates()[1]
		game.debug("Distance to other pirate %s", game.distance(pirate, other_pirate))
		try_to_set_sail(game, pirate, other_pirate.location)
	elif game.get_turn() < 20:
		# Sail to a location relative to the other pirate
		other_pirate = game.all_my_pirates()[1]
		# Create a new location based on the other pirate location
		near_other_pirate = (other_pirate.location[0] + 10, other_pirate.location[1] + 10)

		game.debug("Distance to other pirate: %s", game.distance(pirate, other_pirate))
		game.debug("Distance to target location: %s", game.distance(pirate.location, near_other_pirate))

		# When going out of the other pirate's support range
		if not game.in_range(pirate.location, other_pirate.location):
			game.debug("Out of other pirate support range")

		try_to_set_sail(game, pirate, near_other_pirate)

	elif game.get_turn() < 90:
		# Sail to an island
		island = game.islands()[0]
		game.debug("Distance to island %s", game.distance(pirate, island))
		try_to_set_sail(game, pirate, island.location)

	elif game.get_turn() < 400:
		# Sail to an enemy
		enemy_pirate = game.all_enemy_pirates()[0]
		game.debug("Distance to enemy %s", game.distance(pirate, enemy_pirate))
		try_to_set_sail(game, pirate, enemy_pirate.location)
	else :
		# Sail to center of the map
		# calcualte a new location by the number of the rows and cols in the map.
		center = (game.get_rows() / 2, game.get_cols() / 2)
		game.debug("Distance to center %s", game.distance(pirate.location, center))
		try_to_set_sail(game, pirate, center)



'''
Try to sail to a destination if:
 * Pirate is not lost
 * Not going to sail to where another pirate currently is (try different directions)
 * Not already in destination
 * The location we're moving to next turn is passable
 * Not entering enemy pirate range
'''
def try_to_set_sail(game, pirate, destination):

	if pirate.is_lost:
		# Pirate is lost, can't sail
		game.debug("A lost pirate can't sail!")
		return

	# NOTE: check location equality using equals and not using ==
	if pirate.location == destination:
		# No point in moving if already at the destination
		game.debug("Arrived at destination!")
		return

	# Get the first direction toward the destination
	directions = game.get_directions(pirate, destination)
	direction = directions[0]
	# Get the location on the next turn if we'll sail in that direction
	location_next_turn = game.destination(pirate, direction)
	# If there's a pirate on the destination, don't sail there to avoid collision,
	# and try to use a different direction
	if len(directions) > 1 and game.get_pirate_on(location_next_turn) is not None:
		game.debug("Almost moved to an occupied location! Trying alternative direction")
		# Use the other direction
		direction = directions[1]
		# recalculate the destination of moving in that direction on the next turn
		location_next_turn = game.destination(pirate, direction)


	# If the location is not on the map or in enemy zone, don't move there
	if not game.is_passable(location_next_turn):
		game.debug("Oops, can't go to %s, not passable", location_next_turn)
		return


	# If the location is not on the map or in enemy zone, don't move there
	if game.get_pirate_on(location_next_turn) is not None:
		game.debug("Oops, can't go to %s, there's a pirate there", location_next_turn)
		return


	# Check for all (non-lost) enemy pirates if the movement will get us in their attack range
	# If it will, avoid conflict like a real man!
	for enemy_pirate in game.enemy_pirates():
		if game.in_range(enemy_pirate, location_next_turn):
			game.debug("Can't move there, enemy pirate is in range! My location: %s, his: %s",
					location_next_turn, enemy_pirate.location)
			return



	# If we got here, actually sail to the destination
	game.set_sail(pirate, direction)



