#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     A class which represents an enemy group
    /// </summary>
    public class EnemyGroup : ITarget
    {
        #region Static Fields & Consts

        public static int IdCount;

        #endregion

        #region Fields & Properies

        /// <summary>
        ///     unique-ish id for the enemy group
        /// </summary>
        public readonly int Id;

        /// <summary>
        ///     What is this?
        /// </summary>
        public Location PrevLoc { get; set; }

        /// <summary>
        ///     List of pirate indexes in this group
        /// </summary>
        public List<int> EnemyPirates { get; private set; }

        /// <summary>
        ///     The direction this group's heading to
        /// </summary>
        public HeadingVector Heading { get; private set; }

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     Creates a new instance of the EnemyGroup class
        /// </summary>
        public EnemyGroup()
        {
            this.Id = EnemyGroup.IdCount++;
            this.EnemyPirates = new List<int>();
            this.PrevLoc = new Location(0, 0);
            this.Heading = new HeadingVector();
        }

        /// <summary>
        ///     Creates a new instance of the EnemyGroup class
        /// </summary>
        public EnemyGroup(Location prevLoc, List<int> enemyPirates, HeadingVector heading)
        {
            this.Id = EnemyGroup.IdCount++;
            PrevLoc = prevLoc;
            EnemyPirates = enemyPirates;
            Heading = heading;
        }

        #endregion

        #region Interface Implementations

        /// <summary>
        ///     Gets the score for this group
        /// </summary>
        /// <param name="origin">The group requesting the evaluation</param>
        /// <returns>The score for this group</returns>
        public Score GetScore(Group origin)
        {
            //first check if groups direction is stable, otherwise disqualify
            int stabilityCoeff = 2;
            if (this.Heading.Norm1() < stabilityCoeff)
                return null;

            //next check if it even possible to catch the ship, otherwise disqualify
            if (!Navigator.IsReachable(origin.GetLocation(), GetLocation(), Heading))
                return null;

            //Reduce the score in proportion to distance
            //lower score is worse. Mind the minus sign!
            double distance = Navigator.CalcDistFromLine(origin.GetLocation(), this.GetLocation(), this.Heading);

            Bot.Game.Debug("EnemyGroup's HeadingVector CalcFromLine returned: " + distance);

            //consider attack radious
            distance -= Bot.Game.GetAttackRadius();

            //if the group is strong enough to take the enemy group add its score
            if (origin.LiveCount() > this.LiveCount())
            {
                Bot.Game.Debug("EnemyGroup was moved to ExpIterator processing:" + this.Id + " " + this.LiveCount() +
                               " " + origin.LiveCount());
                return new Score(this, TargetType.EnemyGroup, this.EnemyPirates.Count, distance);
            }

            return null;
        }

        /// <summary>
        ///     Returns the average location for this group
        /// </summary>
        /// <returns>Returns the average location for this group</returns>
        public Location GetLocation()
        {
            //Get a list of all location of the enemy pirates in this group
            List<Location> locs = new List<Location>();

            if (this.EnemyPirates == null)
                return new Location(0, 0);

            foreach (int e in this.EnemyPirates)
            {
                Pirate enemyPirate = Bot.Game.GetEnemyPirate(e);

                if (enemyPirate != null)
                    locs.Add(enemyPirate.Loc);
            }

            //sum all the locations!
            int totalCol = locs.Sum(loc => loc.Col);
            int totalRow = locs.Sum(loc => loc.Row);

            //return the average location
            if (locs.Count != 0)
                return new Location(totalCol / locs.Count, totalRow / locs.Count);

            return new Location(0, 0);
        }

        /// <summary>
        ///     This implements the GetDirection of the ITarget interface
        ///     it returns the best direction to keep the given group (which asked for directions)
        ///     in a path perpendicular to the direction of the enemy ship thus ensuring
        ///     that it will reach it as soon as possible
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public Direction GetDirection(Group group)
        {
            //calculates the direction based on the geographical data from the game
            return Navigator.CalculateDirectionToMovingTarget(group.GetLocation(), group.Heading, GetLocation(),
                Heading);
        }

        public TargetType GetTargetType()
        {
            return TargetType.EnemyGroup;
        }

        public string GetDescription()
        {
            string s = "Enemy Group, Pirates: ";
            foreach (int pirate in EnemyPirates)
                s += " " + pirate;

            return s;
        }

        /// <summary>
        ///     Tests if two enemy groups are the same
        /// </summary>
        /// <param name="operandB">the target to test with</param>
        /// <returns>True if identical or false otherwise</returns>
        public bool Equals(ITarget operandB)
        {
            EnemyGroup enemyGroup = operandB as EnemyGroup;
            if (enemyGroup != null)
            {
                EnemyGroup b = enemyGroup;
                return object.Equals(this, b);
            }

            return false;
        }

        #endregion

        /// <summary>
        ///     counts how many living pirates are in the group
        /// </summary>
        /// <returns>how many living pirates are in the group</returns>
        public int LiveCount()
        {
            return EnemyPirates.ConvertAll(p => Bot.Game.GetMyPirate(p)).Count(p => !p.IsLost);
        }

        /// <summary>
        ///     Determined the minimal time (or distance) between a Group and an
        ///     EnemyGroup before they will get into each others' attack radius.
        /// </summary>
        /// <param name="eg">The EnemyGroup</param>
        /// <param name="group">The Group</param>
        /// <returns>The minimal time before they'd get into each others' range</returns>
        private int InRangeGroupDistance(EnemyGroup eg, Group group)
        {
            Pirate enemyPirate = null, myPirate = null;

            int minDistance = Bot.Game.GetCols() + Bot.Game.GetRows();

            //find the two pirate from the two group with the minimum distance between
            for (int i = 0; i < eg.EnemyPirates.Count; i++)
            {
                Pirate p = Bot.Game.GetEnemyPirate(i);

                for (int j = 0; j < group.Pirates.Count; j++)
                {
                    Pirate aPirate = Bot.Game.GetMyPirate(group.Pirates[j]);

                    int distance = Bot.Game.Distance(p, aPirate);

                    if (distance >= minDistance)
                        continue;

                    minDistance = distance;
                    enemyPirate = p;
                    myPirate = aPirate;
                }
            }

            //return the distance between these pirates with the range in mind
            return Bot.Game.Distance(enemyPirate.Loc, myPirate.Loc) - Bot.Game.GetAttackRadius() * 2;
        }

        /// <summary>
        ///     Determines if an enemy pirate belongs to this enemy group.
        /// </summary>
        /// <param name="enemyPirate">The index of the enemy pirate</param>
        /// <returns>True if the pirate belongs to the group or false, otherwise</returns>
        public bool IsInGroup(int enemyPirate)
        {
            Pirate ePirate = Bot.Game.GetEnemyPirate(enemyPirate);

            //check if the pirate is null
            if (ePirate == null)
                return false;

            //Check if the given pirate is close (max of 2 distance units) to any of the pirates already in this group
            if (ePirate.IsLost)
                return false;

            return
                this.EnemyPirates.ConvertAll(e => Bot.Game.GetEnemyPirate(e))
                    .Select(ep => Bot.Game.Distance(ep, ePirate))
                    .Concat(new int[] {})
                    .Min() <= 2;
        }

        /// <summary>
        ///     Determines if an enemy pirate belongs to to the group of id's specified
        /// </summary>
        /// <param name="group">The indexes of the pirate in the group</param>
        /// <param name="enemyPirate">The index of the enemy pirate</param>
        /// <returns>True if the pirate belongs to the group or false, otherwise</returns>
        public static bool IsInGroup(List<int> group, int enemyPirate)
        {
            Pirate ePirate = Bot.Game.GetEnemyPirate(enemyPirate);

            if (ePirate.IsLost)
                return false;

            //Check if the given pirate is close (max of 2 distance units) to any of the pirates already in this group
            return
                group.ConvertAll(e => Bot.Game.GetEnemyPirate(e))
                    .Select(ep => Bot.Game.Distance(ep, ePirate))
                    .Concat(new int[] {})
                    .Min() <= 2;
        }

        /// <summary>
        ///     Gets the minimal distance from this enemy group to a location
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
        ///     Tries to determine the island target of this group from its HeadingVector.
        /// </summary>
        /// <returns>A SmartIsland if its probably the target or null if no island found</returns>
        public SmartIsland GuessTarget()
        {
            //Sort the islands by distance from this enemy group
            List<SmartIsland> sortedByDistance = SmartIsland.IslandList;
            sortedByDistance.Sort(
                (a, b) =>
                    Bot.Game.Distance(b.Loc, this.GetLocation()).CompareTo(Bot.Game.Distance(a.Loc, this.GetLocation())));

            //Should be tested because magic numbers aren't a good habit
            const int toleranceMargin = 2;

            foreach (SmartIsland isle in sortedByDistance)
            {
                //check if distance is smaller then tolerance margin
                if (Navigator.CalcDistFromLine(isle.GetLocation(), GetLocation(), Heading) < toleranceMargin)
                    return isle;
            }

            return null;
        }

        /// <summary>
        ///     This method updates Enemy's group direction and previous place (for the next turn calculations)
        ///     This method is called by the enemy class only!!!
        /// </summary>
        public void UpdateHeading()
        {
            //get the new direction of the last turn
            Direction newDir = Bot.Game.GetDirections(PrevLoc, GetLocation())[0];

            //update previous location
            PrevLoc = GetLocation();

            //update direction
            Heading.adjustHeading(newDir);
        }

        /// <summary>
        ///     given a list of pirates (that assumed to be contained in the pirates of the group)
        ///     creates a new EnemyGroup with this groups geographical parameters and removes the
        ///     removed pirates
        /// </summary>
        /// <param name="removedPirates"></param>
        /// <returns></returns>
        public EnemyGroup Split(List<int> removedPirates)
        {
            //if it means to recreate the whole group, then no need to do anything
            if (removedPirates.Count == EnemyPirates.Count)
                return null;

            //create new group
            EnemyGroup newEnemyGroup = new EnemyGroup(this.PrevLoc, removedPirates, this.Heading);

            //remove pirates
            this.EnemyPirates.RemoveAll(removedPirates.Contains);

            //return new enemy Group
            return newEnemyGroup;
        }

        /// <summary>
        ///     joins a given enemy group to this by adding its pirates and avraging the geographic data
        ///     if given itself, does nothing
        /// </summary>
        /// <param name="enemyGroup">the group to add</param>
        public void Join(EnemyGroup enemyGroup)
        {
            //check if id are the same then no need to do anything
            if (this.Id == enemyGroup.Id)
                return;

            //otherwise add their pirates and adjust stuff
            this.EnemyPirates.AddRange(enemyGroup.EnemyPirates);
            this.Heading += enemyGroup.Heading;
            PrevLoc = new Location((this.PrevLoc.Row + enemyGroup.PrevLoc.Row) / 2,
                (this.PrevLoc.Col + enemyGroup.PrevLoc.Col) / 2);
        }

        public override string ToString()
        {
            return this.EnemyPirates.Count.ToString();
        }

        /// <summary>
        ///     Tests if two enemy groups are the same
        /// </summary>
        /// <param name="other">the EnemyGroup to test with</param>
        /// <returns>True if identical or false otherwise</returns>
        protected bool Equals(EnemyGroup other)
        {
            if (this.EnemyPirates.Count != other.EnemyPirates.Count)
                return false;

            for (int i = 0; i < this.EnemyPirates.Count; i++)
                if (this.EnemyPirates[i] != other.EnemyPirates[i])
                    return false;

            return true;
        }

        /// <summary>
        ///     Tests if two enemy groups are the same
        /// </summary>
        /// <param name="obj">the object to test with</param>
        /// <returns>True if identical or false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj))
                return false;
            if (object.ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((EnemyGroup) obj);
        }
    }
}