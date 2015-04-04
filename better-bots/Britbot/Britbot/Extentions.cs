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

        public static int Distance(this IPirateGame game, Location loc, SmartIsland isle)
        {
            return Bot.Game.Distance(loc, isle.Loc);
        }


        public static Direction Oppsite(this Direction dir)
        {
            switch (dir)
            {
                case Direction.EAST:
                    return Direction.WEST;
                    break;
                case Direction.NORTH:
                    return Direction.SOUTH;
                    break;
                case Direction.WEST:
                    return Direction.EAST;
                    break;
                case Direction.SOUTH:
                    return Direction.NORTH;
                    break;
                default:
                    return Direction.NOTHING;
            }
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

        public static IEnumerable<Pirate> InRangeFriends(this Pirate pirate)
        {
            foreach (Pirate myPirate in Bot.Game.AllMyPirates())
            {
                if (Bot.Game.InRange(pirate, myPirate))
                    yield return myPirate;
            }
        }

        /// <summary>
        /// Tests if location is actually passable
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public static bool IsActuallyPassable(this Location loc)
        {
            return !Bot.Game.IsOccupied(loc) && Bot.Game.IsPassable(loc);
        }

        /// <summary>
        /// Adds two locations togther
        /// </summary>
        /// <param name="loc1"></param>
        /// <param name="loc2"></param>
        /// <returns></returns>
        public static Location Add(this Location loc1, Location loc2)
        {
            return new Location(loc1.Row + loc2.Row, loc2.Col + loc2.Col);
        }

        /// <summary>
        /// Subtracts a location from the location calling this method
        /// </summary>
        /// <param name="loc1"></param>
        /// <param name="loc2"></param>
        /// <returns></returns>
        public static Location Subtract(this Location loc1, Location loc2)
        {
            return new Location(loc1.Row - loc2.Row, loc2.Col - loc2.Col);
        }
    }
}