using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Britbot
{
    /// <summary>
    /// A Class that encapsulates the default Island type and adds important methods to it
    /// </summary>
    public class SmartIsland : ITarget
    {
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

            //check if the island isn't already ours, if so disqualify it and return null
            if (this.Owner != Consts.ME)
                return new Score(this, TargetType.Island, (origin.Pirates.Count*(this.Value - 1)) + 1,distance + captureTime);
            return null;
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

        public string ToS()
        {
            return "Island, id: " + Id + " location: " + Loc;
            ;
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
        /// Value of the island, or how may normal island it is worth
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

        #region operator overloading and overriding methods

        /// <summary>
        /// Checks if 2 smart Islands are equal
        /// </summary>
        /// <param name="a">Island 1</param>
        /// <param name="b">Island 2</param>
        /// <returns>True if the islands are the same or false otherwise</returns>
        public static bool operator ==(SmartIsland a, SmartIsland b)
        {
            return Equals(a, b);
        }

        /// <summary>
        /// Checks if 2 smart islands are different
        /// </summary>
        /// <param name="a">Island 1</param>
        /// <param name="b">Island 2</param>
        /// <returns>False if the islands are the same or true otherwise</returns>
        public static bool operator !=(SmartIsland a, SmartIsland b)
        {
            return !Equals(a, b);
        }

        /// <summary>
        /// Checks if 2 smart Islands are equal
        /// </summary>
        /// <param name="operandB">The island to check with</param>
        /// <returns>True if the islands are the same or false otherwise</returns>
        public bool Equals(ITarget operandB)
        {
            if (operandB is SmartIsland)
            {
                return Equals((SmartIsland) operandB);
            }

            return false;
        }

        /// <summary>
        /// Checks if 2 smart Islands are equal
        /// </summary>
        /// <param name="other">The island to check with</param>
        /// <returns>True if the islands are the same or false otherwise</returns>
        protected bool Equals(SmartIsland other)
        {
            return this.Id == other.Id;
        }

        /// <summary>
        /// Checks if 2 smart Islands are equal
        /// </summary>
        /// <param name="obj">The object to check with</param>
        /// <returns>True if the islands are the same or false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is SmartIsland)
            {
                return Equals((SmartIsland) obj);
            }
            return false;
        }

        /// <summary>
        /// Gets a unique-ish hash for this instace of SmartIsland
        /// ReSharper generated code
        /// </summary>
        /// <returns>A hash for this island</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ CaptureTurns;
                hashCode = (hashCode*397) ^ Value;
                hashCode = (hashCode*397) ^ (Loc != null ? Loc.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}