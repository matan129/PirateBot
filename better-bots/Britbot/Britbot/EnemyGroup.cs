#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
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

        //---------------#Magic_Numbers--------------------
        //numbers of turns for wich we save data
        const int outOfDateNumber = 5;
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
        /// A queue of the last outOfDateNumber directions of this enemy group
        /// </summary>
        private Queue<Direction> LastDirections;

        /// <summary>
        /// A queue of the last max fight power coefficients
        /// </summary>
        private Queue<int> LastMaxFightPower;
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
            this.LastDirections = new Queue<Direction>();
            this.LastMaxFightPower = new Queue<int>();
        }

        /// <summary>
        ///     Creates a new instance of the EnemyGroup class
        /// </summary>
        public EnemyGroup(Location prevLoc, List<int> enemyPirates)
        {
            this.Id = EnemyGroup.IdCount++;
            PrevLoc = prevLoc;
            EnemyPirates = enemyPirates;
            this.LastDirections = new Queue<Direction>();
            this.LastMaxFightPower = new Queue<int>();
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
            //---------------#Magic_Numbers--------------------
            //first check if groups direction is stable, otherwise disqualify
            /*int stabilityCoeff = 2;
            if (this.GetHeadingSabilityCoeff() < stabilityCoeff)
                return null;*/

            //next check if it even possible to catch the ship, otherwise disqualify
            if (!Navigator.IsReachable(origin.GetLocation(), GetLocation(), this.GetHeading()))
                return null;

            //if it is very close to some island then return null since we might as well go for the island not creating
            //double targets
            foreach (SmartIsland sIsle in SmartIsland.IslandList)
                if (Bot.Game.IsReallyInRange(this.GetLocation(), sIsle.Loc))
                    return null;

            //Reduce the score in proportion to distance
            //lower score is worse. Mind the minus sign!
            double distance = Navigator.CalcDistFromLine(origin.GetLocation(), this.GetLocation(), this.GetHeading());

            //consider attack radious
            distance -= Math.Sqrt(Bot.Game.GetAttackRadius());
            distance = Math.Max(distance, 0);


            //if the group is strong enough to take the enemy group add its score
            if (origin.FightCount() > this.GetMaxFightPower())
            {
                return new Score(this, TargetType.EnemyGroup, 0, this.EnemyPirates.Count, distance);
            }

            return null;
        }

        /// <summary>
        ///     Returns the average location for this group 
        /// </summary>
        /// /// <param name="forcePirate">
        ///     if you ant the function to strictly return a location of a pirate or just the average
        ///     location
        /// </param>
        /// <returns>Returns the average location for this group or the pirate closest to the average</returns>
        public Location GetLocation()
        {
            return GetLocation(false);
        }

        /// <summary>
        ///     Returns the average location for this group 
        /// </summary>
        /// /// <param name="forcePirate">
        ///     if you ant the function to strictly return a location of a pirate or just the average
        ///     location
        /// </param>
        /// <returns>Returns the average location for this group or the pirate closest to the average</returns>
        public Location GetLocation(bool forcePirate)
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

            Location averageLocation = new Location(0,0);

            //return the average location
            if (locs.Count != 0)
                averageLocation =  new Location(totalRow / locs.Count, totalCol / locs.Count);

            if (forcePirate)
            {
                int minDistance = Bot.Game.GetCols() + Bot.Game.GetCols();
                Pirate pete = null;

                //iterate over all the pirate and find the one with the minimun distance to the average location
                foreach (Pirate pirate in this.EnemyPirates.ConvertAll(p => Bot.Game.GetEnemyPirate(p)))
                {
                    if (pirate.IsLost)
                        continue;

                    int currDistance = Bot.Game.Distance(averageLocation, pirate.Loc);
                    if (currDistance < minDistance)
                    {
                        minDistance = currDistance;
                        pete = pirate;
                    }
                }

                //set the returned location to the central pirate location
                if (pete != null)
                    averageLocation = pete.Loc;
            }

            return averageLocation;
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
            //first check if stationary
            if (this.GetHeading().Norm() == 0)
                return Navigator.CalculateDirectionToStationeryTarget(group.FindCenter(true), group.Heading,
                    this.GetLocation());
            //otherwise
            return Navigator.CalculateDirectionToMovingTarget(group.FindCenter(true), group.Heading, GetLocation(),
                this.GetHeading());
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
            //TODO Check this
            return EnemyPirates.ConvertAll(p => Bot.Game.GetMyPirate(p)).Count(p => !p.IsLost);
        }

        

        /// <summary>
        /// Calculates the maximum amount of pirates supporting each other in the enemy
        /// formation
        /// </summary>
        /// <returns>The maximum amount of pirates supporting each other in the enemy formation</returns>
        private int MaxFightCount()
        {
            //initialize maximum variable
            int maxFightCount = 0;

            //go over all pirates
            foreach (int pirate in this.EnemyPirates)
            {
                //count how many pirates are supporting him
                int supportCount = 0;

                //go over all other pirates
                foreach (int otherPirate in this.EnemyPirates)
                {
                    //check if in range
                    if (Bot.Game.InRange(Bot.Game.GetEnemyPirate(pirate), Bot.Game.GetEnemyPirate(otherPirate)))
                        supportCount++;
                }

                //check if the support count is bigger then the maximum
                if (maxFightCount < supportCount)
                    maxFightCount = supportCount;
            }

            //return result
            // + 1 because InRange accounts for itself
            return maxFightCount + 1;
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
                    .Any(e => Bot.Game.IsReallyInRange(ePirate.Loc, e.Loc));
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
        public double MinimalSquaredDistanceTo(Location location)
        {
            double min = Bot.Game.GetCols() + Bot.Game.GetRows();
            foreach (int pirate in EnemyPirates)
            {
                if (Bot.Game.EuclidianDistanceSquared(location, this.GetLocation()) < min)
                    min = Bot.Game.EuclidianDistanceSquared(location, this.GetLocation());
            }
            return min;
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
                if (Navigator.CalcDistFromLine(isle.GetLocation(), GetLocation(), this.GetHeading()) < toleranceMargin)
                    return isle;
            }

            return null;
        }

        /// <summary>
        /// returns the average of the maximum fight power in the 
        /// last outOfDateNumber of turns 
        /// </summary>
        /// <returns>returns the average of the maximum fight power</returns>
        public double GetMaxFightPower()
        {
            //check if LastMaxFightPower isnt empty
            if (this.LastMaxFightPower.Count == 0)
                return 0;
            //otherwise
            return this.LastMaxFightPower.Average();
        }
        
        /// <summary>
        /// This method updates the previous location, the last directions and the fighting power
        /// of this enemy group
        /// </summary>
        public void Update()
        {
            //get the new direction of the last turn
            Direction newDirection = Bot.Game.GetDirections(this.PrevLoc, this.GetLocation())[0];

            //update previous location
            PrevLoc = GetLocation();

            //update directions
            this.LastDirections.Enqueue(newDirection);

            //check if we need to throw irrelevant stuff out
            if (this.LastDirections.Count > outOfDateNumber)
            {
                this.LastDirections.Dequeue();
            }

            //update maximum fire power
            this.LastMaxFightPower.Enqueue(this.MaxFightCount());

            //check if we need to throw irrelevant stuff out
            if (this.LastMaxFightPower.Count > outOfDateNumber)
            {
                this.LastMaxFightPower.Dequeue();
            }
        }

        /// <summary>
        /// This function calculates this enemy group direction based on its last directions
        /// simply adds them up
        /// </summary>
        /// <returns>The heading of this enemy group</returns>
        public HeadingVector GetHeading()
        {
            //creating an accumulator heading vector with (0,0) values
            HeadingVector hv = new HeadingVector();

            //going over the last directions of this group and adding them up
            foreach (Direction dir in this.LastDirections)
            {
                //temporal variable for conversion
                HeadingVector currHeading = new HeadingVector(dir);
                hv += currHeading;
            }

            //return result
            return hv;
        }

        /// <summary>
        /// calculates a stability coefficient for this ship
        /// 1 means high stability and 0 means low one
        /// it becomes lower if there are many direction changes
        /// </summary>
        /// <returns>Stability coefficient</returns>
        public double GetHeadingSabilityCoeff()
        {
            return this.GetHeading().Norm1() / this.LastDirections.Count;
        }

        

        public override string ToString()
        {
            return "EnemyGroup- id: " + this.Id + ", fight power: " + this.GetMaxFightPower()
                   + ", Heading: " + this.GetHeading().ToString() + " location: " + GetLocation();
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
        
        public bool IsFormed() 
        {
            Location[][] ringLocations = new Location[Group.GetRingCount(this.EnemyPirates.Count)][];

            //set the pivot
            Location pivot = this.GetLocation(true);

            //set the locations
            for (int i = 0; i < ringLocations.Length; i++)
            {
                 ringLocations[i] = Group.GenerateRingLocations(pivot, i).ToArray();
            }

            //excluding the 0th ring which is special
            int maxRing = ringLocations.Length - 2;

            //this is basic summation of arithmetic sequence excluding the first ting
            int maxEmptySpots = Group.GetStructureVolume(maxRing) - this.EnemyPirates.Count;

            //iterate over all the rings
            for (int i = 0; i < ringLocations.Length; i++)
            {
                //iterate over all the location in each ring
                for (int k = 0; k < ringLocations[i].Length; k++)
                {
                    if (Bot.Game.GetPirateOn(ringLocations[i][k]) == null)
                        if (i == maxRing)
                            maxEmptySpots--;
                        else
                            return false;
                }
            }

            if (maxEmptySpots < 0)
                return false;

            return true;
        }
    }
}