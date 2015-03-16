package bots;

import java.util.List;

import pirates.game.Direction;
import pirates.game.Island;
import pirates.game.Location;
import pirates.game.Pirate;
import pirates.game.PirateBot;
import pirates.game.PirateGame;


/**
 * 
 * This example demonstrates API usage related to locations and movement.
 * It will demonstrate how to use different methods to get information a locations and pairs of
 * locations and how to manipulate and calculate locations.
 * It will be demonstrated by a bot which controls a single pirate, and sends it to a different 
 * destination according to the turn number (i.e., other pirate, enemy pirate, island, 
 * or user-defined location).
 * The movement will be done by through a movement which will check the destination and check if 
 * and how to move there. 
 * Note that this movement function is not the optimal for an actual bot, and is intended for 
 * demonstrating the API usage.
 
 */
public class MyBot implements PirateBot {

	@Override
	public void doTurn(PirateGame game) {
		Pirate pirate = game.allMyPirates().get(0);
		
		if (game.getTurn() < 5) {
			// Try to sail to another pirate
			Pirate otherPirate = game.allMyPirates().get(1);
			game.debug("Distance to other pirate %s", game.distance(pirate, otherPirate));
			tryToSetSail(game, pirate, otherPirate.getLocation());
		} else if (game.getTurn() < 20) {
			// Sail to a location relative to the other pirate
			Pirate otherPirate = game.allMyPirates().get(1);
			// Create a new location based on the other pirate location
			Location nearOtherPirate = new Location(
					otherPirate.getLocation().row + 10,
					otherPirate.getLocation().col + 10);
			
			game.debug("Distance to other pirate: %s", game.distance(pirate, otherPirate));
			game.debug("Distance to target location: %s",
					game.distance(pirate.getLocation(), nearOtherPirate));
			
			// When going out of the other pirate's support range
			if (!game.InRange(pirate.getLocation(), otherPirate.getLocation())) {
				game.debug("Out of other pirate support range");
			}
			tryToSetSail(game, pirate, nearOtherPirate);
		} else if (game.getTurn() < 90) {
			// Sail to an island
			Island island = game.islands().get(0);
			game.debug("Distance to island %s", game.distance(pirate, island));
			tryToSetSail(game, pirate, island.getLocation());
		} else if (game.getTurn() < 400) {
			// Sail to an enemy
			Pirate enemyPirate = game.allEnemyPirates().get(0);
			game.debug("Distance to enemy %s", game.distance(pirate, enemyPirate));
			tryToSetSail(game, pirate, enemyPirate.getLocation());
		} else {
			// Sail to center of the map
			// calcualte a new location by the number of the rows and cols in the map.
			Location center = new Location(game.getRows() / 2, game.getCols() / 2);
			game.debug("Distance to center %s", game.distance(pirate.getLocation(), center));
			tryToSetSail(game, pirate, center);
		}
	}

	/**
	 * Try to sail to a destination if:
	 *  * Pirate is not lost
	 *  * Not going to sail to where another pirate currently is (try different directions)
	 *  * Not already in destination
	 *  * The location we're moving to next turn is passable
	 *  * Not entering enemy pirate range
	 *  * 
	 * 
	 * 
	 * @param game 
	 * @param pirate pirate to move
	 * @param destination destination to get to
	 */
	private void tryToSetSail(PirateGame game, Pirate pirate, Location destination) {

		if (pirate.isLost()) {
			// Pirate is lost, can't sail
			game.debug("A lost pirate can't sail!");
			return;
		}

		// NOTE: check location equality using equals and not using ==
		if (pirate.getLocation().equals(destination)) {
			// No point in moving if already at the destination
			game.debug("Arrived at destination!");
			return;
		}
		
		// Get the first direction toward the destination
		List<Direction> directions = game.getDirections(pirate, destination);
		Direction direction = directions.get(0);
		// Get the location on the next turn if we'll sail in that direction
		Location locationNextTurn = game.destination(pirate, direction);
		// If there's a pirate on the destination, don't sail there to avoid collision,
		// and try to use a different direction
		if (directions.size() > 1 && game.getPirateOn(locationNextTurn) != null) {
			game.debug("Almost moved to an occupied location! Trying alternative direction");
			// Use the other direction
			direction = directions.get(1);
			// recalculate the destination of moving in that direction on the next turn
			locationNextTurn = game.destination(pirate, direction);
		}
		
		// If the location is not on the map or in enemy zone, don't move there
		if (!game.isPassable(locationNextTurn)) {
			game.debug("Oops, can't go to %s, not passable", locationNextTurn);
			return;
		}
		
		// If the location is not on the map or in enemy zone, don't move there
		if (game.getPirateOn(locationNextTurn) != null) {
			game.debug("Oops, can't go to %s, there's a pirate there", locationNextTurn);
			return;
		}
		
		// Check for all (non-lost) enemy pirates if the movement will get us in their attack range
		// If it will, avoid conflict like a real man!
		for (Pirate enemyPirate: game.enemyPirates()) {
			if (game.inRange(enemyPirate, locationNextTurn)) {
				game.debug("Can't move there, enemy pirate is in range! My location: %s, his: %s",
						locationNextTurn, enemyPirate.getLocation());
				return;
			}
		}
		
		// If we got here, actually sail to the destination
		game.setSail(pirate, direction);
	}
}
