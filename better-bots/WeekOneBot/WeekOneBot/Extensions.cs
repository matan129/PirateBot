using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace WeekOneBot
{
    static class Extensions
    {
        /// <summary>
        /// Get a direction to move to
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate">The pirate to move</param>
        /// <param name="loc">The Location to move to</param>
        /// <returns>A direction to the specified location</returns>
        public static List<Direction> GetDirections(this IPirateGame game, int pirate, Location loc)
        {
            return game.GetDirections(game.GetMyPirate(pirate), loc);
        }

        /// <summary>
        /// Move a pirate
        /// </summary>
        /// <param name="game"></param>
        /// <param name="Pirate">An index of one of our pirates to move</param>
        /// <param name="d">The direction to move to</param>
        public static void SetSail(this IPirateGame game, int Pirate, Direction d)
        {
            Bot.Game.SetSail(Bot.Game.GetMyPirate(Pirate),d);
        }


        /// <summary>
        /// Move a pirate
        /// </summary>
        /// <param name="pirate"></param>
        /// <param name="d">The direction to move to</param>
        public static void SetSail(this Pirate pirate, Direction d)
        {
            Bot.Game.SetSail(pirate, d);
        }

        /// <summary>
        /// Move a pirate
        /// </summary>
        /// <param name="pirate"></param>
        /// <param name="d">The direction to move to</param>
        public static void SetSail(this Pirate pirate, Location loc)
        {
            foreach (Location location in pirate.EnumerateLocationsNearPirate())
            {
                if (loc == location)
                {
                    pirate.SetSail(Bot.Game.GetDirections(pirate,loc).First());
                    return;
                }
            }

            throw new LocationNotNearException();
        }

        /// <summary>
        /// Gets the distance from a pirate to a location
        /// </summary>
        /// <param name="game"></param>
        /// <param name="pirate">the pirate</param>
        /// <param name="row">Row coordinates</param>
        /// <param name="col">Column coordinates</param>
        /// <returns>The Distance</returns>
        public static int Distance(this IPirateGame game, Pirate pirate, int row, int col)
        {
            return Bot.Game.Distance(pirate.Loc, new Location(row, col));
        }

        /// <summary>
        /// Get all the location near a pirate
        /// </summary>
        /// <param name="pirate"></param>
        /// <returns>All the locations near the pirate. Can be used in a foreach loop or converted to basically any collection or array</returns>
        public static IEnumerable<Location> EnumerateLocationsNearPirate(this Pirate pirate)
        {
            int R = pirate.Loc.Row;
            int C = pirate.Loc.Col;

            yield return new Location(R + 1,C);
            yield return new Location(R, C + 1);
            yield return new Location(R + 1, C + 1);
            yield return new Location(R - 1, C - 1);
            yield return new Location(R - 1, C);
            yield return new Location(R, C - 1);
        }
        
        /// <summary>
        /// Get all the location near a location
        /// </summary>
        /// <param name="loc"></param>
        /// <returns>All the locations near the specified location. Can be used in a foreach loop or converted to basically any collection or array</returns>
        public static IEnumerable<Location> EnumerateLocationsNearLoc(this Location loc)
        {
            int R = loc.Row;
            int C = loc.Col;

            yield return new Location(R + 1, C);
            yield return new Location(R, C + 1);
            yield return new Location(R + 1, C + 1);
            yield return new Location(R - 1, C - 1);
            yield return new Location(R - 1, C);
            yield return new Location(R, C - 1);
        }

        /// <summary>
        /// Gets a pirate on a location
        /// </summary>
        /// <param name="loc"></param>
        /// <returns>The pirate if there is one or null if the location is empty</returns>
        public static Pirate GetPirateOn(this Location loc)
        {
            return Bot.Game.GetPirateOn(loc);
        }

    }

    internal class LocationNotNearException : Exception
    {
    }
}
