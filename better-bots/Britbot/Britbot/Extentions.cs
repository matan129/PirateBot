#region #Usings

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     This class provides various extension methods for the IPirateGame interface
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     Smashes a two dimensional array to a flat one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jaggedAray"></param>
        /// <returns></returns>
        public static T[] Flatten<T>(this T[][] jaggedAray)
        {
            List<T> flatList = new List<T>();

            for (int i = 0; i < jaggedAray.Length; i++)
            {
                for (int k = 0; k < jaggedAray[i].Length; k++)
                {
                    flatList.Add(jaggedAray[i][k]);
                }
            }

            return flatList.ToArray();
        }

        public static int Distance(this IPirateGame game, Location loc, SmartIsland isle)
        {
            return Bot.Game.Distance(loc, isle.Loc);
        }

        /// <summary>
        ///     extention method calculating the square of the euclidian distance between
        ///     two locations
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
        ///     replacement for the function provided since it doesn't account for pirates capturing
        /// </summary>
        /// <param name="game">For the compliler magic</param>
        /// <param name="loc1">first location</param>
        /// <param name="loc2">second location</param>
        /// <returns>true if realy the two point are within range</returns>
        public static bool IsReallyInRange(this IPirateGame game, Location loc1, Location loc2)
        {
            return game.EuclidianDistanceSquared(loc1, loc2) <= game.GetAttackRadius();
        }

        /// <summary>
        ///     This function checks if it is possible for a given group to be in
        ///     certain location without pirates getting in enemy zone
        /// </summary>
        /// <param name="game">Compiler magic</param>
        /// DAMM RIGHT IT IS
        /// <param name="loc">the location of the 0 ring (the one we check)</param>
        /// <param name="group">the group trying to pass</param>
        /// <returns>true if it is possible to put this group in this location</returns>
        public static bool IsPassableEnough(this IPirateGame game, Location loc, Group group)
        {
            //Logger.BeginTime("IsPassableEnough");
            //going over all the pirates in the group
            foreach (int pirate in group.Pirates)
            {
                //calculate differance vector between the Group's center and the given pirate
                HeadingVector difference = HeadingVector.CalcDifference(group.FindCenter(true)
                    , Bot.Game.GetMyPirate(pirate).Loc);

                //calculate the location of this pirate if the group is placed in loc
                Location newLocation = HeadingVector.AddvanceByVector(loc, difference);

                //check if isn't passable, if so return false
                if (!game.IsInMap(newLocation) || !game.IsPassable(newLocation))
                    return false;
            }
            //Logger.StopTime("IsPassableEnough");
            //otherwise return true since all the pirates can fit here
            return true;
            /*
            //go over the locations and check if they are passable.
            for (int deltaX = -passRadius; deltaX <= passRadius; deltaX++)
            {
                for (int deltaY = -passRadius + Math.Abs(deltaX); deltaY <= passRadius - Math.Abs(deltaX); deltaY++)
                {
                    Location testLocation = new Location(loc.Row + deltaY, loc.Col + deltaX);

                    //check if passable
                    if (!game.IsInMap(testLocation) || !game.IsPassable(testLocation))
                        return false;
                }
            }
            */
            //return true if Ok
        }

        public static bool IsInMap(this IPirateGame game, Location testLocation)
        {
            int mapRow = game.GetRows();
            int mapCol = game.GetCols();

            //check if outside of the map
            if (testLocation.Row >= mapRow || testLocation.Col >= mapCol ||
                testLocation.Row < 0 || testLocation.Col < 0)
                return false;

            return true;
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

        public static void AddRange<TSource>(this ObservableCollection<TSource> source, IEnumerable<TSource> items)
        {
            foreach (var item in items)
            {
                source.Add(item);
            }
        }

        public static void RemoveAll<T>(this ObservableCollection<T> collection, Func<T, bool> condition)
        {
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (condition(collection[i]))
                {
                    collection.RemoveAt(i);
                }
            }
        }
    }

}