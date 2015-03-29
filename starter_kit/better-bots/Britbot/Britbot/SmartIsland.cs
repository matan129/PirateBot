using System;
using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Britbot
{
    public class SmartIsland :  ITarget
    {
        #region constructors
        /// <summary>
        /// A static constructor which initializes the static island list on the first reference to a SmartIsland
        /// </summary>
        static SmartIsland()
        {
            IslandList = new List<SmartIsland>();
            foreach (Island island in Bot.Game.Islands())
            {
                IslandList.Add(new SmartIsland(island.Id));
            }
        }

        /// <summary>
        /// Creats a new SmartIsland
        /// </summary>
        /// <param name="encapsulate">The regular island index to encapsulate</param>
        public SmartIsland(int encapsulate)
        {
            this.Id = encapsulate;
        }
        #endregion

        #region members
        /// <summary>
        /// A static island list of all the islands in game
        /// </summary>
        public static List<SmartIsland> IslandList { get; private set; }

        /// <summary>
        /// The unique ID of the island
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Turns to capture the island
        /// </summary>
        public int CaptureTurns
        {
            get { return Bot.Game.GetIsland(this.Id).CaptureTurns; }
        }

        /// <summary>
        /// Value of the island, or how may mnrnal island it is worth
        /// </summary>
        public int Value
        {
            get { return Bot.Game.GetIsland(this.Id).Value; }
        }

        /// <summary>
        /// THe location of the island
        /// </summary>
        public Location Loc
        {
            get { return Bot.Game.GetIsland(this.Id).Loc; }
        }

        /// <summary>
        /// The team currently capturing the island
        /// </summary>
        public int TeamCapturing
        {
            get { return Bot.Game.GetIsland(this.Id).TeamCapturing; }
        }

        /// <summary>
        /// How many turns this island is being captured
        /// </summary>
        public int TurnsBeingCaptured
        {
            get { return Bot.Game.GetIsland(this.Id).TurnsBeingCaptured; }
        }

        /// <summary>
        /// Who owns the island?
        /// </summary>
        public int Owner
        {
            get { return Bot.Game.GetIsland(this.Id).Owner; }
        }
        #endregion

        #region operator overloading and overriding methods
        /// <summary>
        /// Checks if 2 smart Islands are equal
        /// </summary>
        /// <param name="a">Island 1</param>
        /// <param name="b">Island 2</param>
        /// <returns>True if the islands are the same or false otherwise</returns>
        public static bool operator ==(SmartIsland a, SmartIsland b)
        {
            return a.Id == b.Id;
        }

        /// <summary>
        /// Checks if 2 smart islands are different
        /// </summary>
        /// <param name="a">Island 1</param>
        /// <param name="b">Island 2</param>
        /// <returns>True or false</returns>
        public static bool operator !=(SmartIsland a, SmartIsland b)
        {
            return a.Id != b.Id;
        }

        protected bool Equals(SmartIsland other)
        {
            return CaptureTurns == other.CaptureTurns && Value == other.Value && Equals(Loc, other.Loc);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SmartIsland)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ CaptureTurns;
                hashCode = (hashCode * 397) ^ Value;
                hashCode = (hashCode * 397) ^ (Loc != null ? Loc.GetHashCode() : 0);
                return hashCode;
            }
        }
        #endregion

        /// <summary>
        /// Calculates the score of this island relative to a certain group by calculating how many potential points it will generate.
        /// </summary>
        /// <param name="origin">The original group of pirates of the type "Group"</param>
        /// <returns>Returns the Score for the Target</returns>
        public Score GetScore(Group origin)
        {
            //calculates the minimum distance between a group and said island
            int distance =
                origin.Pirates.ConvertAll(p => Bot.Game.GetMyPirate(p))
                    .Select(e => Bot.Game.Distance(e.Loc, this.Loc))
                    .Concat(new int[] {})
                    .Min();

            //Amount of turns it takes to capture an island
            int captureTime = this.CaptureTurns;

            //The amount of theoretical islands that we own
            int theoreticalIslandCount = this.Value + Bot.Game.MyIslands().Count;

            //The amount of points we would earn per turn if we owned said island
            int pointsPerTurn = (int) Math.Pow(2, theoreticalIslandCount) - 1;

            //The Amount of turns we will probably own said island
            int maxOwnershipTurns = 0;

            //The amount of future turns
            int turnNumber = 0;

            //Approximation of number of points we will have in the future
            int projectedPoints = 0;


            //TODO why is turnNumber not chaning here?
            while (turnNumber < maxOwnershipTurns)
            {
                projectedPoints = pointsPerTurn*(turnNumber - (distance + captureTime));
            }

            if (projectedPoints < 0) //As there is no such thing as negative points, Show 0.
            {
                projectedPoints = 0;
            }


            //TODO MaxOwnerShipTurns will be calculated using enemy group's heading and proximity to island
            return new Score(origin, projectedPoints);
        }

        /// <summary>
        /// Get the location of the Island
        /// </summary>
        /// <returns>The Location of the island</returns>
        public Location GetLocation()
        {
            return Loc;
        }

        /// <summary>
        /// Implements the getDirection method of the ITarget interface
        /// searches for the direction which brings the path closest to a straight line
        /// </summary>
        /// <param name="group">The group asking for direction</param>
        /// <returns>best direction</returns>
        public Direction GetDirection(Group group)
        {
            //calculates the direction besed on the geographical data from the game
            return HeadingVector.CalculateDirectionToStaitionaryTarget(group.GetLocation(), group.Heading, GetLocation());
        }

        /// <summary>
        /// Checks if there are enemies near said Island that will probably attack it
        /// </summary>
        /// <returns>The amount of enemies near the target</returns>
        public int NearbyEnemyCount()
        {
            int enemyCount = 0; //amount of enemy pirates in proximity to the Island
            int closestIslandDistance = 0; //The distance between this Island and the one nearest too it
            foreach (SmartIsland eisland in IslandList)
                //Calculates the distance between this island and the one nearest
            {
                int temp = Bot.Game.Distance(eisland.Loc, this.Loc);
                if (temp < closestIslandDistance)
                {
                    closestIslandDistance = temp;
                }
            }

            // All enemy pirates are heading towards a certain island and it is safe to assume that they are heading towards the one nearest to them
            int dangerRadius = closestIslandDistance/2;
            foreach (EnemyGroup eGroup in Enemy.Groups)
            {
                //Checks if the group of islands is near the island and if they are getting closer or farther
                if (eGroup.MinimalDistanceTo(this.Loc) <= dangerRadius && eGroup.GuessTarget() == this)
                {
                    //Calculates the sum of pirates in proximity to the island
                    enemyCount = enemyCount + eGroup.EnemyPirates.Count;
                }
            }

            return enemyCount;
        }
    }
}