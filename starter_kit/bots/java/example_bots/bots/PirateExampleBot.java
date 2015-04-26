package bots;

import java.util.ArrayList;
import java.util.List;

import pirates.game.Direction;
import pirates.game.Island;
import pirates.game.Pirate;
import pirates.game.PirateBot;
import pirates.game.PirateGame;

/**
 * In this example we will see how to use all the API that deals with the pirate
 * ships First, we will see functions for getting pirates (enemy pirates, my
 * pirates, etc.. ) Then, we will use the properties/members of each pirate
 * Finally, we will show how to make your own function for returning a list of
 * pirates.
 */
public class MyBot implements PirateBot {

	@Override
	public void doTurn(PirateGame game) {
		game.debug("You have %s Pirates in the game", game.allMyPirates()
				.size());
		game.debug("You have %s Pirates on the map this turn", game.myPirates()
				.size());

		game.debug("The enemy has %s Pirates in the game", game
				.allEnemyPirates().size());
		game.debug("The enemy has %s Pirates on the map this turn", game
				.enemyPirates().size());

		// check if we have no more pirates on the map
		if (game.myPirates().size() == 0) {
			game.debug("We have no pirates! nothing to do..");
			return;
		}

		// check if we have less pirates than the enemy
		if (game.enemyPirates().size() > game.myPirates().size()) {
			game.debug("Oh no! There are more enemies on the map!");
		}

		// choose some pirate
		// game.getMyPirate(int id) gets the pirate that has that id.
		Pirate my_pirate = game.getMyPirate(1);
		// choose some enemy pirate
		Pirate enemy = game.getEnemyPirate(2);
		// choose some island
		Island target_island = game.getIsland(2);

		if (my_pirate.isLost()) {
			// our pirate is lost... we shouldn't do anything
			game.debug("Pirate is lost :( ");
			return;
		}

		// print some information about our pirate:
		game.debug(
				"Us >> Id: %s, Owner: %s, Location {2}, InitialLocation: {3}",
				my_pirate.getId(), my_pirate.getOwner(),
				my_pirate.getLocation(), my_pirate.getInitialLocation());
		// print some information about enemy pirate:
		game.debug(
				"Enemy >> Id: %s, Owner: %s, Location {2}, InitialLocation: {3}",
				enemy.getId(), enemy.getOwner(), enemy.getLocation(),
				enemy.getInitialLocation());

		Direction dir;
		// if the enemy isn't lost we will go to it - otherwise we will go to
		// the island
		if (!enemy.isLost()) {
			// enemy is in the game! go to it.
			game.debug("Enemy isnt lost - going to enemy. distance is: %s",
					game.distance(my_pirate, enemy));
			List<Direction> directions = game.getDirections(my_pirate, enemy);
			dir = directions.get(directions.size() - 1);
		} else {
			// enemy isn't in the game. go to the island.
			game.debug(
					"Enemy is lost! and will return in %s turns. Going to island",
					enemy.getTurnsToRevive());
			List<Direction> directions = game.getDirections(my_pirate, enemy);
			dir = directions.get(directions.size() - 1);
		}
		game.setSail(my_pirate, dir);

		// Get the pirate on the island (or null)
		Pirate pirateOnIsland = game.getPirateOn(target_island);

		// check if enemy is on the island
		if (pirateOnIsland != null && pirateOnIsland.equals(enemy)) {
			game.debug("Enemy is on the island!");
		}

		// check if we are on the island
		if (pirateOnIsland != null && pirateOnIsland.equals(my_pirate)) {
			game.debug("My pirate is on the island!");
		}

		// print how many lost pirates we have with the function we write
		game.debug("We have %s lost pirates", this.myLostPirates(game).size());
	}

	// This function will return us a list of our lost pirates
	List<Pirate> myLostPirates(PirateGame game) {

		// Create an empty list
		List<Pirate> lostPirates = new ArrayList<Pirate>();

		// for loop - go through all the pirates in AllMyPirates()
		for (Pirate pirate : game.allMyPirates()) {
			// check if pirate is lost
			if (pirate.isLost()) {
				// add lost pirate to the list
				lostPirates.add(pirate);
			}
		}

		// return the list
		return lostPirates;
	}

}
