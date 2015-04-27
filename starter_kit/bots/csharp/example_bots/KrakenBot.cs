using System;
using System.Collections.Generic;
using System.Linq;
using Pirates;


namespace HunterBot
{
    public class KrakenBot : Pirates.IPirateBot
	{

        public void DoTurn(IPirateGame game)
        {
            Kraken kraken = game.GetKraken();
		
		    if (kraken == null) {
			    game.Debug("No Kraken!");
	            return;
		    }
		
		if (game.GetTurn() == 1) {
            game.Debug("Playing vs {0}", game.GetOpponentName());
            game.Debug("Kraken settings: Turns per move: {0}, awake turns: {1}, sleep turns: {2}, vanished turns: {3}",
	                game.GetKrakenTurnsPerMove(),
	                game.GetKrakenAwakeTurns(),
	                game.GetKrakenSleepTurns(),
	                game.GetKrakenVanishedTurns());
		}
        game.Debug("Kraken: {0}", kraken);
        
		if (kraken.State == KrakenState.VANISHED) {
			game.Debug("WHERE DID THE KRAKEN GO???");
		}
		
		int numPirates = game.MyPirates().Count;
		int totalPirates = game.AllMyPirates().Count;
		if (numPirates == 0) {
			return;
		}

        foreach (Pirate pirate in game.MyPirates())
        {
            if (pirate.Id == 0)
            {
                if (kraken.IsAsleep)
                {
                    if (kraken.Loc.Equals(pirate.Loc))
                    {
                        game.UnleashTheKraken(pirate, new Location(10, 10));
                    }
                    else
                    {
                        List<Direction> dirs = game.GetDirections(pirate, kraken.Loc);
                        game.SetSail(pirate, dirs[0]);
                    }
                }

                continue;
            }

            // Just arrange the other pirates to see that the kraken approach them
            int row = game.GetRows() / totalPirates * pirate.Id;
            if (pirate.Id % 2 == 0)
            {
                // Put some of the pirates inverse locations so they wouldn't be on the same line;
                row = game.GetRows() - 1 - game.GetRows() / totalPirates * pirate.Id;
            }
            int col = game.GetCols() / totalPirates * pirate.Id;

            Location dest = new Location(row, col);
            game.Debug("sending pirate {0} to {1}", pirate.Id, dest);
            List<Direction> directions = game.GetDirections(pirate, dest);
            Location destinationLoc = game.Destination(pirate, directions[0]);
            if (!game.SafeFromKraken(destinationLoc))
            {
                game.Debug("Sending pirate {0} to possible death...", pirate.Id);
            }
            game.SetSail(pirate, directions[0]);
        }
        }
	}
}

