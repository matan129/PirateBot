using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Britbot
{
    /// <summary>
    /// A class which represents an enemy group
    /// </summary>
    public class EnemyGroup : ITarget
    {
        #region members
        /// <summary>
        /// What is this?
        /// </summary>
        public Location PrevLoc { get; set; }

        public static int idCount;

        /// <summary>
        /// unique-ish id for the enemy group
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// List of pirate indexes in this group
        /// </summary>
        public List<int> EnemyPirates { get; private set; }

        /// <summary>
        /// The direction this group's heading to 
        /// </summary>
        public HeadingVector Heading { get; private set; }
        #endregion

        #region constructors
        /// <summary>
        /// Creates a new instance of the EnemyGroup class
        /// </summary>
        public EnemyGroup()
        {
            this.Id = idCount++;
            this.EnemyPirates = new List<int>();
            this.PrevLoc = new Location(0,0);
            this.Heading = new HeadingVector(this.PrevLoc);
        }

        /// <summary>
        /// Creates a new instance of the EnemyGroup class
        /// </summary>
        public EnemyGroup(Location prevLoc, List<int> enemyPirates, HeadingVector heading)
        {
            this.Id = idCount++;
            PrevLoc = prevLoc;
            EnemyPirates = enemyPirates;
            Heading = heading;
        }
        #endregion

        #region overloaded operators and overrided methods
        /// <summary>
        /// Tests if two enemy groups are the same
        /// </summary>
        /// <param name="operandB">the target to test with</param>
        /// <returns>True if identical or false otherwise</returns>
        public bool Equals(ITarget operandB)
        {
            if (operandB is EnemyGroup)
            {
                EnemyGroup b = (EnemyGroup)operandB;
                return this.Id == b.Id;
            }

            return false;
        }

        /// <summary>
        /// Tests if two enemy groups are the same
        /// </summary>
        /// <param name="other">the EnemyGroup to test with</param>
        /// <returns>True if identical or false otherwise</returns>
        protected bool Equals(EnemyGroup other)
        {
            return ReferenceEquals(this, other);
        }

        /// <summary>
        /// Tests if two enemy groups are the same
        /// </summary>
        /// <param name="obj">the object to test with</param>
        /// <returns>True if identical or false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EnemyGroup)obj);
        }

        /// <summary>
        /// Tests if two enemy groups are the same
        /// </summary>
        /// <returns>True if identical or false otherwise</returns>
        public static bool operator ==(EnemyGroup a, EnemyGroup b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Tests if two enemy groups are the not same
        /// </summary>
        /// <returns>false if identical or true otherwise</returns>
        public static bool operator !=(EnemyGroup a, EnemyGroup b)
        {
            return !(a == b);
        }
        
        #endregion

        /// <summary>
        /// Gets the score for this group
        /// </summary>
        /// <param name="origin">The group requesting the evaluation</param>
        /// <returns>The score for this group</returns>
        public Score GetScore(Group origin)
        {
            //Reduce the score in proportion to distance
            //lower score is worse. Mind the minus sign!
            double distance = HeadingVector.CalcDistFromLine(origin.GetLocation(),this.GetLocation(), this.Heading);
            
            Bot.Game.Debug("EnemyGroup's HeadingVector CalcFromLine returned: " + distance);

            //consider attack radious
            distance -=  2*Bot.Game.GetAttackRadius();

            //if the group is strong enough to take the enemy group add its score
            if (origin.LiveCount() > this.LiveCount())
            {
                Bot.Game.Debug("EnemyGroup was moved to ExpIterator processing:" + this.Id + " " + this.LiveCount() + " " + origin.LiveCount());
                return new Score(this, TargetType.EnemyGroup, this.EnemyPirates.Count, distance);
            }
            else //otherwise we don't even want to consider it
                return null;
        }

        /// <summary>
        /// Returns the average location for this group
        /// </summary>
        /// <returns>Returns the average location for this group</returns>
        public Location GetLocation()
        {
            //Get a list of all location of the enemy pirates in this group
            List<Location> locs = new List<Location>();

            if(this.EnemyPirates == null) return new Location(0,0);

            foreach (int e in this.EnemyPirates)
            {
                Pirate enemyPirate = Bot.Game.GetEnemyPirate(e);
                locs.Add(enemyPirate.Loc);
            }

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
            //calculates the direction based on the geographical data from the game
            return HeadingVector.CalculateDirectionToMovingTarget(group.GetLocation(), group.Heading, GetLocation(),
                Heading);
        }

        /// <summary>
        /// counts how many living pirates are in the group
        /// </summary>
        /// <returns>how many living pirates are in the group</returns>
        public int LiveCount()
        {
            return EnemyPirates.ConvertAll(p => Bot.Game.GetMyPirate(p)).Count(p => !p.IsLost);
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
            /* TODO I THINK I HAVE A BETTER Solution THEN THIS
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

        public override string ToString()
        {
            return this.EnemyPirates.Count.ToString();
        }

        public string ToS()
        {
            string s = "Enemy Group, Pirates: ";
            foreach (int pirate in EnemyPirates)
                s += " " + pirate;

            return s;
        }
    }
}