island_id_to_capture = None

def do_turn(game) :

	display_island_counts(game)
	go_to_island(game)


def go_to_island(game):
	global island_id_to_capture

	# Don't do anything if have not pirates or no islands to capture
	if len(game.my_pirates()) == 0 or len(game.not_my_islands()) == 0:
		return

	# choose a pirate
	pirate = game.my_pirates()[0]

	if island_id_to_capture is None:
		# No islands yet, getting the first island
		target_island = game.not_my_islands()[0]
		island_id_to_capture = target_island.id
	else:
		# Already has an island as destination, get it by id
		target_island = game.get_island(island_id_to_capture)
		if target_island.owner == game.ME:
			# The island is already captured by me, get a different island
			game.debug("Captured The island")
			target_island = game.not_my_islands()[0]


	if island_id_to_capture != target_island.id:
		# The island to capture has changed, display info about it
		island_id_to_capture = target_island.id
		show_island_info(game, target_island)
		if target_island.value > 1:
			game.debug("Found the treasure island!")

	if game.get_pirate_on(target_island) == pirate:
		if game.is_capturing(pirate):
			# Capturing
			game.debug("I'm capturing!")

	else:
		# Not on the island yet, sail to it
		direction = game.get_directions(pirate.location, target_island.location)[0]
		game.set_sail(pirate, direction)



def show_island_info(game, island):
	'''
	Print information about a specific island
	'''
	game.debug("Island: id: %s, location: %s, owner: %s, value: %s," + \
			"Captured by: %s, turns being captured: %s", \
			island.id,
			island.location,
			island.owner,
			island.value,
			island.team_capturing,
			island.turns_being_captured)

def display_island_counts(game):
	'''
	Print summary of the number of islands for the current turn
	'''
	game.debug("Island count:: All Islands: %s, my Islands: %s, "  \
		+ "Enemy Islands: %s, Not my islands: %s, Neutral Islands: %s", \
	len(game.islands()),
	len(game.my_islands()),
	len(game.enemy_islands()),
	len(game.not_my_islands()),
	len(game.neutral_islands()))
