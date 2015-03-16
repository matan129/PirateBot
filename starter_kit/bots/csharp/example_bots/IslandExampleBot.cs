using System;
using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace IslandExample
{
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
	public class IslandExample : Pirates.IPirateBot
	{

	// Hold the game for the current turn
	private IPirateGame game;

	// Id of the island to go to
	private int? islandIdToCapture = null;


    public void DoTurn(IPirateGame game) {
		// Set the game for the current turn
		this.game = game;

		this.displayIslandCounts();
		this.goToIsland();
	}

	/**
	 * Print summary of the number of islands for the current turn
	 */
	private void displayIslandCounts() {
		game.Debug("Island count:: All Islands: {0}, my Islands: {1}, "
				+ "Enemy Islands: {2}, Not my islands: {3}, Neutral Islands: {4}",
			game.Islands().Count(),
			game.MyIslands().Count(),
			game.EnemyIslands().Count(),
			game.NotMyIslands().Count(),
			game.NeutralIslands().Count());

	}


	/**
	 * Send a single pirate to an island not yet captured
	 */
	private void goToIsland() {

		// Don't do anything if have not pirates or no islands to capture
		if (game.MyPirates().Count() == 0 || game.NotMyIslands().Count() == 0) {
			return;
		}

		// Assiging only the first island
		Pirate pirate = game.MyPirates()[0];
		Island targetIsland;

		if (islandIdToCapture == null) {
			// No islands yet, getting the first island
			targetIsland = game.NotMyIslands()[0];
			islandIdToCapture = targetIsland.Id;
		} else {
			// Already has an island as destination, get it by id
			targetIsland = game.GetIsland(islandIdToCapture.Value);
			if (targetIsland.Owner == Consts.ME) {
				// The island is already captured by me, get a different island
				game.Debug("Captured The island");
				targetIsland = game.NotMyIslands()[0];
			}
		}


		if (islandIdToCapture != targetIsland.Id) {
			// The island to capture has changed, display info about it
			islandIdToCapture = targetIsland.Id;
			showIslandInfo(targetIsland);
			if (targetIsland.Value > 1) {
				game.Debug("Found the treasure island!");
			}
		}

		if (game.GetPirateOn(targetIsland) == pirate) {
			if (game.isCapturing(pirate)) {
				// Capturing
				game.Debug("I'm capturing!");
			}
		} else {
			// Not on the island yet, sail to it
			Direction direction = game.GetDirections(pirate.Loc, targetIsland.Loc)[0];
			game.SetSail(pirate, direction);
		}
	}

	/**
	 * Print information about a specific island
	 * @param island
	 */
	private void showIslandInfo(Island island) {
		game.Debug("Island: id: {0}, location: {1}, owner: {2}, points: {3}," +
				"Captured by: {4}, turns being captured: {5}",
				island.Id,
				island.Loc,
				island.Owner,
				island.Value,
				island.TeamCapturing,
				island.TurnsBeingCaptured);
	}
}
}
