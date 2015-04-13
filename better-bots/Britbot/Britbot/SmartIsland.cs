#region Usings

using System.Collections.Generic;
using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     A Class that encapsulates the default Island type and adds important methods to it
    /// </summary>
    public class SmartIsland : ITarget
    {
        #region Fields & Properies

        /// <summary>
        ///     The unique ID of the island
        /// </summary>
        public readonly int Id;

        /// <summary>
        ///     A static island list of all the islands in game
        /// </summary>
        public static List<SmartIsland> IslandList { get; private set; }

        /// <summary>
        ///     Turns to capture the island
        /// </summary>
        public int CaptureTurns
        {
            get { return Bot.Game.GetIsland(this.Id).CaptureTurns; }
        }

        /// <summary>
        ///     Value of the island, or how may normal island it is worth
        /// </summary>
        public int Value
        {
            get { return Bot.Game.GetIsland(this.Id).Value; }
        }

        /// <summary>
        ///     THe location of the island
        /// </summary>
        public Location Loc
        {
            get { return Bot.Game.GetIsland(this.Id).Loc; }
        }

        /// <summary>
        ///     The team currently capturing the island
        /// </summary>
        public int TeamCapturing
        {
            get { return Bot.Game.GetIsland(this.Id).TeamCapturing; }
        }

        /// <summary>
        ///     How many turns this island is being captured
        /// </summary>
        public int TurnsBeingCaptured
        {
            get { return Bot.Game.GetIsland(this.Id).TurnsBeingCaptured; }
        }

        /// <summary>
        ///     Who owns the island?
        /// </summary>
        public int Owner
        {
            get { return Bot.Game.GetIsland(this.Id).Owner; }
        }

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     A static constructor which initializes the static island list on the first reference to a SmartIsland
        /// </summary>
        static SmartIsland()
        {
            SmartIsland.IslandList = new List<SmartIsland>();
            foreach (Island island in Bot.Game.Islands())
            {
                SmartIsland.IslandList.Add(new SmartIsland(island.Id));
            }
        }

        /// <summary>
        ///     Creates a new SmartIsland
        /// </summary>
        /// <param name="encapsulate">The regular island index to encapsulate</param>
        private SmartIsland(int encapsulate)
        {
            this.Id = encapsulate;
        }

        #endregion

        #region Interface Implementations

        /// <summary>
        ///     Calculates the score of this island relative to a certain group by calculating how many potential points it will
        ///     generate.
        /// </summary>
        /// <param name="origin">The original group of pirates of the type "Group"</param>
        /// <returns>Returns the Score for the Target</returns>
        public Score GetScore(Group origin)
        {
            //constant defining how far to look for enemy ships 
            int DangerZone = 6 * Bot.Game.GetAttackRadius();
            //constant defining how far to consider enemies capturing the island
            int CaptureZone = Bot.Game.GetAttackRadius();

            //check if there are more enemies than we can kill
            if (this.NearbyEnemyCount(DangerZone) > origin.FightCount())
                return null;

            //calculates the minimum distance between a group and said island
            int distance = Bot.Game.Distance(this.Loc, origin.FindCenter(true));

            //Amount of turns it takes to capture an island
            int captureTime = this.CaptureTurns;

            //TODO this is disabled in the meanwhile because it caused us to lost
            //should be taken into consideration in the score globalizing function
            /*if (this.Owner == Consts.ENEMY)
                captureTime *= 2;*/

            //check if the island isn't already ours, if so disqualify it and return null
            if (this.Owner != Consts.ME || this.TeamCapturing == Consts.ENEMY)
                return new Score(this, TargetType.Island, this.Value, this.NearbyEnemyCount(CaptureZone),
                    distance + captureTime);
            return null;
        }

        /// <summary>
        ///     Get the location of the Island
        /// </summary>
        /// <returns>The Location of the island</returns>
        public Location GetLocation()
        {
            return Loc;
        }

        /// <summary>
        ///     Implements the getDirection method of the ITarget interface
        ///     searches for the direction which brings the path closest to a straight line
        /// </summary>
        /// <param name="group">The group asking for direction</param>
        /// <returns>best direction</returns>
        public Direction GetDirection(Group group)
        {
            //calculates the direction based on the geographical data from the game
            return Navigator.CalculateDirectionToStationeryTarget(group.FindCenter(true), group.Heading, GetLocation());
        }

        public TargetType GetTargetType()
        {
            return TargetType.Island;
        }

        public string GetDescription()
        {
            return "Island, id: " + Id + " location: " + Loc;
        }

        /// <summary>
        ///     Checks if 2 smart Islands are equal
        /// </summary>
        /// <param name="operandB">The island to check with</param>
        /// <returns>True if the islands are the same or false otherwise</returns>
        public bool Equals(ITarget operandB)
        {
            SmartIsland b = operandB as SmartIsland;
            if (b != null)
            {
                return Equals(b);
            }

            return false;
        }

        #endregion

        public override int GetHashCode()
        {
            return this.Id;
        }

        public static bool IsNearNonOurIsland(Location loc, int Range)
        {
            foreach (SmartIsland island in SmartIsland.IslandList)
            {
                if (island.Owner == Consts.ME)
                    continue;

                if (Bot.Game.Distance(loc, island) < Range)
                    return true;
            }
            return false;
        }

        /// <summary>
        ///     Checks if there are enemies near said Island that will probably attack it
        /// </summary>
        /// <returns>The amount of enemies near the target</returns>
        public int NearbyEnemyCount(int dangerRadius = 15)
        {
            int enemyCount = 0; //amount of enemy pirates in proximity to the Island
            /*int closestIslandDistance = 0; //The distance between this Island and the one nearest too it
            foreach (SmartIsland eIsland in SmartIsland.IslandList)
                //Calculates the distance between this island and the one nearest
            {
                int temp = Bot.Game.Distance(eIsland.Loc, this.Loc);
                if (temp < closestIslandDistance)
                {
                    closestIslandDistance = temp;
                }
            }*/
            foreach (EnemyGroup eGroup in Enemy.Groups)
            {
                //Checks if the group of islands is near the island and if they are getting closer or farther
                if (eGroup.MinimalSquaredDistanceTo(this.Loc) <= dangerRadius)
                {
                    //Calculates the sum of pirates in proximity to the island
                    enemyCount = enemyCount + eGroup.EnemyPirates.Count;
                }
            }

            return enemyCount;
        }

        /// <summary>
        ///     Checks if 2 smart Islands are equal
        /// </summary>
        /// <param name="a">Island 1</param>
        /// <param name="b">Island 2</param>
        /// <returns>True if the islands are the same or false otherwise</returns>
        public static bool operator ==(SmartIsland a, SmartIsland b)
        {
            return object.Equals(a, b);
        }

        /// <summary>
        ///     Checks if 2 smart islands are different
        /// </summary>
        /// <param name="a">Island 1</param>
        /// <param name="b">Island 2</param>
        /// <returns>False if the islands are the same or true otherwise</returns>
        public static bool operator !=(SmartIsland a, SmartIsland b)
        {
            return !object.Equals(a, b);
        }

        /// <summary>
        ///     Checks if 2 smart Islands are equal
        /// </summary>
        /// <param name="other">The island to check with</param>
        /// <returns>True if the islands are the same or false otherwise</returns>
        protected bool Equals(SmartIsland other)
        {
            bool eq = this.Id == other.Id;
            //Bot.Game.Debug("Ack identical targets: " + eq);
            return eq;
        }

        /// <summary>
        ///     Checks if 2 smart Islands are equal
        /// </summary>
        /// <param name="obj">The object to check with</param>
        /// <returns>True if the islands are the same or false otherwise</returns>
        public override bool Equals(object obj)
        {
            SmartIsland b = obj as SmartIsland;
            if (b != null)
            {
                return this.Equals(b);
            }
            return false;
        }
    }
}