using System;
using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace LocationExample
{
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
    public class LocationExample : Pirates.IPirateBot
    {

        public void DoTurn(IPirateGame game)
        {
            Pirate pirate = game.AllMyPirates()[0];

            if (game.GetTurn() < 5)
            {
                // Try to sail to another pirate
                Pirate otherPirate = game.AllMyPirates()[1];
                game.Debug("Distance to other pirate %s", game.Distance(pirate, otherPirate));
                tryToSetSail(game, pirate, otherPirate.Loc);
            }
            else if (game.GetTurn() < 20)
            {
                // Sail to a location relative to the other pirate
                Pirate otherPirate = game.AllMyPirates()[1];
                // Create a new location based on the other pirate location
                Location nearOtherPirate = new Location(
                        otherPirate.Loc.Row + 10,
                        otherPirate.Loc.Col + 10);

                game.Debug("Distance to other pirate: %s", game.Distance(pirate, otherPirate));
                game.Debug("Distance to target location: %s",
                        game.Distance(pirate.Loc, nearOtherPirate));

                // When going out of the other pirate's support range
                if (!game.InRange(pirate.Loc, otherPirate.Loc))
                {
                    game.Debug("Out of other pirate support range");
                }
                tryToSetSail(game, pirate, nearOtherPirate);
            }
            else if (game.GetTurn() < 90)
            {
                // Sail to an island
                Island island = game.Islands()[0];
                game.Debug("Distance to island %s", game.Distance(pirate, island));
                tryToSetSail(game, pirate, island.Loc);
            }
            else if (game.GetTurn() < 400)
            {
                // Sail to an enemy
                Pirate enemyPirate = game.AllEnemyPirates()[0];
                game.Debug("Distance to enemy %s", game.Distance(pirate, enemyPirate));
                tryToSetSail(game, pirate, enemyPirate.Loc);
            }
            else
            {
                // Sail to center of the map
                // calcualte a new location by the number of the rows and cols in the map.
                Location center = new Location(game.GetRows() / 2, game.GetCols() / 2);
                game.Debug("Distance to center %s", game.Distance(pirate.Loc, center));
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
        private void tryToSetSail(IPirateGame game, Pirate pirate, Location destination)
        {

            if (pirate.IsLost)
            {
                // Pirate is lost, can't sail
                game.Debug("A lost pirate can't sail!");
                return;
            }

            // NOTE: check location equality using equals and not using ==
            if (pirate.Loc.Equals(destination))
            {
                // No point in moving if already at the destination
                game.Debug("Arrived at destination!");
                return;
            }

            // Get the first direction toward the destination
            List<Direction> directions = game.GetDirections(pirate, destination);
            Direction direction = directions[0];
            // Get the location on the next turn if we'll sail in that direction
            Location locationNextTurn = game.Destination(pirate, direction);
            // If there's a pirate on the destination, don't sail there to avoid collision,
            // and try to use a different direction
            if (directions.Count() > 1 && game.GetPirateOn(locationNextTurn) != null)
            {
                game.Debug("Almost moved to an occupied location! Trying alternative direction");
                // Use the other direction
                direction = directions[1];
                // recalculate the destination of moving in that direction on the next turn
                locationNextTurn = game.Destination(pirate, direction);
            }

            // If the location is not on the map or in enemy zone, don't move there
            if (!game.IsPassable(locationNextTurn))
            {
                game.Debug("Oops, can't go to %s, not passable", locationNextTurn);
                return;
            }

            // If the location is not on the map or in enemy zone, don't move there
            if (game.GetPirateOn(locationNextTurn) != null)
            {
                game.Debug("Oops, can't go to %s, there's a pirate there", locationNextTurn);
                return;
            }

            // Check for all (non-lost) enemy pirates if the movement will get us in their attack range
            // If it will, avoid conflict like a real man!
            foreach (Pirate enemyPirate in game.EnemyPirates())
            {
                if (game.InRange(enemyPirate, locationNextTurn))
                {
                    game.Debug("Can't move there, enemy pirate is in range! My location: %s, his: %s",
                            locationNextTurn, enemyPirate.Loc);
                    return;
                }
            }

            // If we got here, actually sail to the destination
            game.SetSail(pirate, direction);
        }
    }
}
