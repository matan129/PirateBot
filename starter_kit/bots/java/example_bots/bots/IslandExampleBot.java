package bots;


import pirates.game.Constants;
import pirates.game.Direction;
import pirates.game.Island;
import pirates.game.Pirate;
import pirates.game.PirateBot;
import pirates.game.PirateGame;

/**
 * 
 * In this example we will see how to use all the API regarding islands.
 * The API returns various list of islands by their state (by whom they are captured), and 
 * information about a specific Island object.
 * We can access islands from the various list, or retrieve it by id if we want to safely access
 * the same island between different turns.
 * This example demonstrates the API by printing informations about the all the islands lists, and assiging
 * a single pirate to capture a target island, printing extended information about that island.   
 * Enjoy the Example!
 
 *
 */
public class MyBot implements PirateBot {
	// Hold the game for the current turn
	private PirateGame game;
	
	// Id of the island to go to 
	private Integer islandIdToCapture = null;
	

	@Override
	public void doTurn(PirateGame game) {
		// Set the game for the current turn
		this.game = game;
		
		displayIslandCounts();
		goToIsland();
	}
	
	/**
	 * Print summary of the number of islands for the current turn
	 */
	private void displayIslandCounts() {
		game.debug("Island count:: All Islands: %s, my Islands: %s, "
				+ "Enemy Islands: %s, Not my islands: %s, Neutral Islands: %s", 
			game.islands().size(),
			game.myIslands().size(),
			game.enemyIslands().size(),
			game.notMyIslands().size(),
			game.neutralIslands().size());
				
	}
	
	
	/**
	 * Send a single pirate to an island not yet captured
	 */
	private void goToIsland() {
		
		// Don't do anything if have not pirates or no islands to capture
		if (game.myPirates().size() == 0 || game.notMyIslands().size() == 0) {
			return;
		}
		
		// Assiging only the first island
		Pirate pirate = game.myPirates().get(0);
		Island targetIsland;
		
		if (islandIdToCapture == null) {
			// No islands yet, getting the first island
			targetIsland = game.notMyIslands().get(0);
			islandIdToCapture = targetIsland.getId(); 
		} else {
			// Already has an island as destination, get it by id
			targetIsland = game.getIsland(islandIdToCapture);
			if (targetIsland.getOwner() == Constants.ME) {
				// The island is already captured by me, get a different island
				game.debug("Captured The island");
				targetIsland = game.notMyIslands().get(0);
			}
		}

		
		if (islandIdToCapture != targetIsland.getId()) {
			// The island to capture has changed, display info about it
			islandIdToCapture = targetIsland.getId();
			showIslandInfo(targetIsland);
			if (targetIsland.getValue() > 1) {
				game.debug("Found the treasure island!");
			}
		}
				
		if (game.getPirateOn(targetIsland) == pirate) {
			if (game.isCapturing(pirate)) {
				// Capturing
				game.debug("I'm capturing!");
			}
		} else {
			// Not on the island yet, sail to it
			Direction direction = game.getDirections(pirate.getLocation(), targetIsland.getLocation()).get(0); 
			game.setSail(pirate, direction);
		}
	}
	
	/**
	 * Print information about a specific island 
	 * @param island
	 */
	private void showIslandInfo(Island island) {
		game.debug("Island: id: %s, location: %s, owner: %s, value: %s," + 
				"Captured by: %s, turns being captured: %s",
				island.getId(),
				island.getLocation(),
				ownerAsString(island.getOwner()),
				island.getValue(),
				ownerAsString(island.getTeamCapturing()),
				island.getTurnsBeingCaptured());
	}

	/**
	 * Convert an owner integer to string
	 * @param owner
	 * @return
	 */
	public static String ownerAsString(int owner) {
		switch (owner) {
		case Constants.ME:
			return "Me";
		case Constants.ENEMY:
			return "Enemy";
		case Constants.NO_OWNER:
			return "No Owner";
		default:
			return "Unknown";
		}
	}
	
	
	
	

}
