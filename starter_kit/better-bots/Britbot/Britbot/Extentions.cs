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
        /// <param name="Id">The SmartIsland's ID, which is equal to the ID of the normal island it encapsulates</param>
        /// <returns>The SmartIsland with the relevant ID</returns>
        public static SmartIsland GetSmartIsland(this IPirateGame game, int Id)
        {
            return SmartIsland.IslandList.Find(isle => isle.Id == Id);
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
    }
}