package bots;


import pirates.game.Constants;
import pirates.game.Kraken;
import pirates.game.KrakenState;
import pirates.game.Pirate;
import pirates.game.PirateBot;
import pirates.game.PirateGame;
import pirates.game.Location;
import pirates.game.Direction;

import java.util.List;

public class MyBot implements PirateBot{

	@Override
	public void doTurn(PirateGame game) {
		Kraken kraken = game.getKraken();
		
		if (kraken == null) {
			game.debug("No Kraken!");
	        return;
		}
		
		if (game.getTurn() == 1) {
	        game.debug("Kraken settings: Turns per move: %s, awake turns: %s, sleep turns: %s, vanished turns: %s",
	                game.getKrakenTurnsPerMove(),
	                game.getKrakenAwakeTurns(),
	                game.getKrakenSleepTurns(),
	                game.getKrakenVanishedTurns());
		}
		
		game.debug("Kraken: %s. Can move: %s", kraken, kraken.canMove());
		
		if (kraken.getState() == KrakenState.VANISHED) {
			game.debug("WHERE DID THE KRAKEN GO???");
		}
		
		int numPirates = game.myPirates().size();
		int totalPirates = game.allMyPirates().size();
		if (numPirates == 0) {
			return;
		}
		
		for (Pirate pirate: game.myPirates()) {
			if (pirate.getId() == 0) {
				if (kraken.isAsleep()) {
					if (kraken.getLocation().equals(pirate.getLocation())) {
						game.unleashTheKraken(pirate, new Location(10, 10));
					} else {
						List<Direction> directions = game.getDirections(pirate, kraken.getLocation());
						game.setSail(pirate, directions.get(0));
					}
				}
				
				continue;
			}
			
			// Just arrange the other pirates to see that the kraken approach them
			int row = game.getRows() / totalPirates * pirate.getId();
			if (pirate.getId() % 2 == 0) {
				// Put some of the pirates inverse locations so they wouldn't be on the same line;
				row = game.getRows() - 1 - game.getRows() / totalPirates * pirate.getId();
			}
			int col = game.getCols() / totalPirates * pirate.getId();
			
			Location dest = new Location(row, col);
			game.debug("sending pirate %s to %s", pirate.getId(), dest);
			List<Direction> directions = game.getDirections(pirate, dest);
			Location destinationLoc = game.destination(pirate, directions.get(0));
			if (!game.safeFromKraken(destinationLoc)) {
				game.debug("Sending pirate %s to possible death...", pirate.getId());
			}
			game.setSail(pirate, directions.get(0));

		}
		
	}

}
