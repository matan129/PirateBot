#region Usings
using System;
using System.Collections.Generic;
using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     This class provied various extension methods for the IPirateGame interface
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     Gets a SmartIsland by index, as we would do with a normal Island
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
        /// extention method calculating the square of the euclidian distance between
        /// two locations
        /// </summary>
        /// <param name="game">For the compliler magic</param>
        /// <param name="loc1">first location</param>
        /// <param name="loc2">second location</param>
        /// <returns>regular distance squered</returns>
        public static double EuclidianDistanceSquared(this IPirateGame game, Location loc1, Location loc2)
        {
            return Math.Pow(loc1.Col - loc2.Col, 2) + Math.Pow(loc1.Row - loc2.Row, 2);
        }

        /// <summary>
        /// replacement for the function provided since it doesn't account for pirates capturing
        /// </summary>
        /// <param name="game">For the compliler magic</param>
        /// <param name="loc1">first location</param>
        /// <param name="loc2">second location</param>
        /// <returns>true if realy the two point are within range</returns>
        public static bool IsReallyInRange(this IPirateGame game, Location loc1, Location loc2)
        {
            return game.EuclidianDistanceSquared(loc1, loc2) < game.GetAttackRadius();
        }

        /// <summary>
        ///     Gets the list of SmartIsland like we would normally do with normal Islands
        /// </summary>
        /// <param name="game"></param>
        /// <returns>A list containing all the SmartIslands in game</returns>
        public static List<SmartIsland> SmartIslands(this IPirateGame game)
        {
            return SmartIsland.IslandList;
        }

        /// <summary>
        /// This function checks if it is bossible to put a group of given radious with a given radious in 
        /// curtain location without pirates getting in enemy zone
        /// </summary>
        /// <param name="game">Compiler magic</param>
        /// <param name="loc">the location of the 0 ring (the one we check)</param>
        /// <param name="passRadious">the radius of the group (amount of rings)</param>
        /// <returns>true if it is possible to put a group with passRadious rings in this location</returns>
        public static bool IsPassableEnough(this IPirateGame game, Location loc, int passRadious)
        {
            //going over all close location looking for impassable location
            //iterating over x distances from loc (from -passRadius to +passRadiou) 
            for (int x = -passRadious; x <= passRadious; x++)
            {
                //iterating over y values within reasenable distance (from -Math.abs(passRadious - x) to +Math.abs(passRadious - x)
                for (int y = -Math.Abs(passRadious - x); y <= Math.Abs(passRadious - x); y++)
                {
                    //create the current location
                    Location currLoc = new Location(loc.Row + y, loc.Col + x);

                    //if current location isn't passable return false
                    if (!Bot.Game.IsPassable(currLoc))
                        return false;
                }
            }
            //if here then all the area is passible
            return true;
        }


        /// <summary>
        ///     Gets a list of directions to move from a pirate to another pirate
        ///     Note that this method is NOT smart, its output is location only based
        /// </summary>
        /// <param name="game"></param>
        /// <param name="a">the moving pirate</param>
        /// <param name="b">the target pirate</param>
        /// <returns>A list of possible direction for the target</returns>
        public static List<Direction> GetDirections(this IPirateGame game, Pirate a, Pirate b)
        {
            return Bot.Game.GetDirections(a.Loc, b.Loc);
        }

        /// <summary>
        /// Returns all the friend pirates (that is, our pirates) that are in support range of the pirate
        /// </summary>
        /// <param name="pirate"></param>
        /// <returns></returns>
        public static IEnumerable<Pirate> InRangeFriends(this Pirate pirate)
        {
            foreach (Pirate myPirate in Bot.Game.AllMyPirates())
            {
                if (!myPirate.IsLost && myPirate.Id != pirate.Id && Bot.Game.InRange(pirate, myPirate))
                    yield return myPirate;
            }
        }

        /// <summary>
        ///     Tests if location is actually passable
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public static bool IsActuallyPassable(this Location loc)
        {
            return !Bot.Game.IsOccupied(loc) && Bot.Game.IsPassable(loc);
        }

        /// <summary>
        ///     Adds two locations togther
        /// </summary>
        /// <param name="loc1"></param>
        /// <param name="loc2"></param>
        /// <returns></returns>
        public static Location Add(this Location loc1, Location loc2)
        {
            return new Location(loc1.Row + loc2.Row, loc2.Col + loc2.Col);
        }

        /// <summary>
        ///     Subtracts a location from the location calling this method
        /// </summary>
        /// <param name="loc1"></param>
        /// <param name="loc2"></param>
        /// <returns></returns>
        public static Location Subtract(this Location loc1, Location loc2)
        {
            return new Location(loc1.Row - loc2.Row, loc2.Col - loc2.Col);
        }

        /// <summary>
        ///     Moves a location closer to the center of the map
        /// </summary>
        /// <param name="pivot"></param>
        /// <returns></returns>
        public static Location AdvancePivot(this Location pivot)
        {
            //this basically moves the location closer to the center of the map
            int maxCols = Bot.Game.GetCols();
            int maxRows = Bot.Game.GetRows();

            int addCol = 0, addRow = 0;
            int deltaCol = maxCols - pivot.Col;
            int deltaRow = maxRows - pivot.Row;

            if (deltaCol > pivot.Col)
                addCol++;
            else if (deltaCol < pivot.Col)
                addCol--;

            if (deltaRow > pivot.Row)
                addRow++;
            else if (deltaRow < pivot.Row)
                addRow--;

            pivot = new Location(pivot.Row + addRow, pivot.Col + addCol);
            return pivot;
        }
    }
}