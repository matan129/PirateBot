using System.Collections.Generic;
using Pirates;

namespace Britbot
{
    /// <summary>
    /// This class provied various extension methods for the IPirateGame interface
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets a SmartIsland by index, as we would do with a normal Island
        /// </summary>
        /// <param name="game"></param>
        /// <param name="id">The SmartIsland's ID, which is equal to the ID of the normal island it encapsulates</param>
        /// <returns>The SmartIsland with the relevant ID</returns>
        public static SmartIsland GetSmartIsland(this IPirateGame game, int id)
        {
            return SmartIsland.IslandList.Find(isle => isle.Id == id);
        }

        /// <summary>
        /// Gets the list of SmartIsland like we would normally do with normal Islands
        /// </summary>
        /// <param name="game"></param>
        /// <returns>A list containing all the SmartIslands in game</returns>
        public static List<SmartIsland> SmartIslands(this IPirateGame game)
        {
            return SmartIsland.IslandList;
        }

        /// <summary>
        /// Gets a list of directions to move from a pirate to another pirate
        /// Note that this method is NOT smart, its output is location only based
        /// </summary>
        /// <param name="game"></param>
        /// <param name="a">the moving pirate</param>
        /// <param name="b">the target pirate</param>
        /// <returns>A list of possible direction for the target</returns>
        public static List<Direction> GetDirections(this IPirateGame game, Pirate a, Pirate b)
        {
            return Bot.Game.GetDirections(a.Loc, b.Loc);
        }
    }
}