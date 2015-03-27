using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace SarcasticBot
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
    }
}
