#region #Usings

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

        //---------------#Magic_Numbers--------------------
        //numbers of turns for wich we save data
        private const int outOfDateNumber = 10;
        public static int IdCount;

        #endregion

        #region Fields & Properies

        /// <summary>
        ///     unique-ish id for the enemy group
        /// </summary>
        public readonly int Id;

        /// <summary>
        ///     The last turn in which this target was assigned
        /// </summary>
        private int LastAssignmentTurn;

        /// <summary>
        ///     A queue of the last outOfDateNumber directions of this enemy group
        /// </summary>
        private Queue<Direction> LastDirections;

        /// <summary>
        ///     A queue of the last max fight power coefficients
        /// </summary>
        private Queue<int> LastMaxFightPower;

        /// <summary>
        ///     What is this?
        /// </summary>
        public Location PrevLoc { get; set; }

        /// <summary>
        ///     List of pirate indexes in this group
        /// </summary>
        public List<int> EnemyPirates { get; private set; }

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
            double stabilityCoeff = 0.75;
            if (this.GetHeadingSabilityCoeff() < stabilityCoeff)
                return null;

            //check if the enemy group isn't in spawn
            //TODO: maybe we can be smarter here
            if (!Bot.Game.IsPassable(this.GetLocation()))
                return null;

            //next check if it even possible to catch the ship, otherwise disqualify
            if (!Navigator.IsReachable(origin.GetLocation(), GetLocation(), this.GetHeading()))
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
                return new Score(this, TargetType.EnemyGroup, 0, this.EnemyPirates.Count, distance,
                    Bot.Game.GetSpawnTurns());
            }

            return null;
        }

        /// <summary>
        ///     Returns the average location for this group
        /// </summary>
        /// ///
        /// <param name="forcePirate">
        ///     if you ant the function to strictly return a location of a pirate or just the average
        ///     location
        /// </param>
        /// <returns>Returns the average location for this group or the pirate closest to the average</returns>
        public Location GetLocation()
        {
            return GetLocation(false);
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

        /// <summary>
        ///     updates the last turn of assignment
        /// </summary>
        public void TargetAssignmentEvent()
        {
            this.LastAssignmentTurn = Bot.Game.GetTurn();
        }

        /// <summary>
        ///     checks if last assignment wasn't to close
        ///     if so it adds to the enemy suspition metter
        /// </summary>
        public void TargetDessignmentEvent()
        {
            //this defines what is the minimum turn number till it is legit to change target (on average)
            //---------------#Magic_Numbers--------------------
            const int minimumTillItIsOkToDropTarget = 5;

            //check if time from the last assignment raises suspition of inteligence in the enemy
            if (Bot.Game.GetTurn() - this.LastAssignmentTurn < minimumTillItIsOkToDropTarget)
            {
                Enemy.EnemyIntelligenceSuspicionCounter++;
            }
        }

        #endregion

        /// <summary>
        ///     Returns the average location for this group
        /// </summary>
        /// ///
        /// <param name="forcePirate">
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

            Location averageLocation = new Location(0, 0);

            //return the average location
            if (locs.Count != 0)
                averageLocation = new Location(totalRow / locs.Count, totalCol / locs.Count);

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
        ///     counts how many living pirates are in the group
        /// </summary>
        /// <returns>how many living pirates are in the group</returns>
        public int LiveCount()
        {
            //TODO Check this
            return EnemyPirates.ConvertAll(p => Bot.Game.GetMyPirate(p)).Count(p => !p.IsLost);
        }

        /// <summary>
        ///     Calculates the maximum amount of pirates supporting each other in the enemy
        ///     formation
        /// </summary>
        /// <returns>The maximum amount of pirates supporting each other in the enemy formation</returns>
        private int MaxFightCount()
        {
            //initialize maximum variable
            int maxFightCount = 0;

            //go over all pirates
            foreach (int pirate in this.EnemyPirates)
            {
                //convert to pirate
                Pirate pete = Bot.Game.GetEnemyPirate(pirate);

                //check if this pirate counts
                if (Bot.Game.isCapturing(pete))
                    continue;
                //count how many pirates are supporting him
                int supportCount = 0;

                //go over all other pirates
                foreach (int otherPirate in this.EnemyPirates)
                {
                    //convert to pirate
                    Pirate otherPete = Bot.Game.GetEnemyPirate(otherPirate);

                    //check if this pirate counts
                    if (Bot.Game.isCapturing(otherPete))
                        continue;

                    //check if in range
                    if (Bot.Game.EuclidianDistanceSquared(pete.Loc, otherPete.Loc) <= Bot.Game.GetAttackRadius())
                        supportCount++;
                }

                //check if the support count is bigger then the maximum
                if (maxFightCount < supportCount)
                    maxFightCount = supportCount;
            }

            //return result
            return maxFightCount;
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
        /// Gets the minimal number of turns for the group to reach the given location
        /// </summary>
        /// <param name="location">the location tested</param>
        /// <returns>minimal number of turns till the group gets to the location</returns>
        public double MinimalETATo(Location location)
        {
            double min = Bot.Game.GetCols() + Bot.Game.GetRows();
            foreach (int pirate in EnemyPirates)
            {
                if (Bot.Game.Distance(location, this.GetLocation()) < min)
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
        /// This method checks if this enemy group could be approaching the given island
        /// first checks if it moves in the general direction of the island
        /// compares difference in trajectory to a MAGIC constant
        /// </summary>
        /// <param name="sIsland">the island we check</param>
        /// <returns>true if it is possible that the enemy is approching the island</returns>
        public bool IsApproachingIsland(SmartIsland sIsland)
        {
            //if the group is stationary just check if it is close
            if(this.GetHeading().NormSquared() == 0)
            {
                return Bot.Game.EuclidianDistanceSquared(this.GetLocation(), sIsland.Loc) <= Magic.ApproachDistanceSquaredOfStationaryTarget;
            }
            //check if the enemy group is on the island
            if (this.MinimalSquaredDistanceTo(sIsland.Loc) < 1)
                return true;

            //calculate the difference vector between the enemy group and the island
            HeadingVector difference = HeadingVector.CalcDifference(this.GetLocation(), sIsland.Loc);

            //check if it isn't moving in the general direction of the island, if so return false
            if (difference * this.GetHeading() <= 0)
            {
                return false;
            }
            //read the distance between the trajectory of the enemy group and the island
            double distance = Navigator.CalcDistFromLine(sIsland.Loc, this.GetLocation(), this.GetHeading());
            /*Bot.Game.Debug("Loc " + sIsland.Loc + " line loc: " + this.GetLocation().ToString() + " heading: " + this.GetHeading().ToString());
            Bot.Game.Debug("Distance: " + distance);*/
            //compare it to the constant defined in magic
            return distance <= Magic.EnemyPredictionSensitivity;
        }

        /// <summary>
        ///     returns the average of the maximum fight power in the
        ///     last outOfDateNumber of turns
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
        ///     This method updates the previous location, the last directions and the fighting power
        ///     of this enemy group
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
            if (this.LastDirections.Count > EnemyGroup.outOfDateNumber)
            {
                this.LastDirections.Dequeue();
            }

            //update maximum fire power
            this.LastMaxFightPower.Enqueue(this.MaxFightCount());

            //check if we need to throw irrelevant stuff out
            if (this.LastMaxFightPower.Count > EnemyGroup.outOfDateNumber)
            {
                this.LastMaxFightPower.Dequeue();
            }
        }

        /// <summary>
        ///     This function calculates this enemy group direction based on its last directions
        ///     simply adds them up
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
        ///     calculates a stability coefficient for this ship
        ///     1 means high stability and 0 means low one
        ///     it becomes lower if there are many direction changes
        /// </summary>
        /// <returns>Stability coefficient</returns>
        public double GetHeadingSabilityCoeff()
        {
            //count actual moves
            int moveCount = 0;

            //go over all the direction and compare to Direction.Nothing
            foreach (Direction dir in this.LastDirections)
            {
                if (dir != Direction.NOTHING)
                    moveCount++;
            }

            //if move count is 0 then target is stationary and thus very stable
            if (moveCount == 0)
                return 1;
            //otherwise return that
            return this.GetHeading().Norm1() / moveCount;
        }

        public override string ToString()
        {
            return "EnemyGroup- id: " + this.Id + ", fight power: " + this.GetMaxFightPower()
                   + ", Heading: " + this.GetHeading() + " location: " + GetLocation();
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