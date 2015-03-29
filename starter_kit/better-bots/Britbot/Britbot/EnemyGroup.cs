﻿using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Britbot
{
    /// <summary>
    /// A class which represents an enemy group
    /// </summary>
    public class EnemyGroup : ITarget
    {
        /// <summary>
        /// What is this?
        /// </summary>
        public Location PrevLoc { get; set; }

        /// <summary>
        /// List of pirate indexes in this group
        /// </summary>
        public List<int> EnemyPirates { get; private set; }

        /// <summary>
        /// The direction this group's heading to 
        /// </summary>
        public HeadingVector Heading { get; private set; }

        /// <summary>
        /// Gets the score for this group
        /// </summary>
        /// <param name="origin">The group requesting the evaluation</param>
        /// <returns>The score for this group</returns>
        public Score GetScore(Group origin)
        {
            //Reduce the score in proportion to distance
            //lower score is worse. Mind the minus sign!
            int distance = InRangeGroupDistance(this, origin);
            int scoreVal = -distance;

            distance +=  + 2*Bot.Game.GetAttackRadius();

            /*
             * if the score requesting group is bigger then this enemy group, add a bunch of points because killing enemy
             * ships is awesome. Otherwise, reduce lots of point because the origin group does not want to die!
             */
            scoreVal += 25*(origin.Pirates.ConvertAll(p => Bot.Game.GetMyPirate(p)).Count(p => !p.IsLost)) -
                        this.EnemyPirates.ConvertAll(e => Bot.Game.GetEnemyPirate(e)).Count(e => !e.IsLost);

            //return new score instance with the relevant information
            return new Score(this, TargetType.EnemyGroup, scoreVal, distance);
        }

        /// <summary>
        /// Returns the average location for this group
        /// </summary>
        /// <returns>Returns the average location for this group</returns>
        public Location GetLocation()
        {
            //Get a list of all location of the enemy pirates in this group
            List<Location> locs =
                this.EnemyPirates.ConvertAll(e => Bot.Game.GetEnemyPirate(e))
                    .Select(p => p.Loc)
                    .Concat(new Location[] {}).ToList();

            //sum all the locations!
            int totalCol = locs.Sum(loc => loc.Col);
            int totalRow = locs.Sum(loc => loc.Row);

            //return the average location
            return new Location(totalCol/locs.Count, totalRow/locs.Count);
        }

        /// <summary>
        /// This implements the GetDirection of the ITarget interface
        /// it returns the best direction to keep the given group (which asked for directions)
        /// in a path perpendicular to the direction of the enemy ship thus ensuring
        /// that it will reach it as soon as possible
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public Direction GetDirection(Group group)
        {
            //calculates the direction besed on the geographical data from the game
            return HeadingVector.CalculateDirectionToMovingTarget(group.GetLocation(), group.Heading, GetLocation(),
                Heading);
        }

        /// <summary>
        /// Determined the minimal time (or distance) between a Group and an 
        /// EnemyGroup before they will get into each others' attack radius.
        /// </summary>
        /// <param name="eg">The EnemyGroup</param>
        /// <param name="group">The Group</param>
        /// <returns>The minimal time before they'd get into each others' range</returns>
        private int InRangeGroupDistance(EnemyGroup eg, Group group)
        {
            Pirate enemyPirate = null, myPirate = null;

            int minDistance = Bot.Game.GetCols() + Bot.Game.GetRows();

            //find the two pirate from the two group with the minimum distance between
            foreach (Pirate p in eg.EnemyPirates.ConvertAll(ep => Bot.Game.GetEnemyPirate(ep)))
            {
                foreach (Pirate aPirate in group.Pirates.ConvertAll(pir => Bot.Game.GetMyPirate(pir)))
                {
                    int distance = Bot.Game.Distance(p, aPirate);

                    if (distance >= minDistance) continue;

                    minDistance = distance;
                    enemyPirate = p;
                    myPirate = aPirate;
                }
            }

            //return the distance between these pirates with the range in mind
            return Bot.Game.Distance(enemyPirate.Loc, myPirate.Loc) - Bot.Game.GetAttackRadius()*2;
        }

        /// <summary>
        /// Determines if an enemy pirate belongs to this enemy group.
        /// </summary>
        /// <param name="enemyPirate">The index of the enemy pirate</param>
        /// <returns>True if the pirate belongs to the group or false, otherwise</returns>
        public bool IsInGroup(int enemyPirate)
        {
            Pirate ePirate = Bot.Game.GetEnemyPirate(enemyPirate);

            //Check if the given pirate is close (max of 2 distance units) to any of the pirates already in this group
            return
                this.EnemyPirates.ConvertAll(e => Bot.Game.GetEnemyPirate(e))
                    .Select(ep => Bot.Game.Distance(ep, ePirate))
                    .Concat(new int[] {})
                    .Min() <= 2;
        }

        /// <summary>
        /// Gets the minimal distance from this enemy group to a location
        /// </summary>
        /// <param name="location">the location to test for</param>
        /// <returns>The minimal distance from this group to that location</returns>
        public int MinimalDistanceTo(Location location)
        {
            return
                this.EnemyPirates.ConvertAll(p => Bot.Game.GetEnemyPirate(p))
                    .Select(pirate => Bot.Game.Distance(pirate.Loc, location))
                    .Concat(new int[] {})
                    .Min();
        }

        /// <summary>
        /// Tries to determine the island target of this group from its HeadingVector.
        /// </summary>
        /// <returns>A SmartIsland if its probably the target or null if no island found</returns>
        public SmartIsland GuessTarget()
        {
            List<SmartIsland> sortedByDistance = SmartIsland.IslandList;
            sortedByDistance.Sort(
                (a, b) => Bot.Game.Distance(b.Loc, GetLocation()).CompareTo(Bot.Game.Distance(a.Loc, GetLocation())));


            //Should be tested because magic numbers aren't a good habit
            const int toleranceMargin = 2;

            foreach (SmartIsland isle in sortedByDistance)
            {
                //check if distance is smaller then tolerance margin
                if (HeadingVector.CalcDistFromLine(isle.GetLocation(), GetLocation(), Heading) < toleranceMargin)
                    return isle;
            }
            /* I THINK I HAVE A BETTER Solution THEN THIS
            //Go over each location determined by the heading vector
            foreach (Location loc in this.Heading.EnumerateLocations(this.GetLocation()))
            {
                //sort the smart islands by distance to this group's location. closer is better.
                sortedByDistance.Sort(
                    (a, b) => Bot.Game.Distance(b.Loc, loc).CompareTo(Bot.Game.Distance(a.Loc, loc)));

                //go over each island in the sorted list
                foreach (SmartIsland island in sortedByDistance)
                {
                    // Check if the island is in the direction of the vector. 
                    // If true, return it since it's the closest island that is in our direction
                    if (Bot.Game.Distance(island.Loc, loc) < toleranceMargin)
                    {
                        return island;
                    }
                }
            }*/


            return null;
        }

        /// <summary>
        /// This method updates Enemy's group direction and previous place (for the next turn calculations)
        /// This method is called by the enemy class only!!!
        /// </summary>
        public void UpdateHeading()
        {
            //get the new direction of the last turn
            Direction newDir = Bot.Game.GetDirections(PrevLoc, GetLocation())[0];

            //update previous location
            PrevLoc = GetLocation();

            //update direction
            Heading += newDir;
        }
    }
}