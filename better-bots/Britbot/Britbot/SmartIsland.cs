#region #Usings

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
        ///     list of all the enemies and their distances from the island
        /// </summary>
        public List<KeyValuePair<EnemyGroup,bool>> approachingEnemies;

        /// <summary>
        ///     Queue of the last few amounts of enemy troops around this island
        /// </summary>
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
            this.approachingEnemies = new List<KeyValuePair<EnemyGroup, bool>>();
            //this.SurroundingForces = new Queue<int>();
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
            //constant defining how far to consider enemies capturing the island
            int CaptureZone = Bot.Game.GetAttackRadius();

            //check if there are more enemies than we can kill
            if (this.IsDangerousForGroup(origin))
            {
                Bot.Game.Debug("Danger--- Group " + origin.Id + " island: " + this.Id);
                return null;
            }

            //calculates the minimum distance between a group and said island
            int distance = Bot.Game.Distance(this.Loc, origin.FindCenter(true));

            //Amount of turns it takes to capture an island
            int captureTime = this.RealTimeTillCapture(Consts.ME);

            //TODO this is disabled in the meanwhile because it caused us to lost
            //should be taken into consideration in the score globalizing function
            /*if (this.Owner == Consts.ENEMY)
                captureTime *= 2;*/

            //check if the island isn't already ours, if so disqualify it and return null
            if (this.Owner != Consts.ME)
                return new Score(this, TargetType.Island, this.Value, this.NearbyEnemyCount(CaptureZone),
                    distance + captureTime, this.TurnsToEnemyCapture(origin));
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

        /// <summary>
        ///     just interface implementation, does nothing
        ///     so far
        /// </summary>
        public void TargetAssignmentEvent()
        {
        }

        /// <summary>
        ///     cana"l
        /// </summary>
        public void TargetDessignmentEvent()
        {
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
                double distance = eGroup.MinimalSquaredDistanceTo(this.Loc);
                //Checks if the group of islands is near the island and if they are getting closer or farther
                if (distance <= dangerRadius)
                {
                    //Calculates the sum of pirates in proximity to the island
                    //if distance is 0 then one of the pirates in on the island and we dont need to count it
                    if (distance == 0)
                        enemyCount = enemyCount + eGroup.EnemyPirates.Count - 1;
                    else
                        enemyCount = enemyCount + eGroup.EnemyPirates.Count;
                }
            }

            return enemyCount;
        }

        /// <summary>
        ///     Calculates the minimum amount of turns that a island will be under our control
        /// </summary>
        /// <returns>Distance of the nearest enemy group + the amount of turns it will take them to capture it</returns>
        public int TurnsToEnemyCapture(Group myGroup)
        {
            int tempDistance;
            int minDistance = Bot.Game.GetCols() + Bot.Game.GetRows();

            foreach (EnemyGroup eGroup in Enemy.Groups)
            {
                tempDistance = Bot.Game.Distance(this.Loc, eGroup.GetLocation());
                if ((minDistance > tempDistance) && (eGroup.GetMaxFightPower() >= myGroup.FightCount()))
                    minDistance = tempDistance;
            }

            return minDistance + this.CaptureTurns;
        }

        /// <summary>
        ///     updates the distances of all the enemies approaching the isalnds
        /// </summary>
        public void Update()
        {
            //---------------#Magic_Numbers--------------------
            //constant defining how far to look for enemy ships 
            //int DangerZone = 6 * Bot.Game.GetAttackRadius();

            //---------------#Magic_Numbers--------------------
            /*const int HowLongToLookBack = 1;
            //add new data
            this.SurroundingForces.Enqueue(this.NearbyEnemyCount(Magic.DangerZone));

            //trim the end of the queue if needed
            if (this.SurroundingForces.Count > HowLongToLookBack)
            {
                this.SurroundingForces.Dequeue();
            }*/

            //update enemy distances
            this.approachingEnemies.Clear();
            foreach (EnemyGroup eGroup in Enemy.Groups)
            {
                this.approachingEnemies.Add(new KeyValuePair<EnemyGroup, bool>(eGroup, eGroup.IsApproachingIsland(this)));
            }
            //sort the list by distance
            this.approachingEnemies.Sort(
                (e1, e2) => e1.Key.MinimalETATo(this.Loc).CompareTo(e2.Key.MinimalSquaredDistanceTo(this.Loc)));
        }

        /// <summary>
        ///     Returns the minimal distance to enemy group
        ///     uses the *SORTED* EnemyDistances list
        /// </summary>
        /// <returns>minimal distance to enemy group</returns>
        public double GetMinimumSquareDistanceFromEnemy()
        {
            //if isn't empty
            if (this.approachingEnemies.Count > 0)
                return this.approachingEnemies[0].Key.MinimalSquaredDistanceTo(this.Loc);

            //otherwise return the constant in MAGIC representing the best case scenario
            return Magic.MaxCalculableDistance;
        }

        /*public int GetEnemyNum()
        {
            int max = 0;

            
            foreach (int s in this.SurroundingForces)
            {
                if (s > max)
                    max = s;
            }
            return max;
        }*/

        public static void UpdateAll()
        {
            foreach (SmartIsland sIsland in SmartIsland.IslandList)
            {
                sIsland.Update();
            }
            SmartIsland.DebugAll();
        }

        public void Debug()
        {
            Bot.Game.Debug("Island " + this.Id + " enemies: " + string.Join(", ", this.approachingEnemies));
        }

        public static void DebugAll()
        {
            Bot.Game.Debug("--------ISLANDS DEBUG----------");
            foreach (SmartIsland sIsland in SmartIsland.IslandList)
                sIsland.Debug();
        }

        /// <summary>
        ///     This function sais if it is ok for a given group to try and capture this isalnd
        ///     considers all the other
        /// </summary>
        /// <param name="g">the group for which we test danger</param>
        /// <returns>true if it is dangerous, false otherwise</returns>
        public bool IsDangerousForGroup(Group g)
        {/*
            //Bot.Game.Debug("IsDangerousForGroup island: " + Id + " group " + g.Id);
            //calculate distance in turns till we reach the island
            int eta = Bot.Game.Distance(this.Loc, g.FindCenter(true));

            //find the distance of g from the island
            int distance = Bot.Game.Distance(this.Loc, g.FindCenter(true));


            //go over all the enemy groups approaching and their distances
            //TODO: maybe consider the actual firepower of the enemy
            foreach (EnemyGroup eGroup in this.approachingEnemies)
            {
                //calculate this enemy groups time of arival
                double EnemyETA = eGroup.MinimalETATo(this.Loc);

                //read enemy force
                double enemyForce = eGroup.GetMaxFightPower();

                //We diffarentiate between two cases
                if (EnemyETA < distance) //case 1: Enemy is closer to the island then we are
                {
                    //check if we arrive before capture
                    if (distance < EnemyETA + this.RealTimeTillCapture(Consts.ENEMY))
                    {
                        //the enemy who are closer are at disaddvantage since they lose one pirate capturing the island
                        if (g.LiveCount() <= enemyForce - 1)
                            return true;
                    }
                }
                else //case 2: Enemy is farther away to the island then we are
                {
                    //if the enemy reaches us before we capture
                    if (EnemyETA < this.RealTimeTillCapture(Consts.ME) + distance)
                    {
                        //for the enemy who are farther, we are at a disaddvantage
                        if ((enemyForce != 0) && (g.LiveCount() - 1 < enemyForce))
                            return true;

                        //special case: Manuver 
                        if ((enemyForce != 0) && (g.LiveCount() - 1 == enemyForce))
                        {
                            //we would like to run from the island only when they are realy close so we will catch them
                            if (eGroup.MinimalSquaredDistanceTo(this.Loc) < Magic.ManeuverDistance)
                            {
                                return true;
                            }
                        }
                    }
                }
            }*/

            //if we are here then everything is ok
            return false;
        }

        /// <summary>
        ///     This returns how many turns it would take for the conqueror team to capture the island
        ///     considering everything? (the owner and if there was some capturing before)
        /// </summary>
        /// <param name="conqueror">the team we ask about (see Consts)</param>
        /// <returns>how many turns it would take for the conqueror team to capture the island</returns>
        public int RealTimeTillCapture(int conqueror)
        {
            //calculate total number of turns to capture (without partials)
            int totalCaptureTime;
            //check if conqueror is the owner
            if (this.Owner == conqueror)
            {
                totalCaptureTime = 0;
            } //if the isalnd is nutral
            else if (this.Owner == Consts.NO_OWNER)
            {
                totalCaptureTime = this.CaptureTurns;
            } //if the island is of the other team
            else
            {
                totalCaptureTime = 2 * this.CaptureTurns;
            }

            //check if we already have some capture time
            if (this.TeamCapturing == conqueror)
            {
                return totalCaptureTime - this.TurnsBeingCaptured;
            }
            return totalCaptureTime;
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