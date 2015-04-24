#region #Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pirates;
using Britbot.Simulator;
#endregion

namespace Britbot
{
    /// <summary>
    ///     Just stuff that makes the hard decisions
    /// </summary>
    public static class Commander
    {
        #region Static Fields & Consts

        private static Stopwatch _turnTimer;

        #endregion

        #region Fields & Properies

        /// <summary>
        ///     List of groups of our pirates
        /// </summary>
        public static List<Group> Groups { get; set; }

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     This static constructor will run once and initialize the commander
        /// </summary>
        static Commander()
        {
            Commander.Groups = new List<Group>();
            Commander._turnTimer = new Stopwatch();

            #region Terrible Switch-Case

            //TODO initial config should be better then this
            //Hookup the UltimateConfig() here

            if (Bot.Game.Islands().Count == 1)
            {
                Commander.Groups.Add(new Group(0, Bot.Game.AllMyPirates().Count));
                return;
            }

            //TODO this is awfully specific for the game bots. We have to generalize this
            switch (Bot.Game.AllMyPirates().Count)
            {
                case 3:
                    Commander.Groups.Add(new Group(0, 2));
                    Commander.Groups.Add(new Group(2, 1));
                    break;
                case 4:
                    if (Bot.Game.AllEnemyPirates().Count > 4)
                    {
                        Commander.Groups.Add(new Group(0, 1));
                        Commander.Groups.Add(new Group(1, 1));
                        Commander.Groups.Add(new Group(2, 2));
                    }
                    else
                    {
                        Commander.Groups.Add(new Group(0, 3));
                        Commander.Groups.Add(new Group(3, 1));
                        ;
                    }
                    break;
                case 5:
                    Commander.Groups.Add(new Group(0, 2));
                    Commander.Groups.Add(new Group(2, 2));
                    Commander.Groups.Add(new Group(4, 1));
                    break;
                case 6:
                    if (Bot.Game.EnemyIslands().Count > 0)
                    {
                        Commander.Groups.Add(new Group(0, 5));
                        Commander.Groups.Add(new Group(5, 1));
                    }
                    else
                    {
                        Commander.Groups.Add(new Group(2, 4));
                        Commander.Groups.Add(new Group(0, 1));
                        Commander.Groups.Add(new Group(1, 1));
                    }
                    break;
                case 7:
                    Commander.Groups.Add(new Group(0, 2));
                    Commander.Groups.Add(new Group(2, 3));
                    Commander.Groups.Add(new Group(5, 2));

                    break;
                case 8:
                    if (Bot.Game.GetMyPirate(7).Loc.Row == 39)
                    {
                        Commander.Groups.Add(new Group(0, 4));
                        Commander.Groups.Add(new Group(4, 3));
                        Commander.Groups.Add(new Group(7, 1));
                    }
                    else
                    {
                        Commander.Groups.Add(new Group(0, 3));
                        Commander.Groups.Add(new Group(3, 2));
                        Commander.Groups.Add(new Group(5, 2));
                        Commander.Groups.Add(new Group(7, 1));
                    }
                    break;
                case 9:
                    Commander.Groups.Add(new Group(0, 3));
                    Commander.Groups.Add(new Group(3, 3));
                    Commander.Groups.Add(new Group(6, 2));
                    Commander.Groups.Add(new Group(8, 1));
                    break;
                default:
                    /*for (int i = 0; i < Bot.Game.AllMyPirates().Count - Bot.Game.AllMyPirates().Count%2; i += 2)
                {
                    Commander.Groups.Add(new Group(i, 2));
                }
                if (Bot.Game.AllMyPirates().Count%2 == 1)
                    Commander.Groups.Add(new Group(Bot.Game.AllMyPirates().Count, 1));*/
                    //Commander.Groups.Add(new Group(0, Bot.Game.AllMyPirates().Count));
                    for (int i = 0; i < Bot.Game.AllMyPirates().Count; i++)
                        Commander.Groups.Add(new Group(i, 1));
                    break;
            }

            #endregion
        }

        #endregion

        /// <summary>
        ///     Do something!
        /// </summary>
        public static Dictionary<Pirate, Direction> Play(CancellationToken cancellationToken, out bool onTime)
        {
            // Restart the timer
            Commander._turnTimer.Restart();

            //note that because this method is on a separate thread we need this try-catch although we have on our bot
            try
            {
                Logger.BeginTime("Update");
                //update the enemy info
                Enemy.Update(cancellationToken);
                Logger.StopTime("Update");


                Logger.BeginTime("UpdateAll");
                //update smartIslands
                SmartIsland.UpdateAll();
                Logger.StopTime("UpdateAll");

                Logger.BeginTime("CalculateAndAssignTargets");
                //calculate targets
                Commander.CalculateAndAssignTargets(cancellationToken);
                Logger.StopTime("CalculateAndAssignTargets");

                //fix configuration
                Logger.BeginTime("GroupSplitting");
                Veteran.GroupSplitting();
                Logger.StopTime("GroupSplitting");

                Logger.BeginTime("GroupJoining");
                Veteran.GroupJoining();
                Logger.StopTime("GroupJoining");

                /*Logger.BeginTime("FixGroupArrangement");
                Commander.FixGroupArrangement();
                Logger.StopTime("FixGroupArrangement");*/

                Logger.BeginTime("GetAllMoves");
                //Get the moves for all the pirates and return them
                Dictionary<Pirate, Direction> moves = Commander.GetAllMoves(cancellationToken);
                Logger.StopTime("GetAllMoves");


                Logger.Write(
                    string.Format("Commander done doing calculations and drinking coffee after {0}ms",
                        Commander._turnTimer.ElapsedMilliseconds), true);

                //we are on time!
                onTime = true;

                return moves;
            }
            catch (AggregateException ex)
            {
                Logger.Write("****** COMMANDER EXITING DUE TO AggregateException ******", true);
                foreach (Exception e in ex.InnerExceptions)
                    Logger.Write(e.ToString(), true);
                onTime = false;
                Logger.Profile();
                return new Dictionary<Pirate, Direction>();
            }
            catch (OperationCanceledException) //catch task cancellation
            {
                Logger.Write("****** COMMANDER EXITING DUE TO TASK CANCELLATION ******", true);
                onTime = false;
                Logger.Profile();
                return new Dictionary<Pirate, Direction>();
            }
            catch (Exception ex) //catch everyting else
            {
                Logger.Write("==========COMMANDER EXCEPTION============", true);
                Logger.Write("Commander almost crashed because of exception: " + ex.Message, true);

                //Holy shit. This actually works!!
                StackTrace exTrace = new StackTrace(ex, true);
                StackFrame frame = exTrace.GetFrame(0);
                Logger.Write(
                    string.Format("The exception was thrown from method {0} at file {1} at line #{2}", frame.GetMethod(),
                        frame.GetFileName(), frame.GetFileLineNumber()), true);

                Logger.Write("==========COMMANDER EXCEPTION============", true);
                onTime = false;
                Logger.Profile();
                return new Dictionary<Pirate, Direction>();
            }
        }

        /// <summary>
        ///     Max proirities we can affort to compute and iterate over
        /// </summary>
        /// <returns></returns>
        public static int CalcMaxPrioritiesNum()
        {
            return (int) (Math.Pow(Magic.MaxIterator, 1.0 / Commander.Groups.Count));
        }

        /// <summary>
        ///     Assigns targets to each group based on pure magic
        ///     Also initiate local scoring
        /// </summary>
        /// <param name="cancellationToken"></param>
        public static void CalculateAndAssignTargets(CancellationToken cancellationToken)
        {
            //force groups to calculate priorities
            Commander.StartCalcPriorities(cancellationToken);

            //read dimensions of iteration
            int[] dimensions = Commander.GetTargetsDimensions();

            //read all possible target-group assignment
            Score[][] possibleAssignments = Commander.GetPossibleTargetMatrix();

            //indexes of the best assignment yet
            int[] maxAssignment = new int[dimensions.Length];
            double maxScore = -9999999999999;

            //create new iteration object
            ExpIterator iterator = new ExpIterator(dimensions);

            //Score array for calculations in each iteration
            Score[] scoreArr = new Score[dimensions.Length];

            //iterating over all possible target assignments
            do
            {
                //Throwing an exception if cancellation was requested.
                cancellationToken.ThrowIfCancellationRequested();

                //set score array for current iteration
                scoreArr = Commander.GetSpecificAssignmentScores(possibleAssignments, iterator.Values);

                //calculate new score
                double newScore = Commander.GlobalizeScore(scoreArr, cancellationToken);

                //check if the score is better
                if (newScore > maxScore)
                {
                    //replace best
                    maxScore = newScore;
                    Array.Copy(iterator.Values, maxAssignment, iterator.Values.Length);
                }
            } while (iterator.NextIteration());

            //read the "winning" assignment
            scoreArr = Commander.GetSpecificAssignmentScores(possibleAssignments, maxAssignment);

            //no we got the perfect assignment, just set it up
            for (int i = 0; i < dimensions.Length; i++)
            {
                Logger.Write(string.Format("Group {0} assinged to {1}", i, scoreArr[i].Target.GetDescription()), true);
                Commander.Groups[i].SetTarget(scoreArr[i].Target);
            }
        }

        /// <summary>
        ///     Distribute our pirates into groups and re-arrange them at the start of the game
        /// </summary>
        /// <param name="config">The new configuration. i.e. {2,2,2} for three groups of two pirates</param>
        public static void DistributeForces(int[] config)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Returns a list of integers that describes the current wanted configuration
        /// </summary>
        /// <returns>ULTIMATE group configuration</returns>
        public static List<int> GetUltimateGameConfig()
        {
            int[] eConfig = Enemy.Groups.ConvertAll(group => group.EnemyPirates.Count).ToArray();

            Array.Sort(eConfig, (a, b) => a.CompareTo(b));

            List<int> ret = new List<int>();

            int myPirates = Bot.Game.AllMyPirates().Count;


            for (int i = 0; i < eConfig.Length && myPirates > 0; i++)
            {
                if (eConfig[i] + 1 > myPirates)
                    break;
                ret.Add(eConfig[i] + 1);
                myPirates -= eConfig[i] + 1;
            }

            while (myPirates > 0)
            {
                ret.Add(1);
                myPirates--;
            }

            while (ret.Count > Bot.Game.Islands().Count)
            {
                ret[ret.Count - 2] += ret.Last();
                ret.RemoveAt(ret.Count - 1);
            }

            return ret;
        }

        /// <summary>
        ///     This function should convert an array of local scores into a numeric
        ///     score based on global criteria
        /// </summary>
        /// <param name="scoreArr">array of local scores</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static double GlobalizeScore(Score[] scoreArr, CancellationToken cancellationToken)
        {
            /*double score = 0;
            double timeAvg = 0;
            double enemyShips = 0;
            double ownedIslands = 0;
            double maxIslandOwnership = 0;
            double totalProjectedPoints = 0;

            foreach (Score s in scoreArr)
            {
                //Throwing an exception if cancellation was requested.
                cancellationToken.ThrowIfCancellationRequested();

                //Calculates the Island that will be held onto the longest
                if (maxIslandOwnership < s.MinTurnsToEnemyCapture)
                    maxIslandOwnership = s.MinTurnsToEnemyCapture;

                ownedIslands += s.Value; //Number of owned islands in this option
                enemyShips += s.EnemyShips; //enemy ships destroyed in this option
                timeAvg += s.Eta;
            }

            //check if there are two of the same target
            for (int i = 0; i < scoreArr.Length - 1; i++)
            {
                for (int j = i + 1; j < scoreArr.Length; j++)
                {
                    if (scoreArr[i].Target.Equals(scoreArr[j].Target))
                        score -= 1000;
                }
            }
            //TODO: This is EXTEMELY inefficient, we dont need to go over each turn, we only need to go over turns in which things change, Ron talk to me sometime (Matan K)
            //TODO: add points also for killing pirates
            //TODO: Recalculate IslandOwnership when an enemy pirate dies (This will help better implement the killing of enemy pirates in the score)
            for (int i = 0; i < maxIslandOwnership; i++)
            {
                //calculate how many points we have in the i'th turn
                double totalAditionalIslandPoints = 0;
                foreach (Score s in scoreArr)
                {
                    //check if this target gives us points this turn
                    if ((s.Eta <= i) && (s.MinTurnsToEnemyCapture > i))
                        totalAditionalIslandPoints += s.Value;
                }
                totalProjectedPoints += ScoreHelper.ComputePPT(totalAditionalIslandPoints);
            }

            //TODO: give more points if we take an island from the enemy

            return score + totalProjectedPoints;*/
            SimulatedGame sg = new SimulatedGame(new List<SimulatedEvent>());
            Commander.SetEventList(sg);

            for (int i = 0; i < scoreArr.Length; i++)
            {
                if (scoreArr[i].Type == TargetType.Island)
                    sg.AddEvent(new GroupArrivalEvent((int)scoreArr[i].Eta, sg.Islands[((SmartIsland)(scoreArr[i].Target)).Id], sg.MyGroups[Groups[i].Id]));
                if(scoreArr[i].Type == TargetType.EnemyGroup)
                    sg.AddEvent(new BattleEvent((int)scoreArr[i].Eta, sg.EnemyGroups[((EnemyGroup)(scoreArr[i].Target)).Id], sg.MyGroups[Groups[i].Id]) );
            }

            return sg.SimulateGame();
        }

        internal static void SetEventList(SimulatedGame sg)
        {
            List<SimulatedEvent> eventList = new List<SimulatedEvent>();
            //going over all the islands
            foreach(SmartIsland sIsland in SmartIsland.IslandList)
            {
                //go over the enemy list of each island
                foreach(KeyValuePair<EnemyGroup,bool> enemy in sIsland.approachingEnemies)
                {
                    //if it is likely that he will come to the island
                    if(enemy.Value)
                    {
                        eventList.Add(new GroupArrivalEvent((int)enemy.Key.MinimalETATo(sIsland.Loc),
                                      sg.Islands[sIsland.Id],
                                      sg.EnemyGroups[enemy.Key.Id]));
                    }
                    else
                    {
                        eventList.Add(new PossibleArrivalEvent((int)enemy.Key.MinimalETATo(sIsland.Loc),
                                      sg.Islands[sIsland.Id],
                                      sg.EnemyGroups[enemy.Key.Id]));
                    }
                }
            }
        }

        /// <summary>
        ///     Gets all th moves for each pirate in each group
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A dictionary that gives each pirate a direction to move to in this turn</returns>
        private static Dictionary<Pirate, Direction> GetAllMoves(CancellationToken cancellationToken)
        {
            Commander.Groups.RemoveAll(g => g.Pirates.Count == 0);

            // Update all groups
            Parallel.ForEach(Commander.Groups, g => g.Update());

            //A list with all the moves from all groups
            List<KeyValuePair<Pirate, Direction>> allMoves =
                new List<KeyValuePair<Pirate, Direction>>(Bot.Game.AllMyPirates().Count);

            //Get the moves from each group we have
            foreach (Group group in Commander.Groups)
            {
                allMoves.AddRange(group.GetGroupMoves(cancellationToken));
            }

            //Convert the moves list to dictionary
            return allMoves.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        /// <summary>
        ///     This function goes over all the groups, reads their priorities and
        ///     arranges them in a 2-dimensional array: Each group has it's own row
        ///     which contains all its possible targets.
        ///     Note: it is a jagged array and each group may have different number of
        ///     targets.
        ///     We use array because it has quick access property which we will use heavily
        /// </summary>
        /// <returns>Matrix of all possible targets</returns>
        private static Score[][] GetPossibleTargetMatrix()
        {
            //allocate array of array: array for each group's possible targets
            Score[][] possibleTargets = new Score[Commander.Groups.Count][];

            for (int i = 0; i < Commander.Groups.Count; i++)
            {
                //convert the priority list to an array (to enable quick access)
                possibleTargets[i] = Commander.Groups[i].Priorities.ToArray();
            }

            //return the matrix
            return possibleTargets;
        }

        /// <summary>
        ///     Given all the possible assignments, and a specific assignment (given by array of indexes)
        ///     returns the actual scores corresponding to this assignment
        /// </summary>
        /// <param name="possibleAssignments">jagged array of all possible assignments</param>
        /// <param name="assignment">indexes of this assignment</param>
        /// <returns></returns>
        private static Score[] GetSpecificAssignmentScores(Score[][] possibleAssignments, int[] assignment)
        {
            //declare the array to later be returned
            Score[] scoreArr = new Score[possibleAssignments.Length];

            //fill the array with the appropriate values
            for (int i = 0; i < scoreArr.Length; i++)
            {
                //fill the i'th place of the score array with the target of the i'th group in the assignment index
                scoreArr[i] = possibleAssignments[i][assignment[i]];
            }

            //return the result
            return scoreArr;
        }

        /// <summary>
        ///     Get the dimension vector which later will be used to create the iteration
        ///     over all possible Group-Target assignments
        /// </summary>
        /// <returns>array of numbers of priorities for each group</returns>
        private static int[] GetTargetsDimensions()
        {
            //allocate a new array for the dimensions of each group's target
            int[] dimensions = new int[Commander.Groups.Count];

            //go over all the groups and read number of priorities to dimension
            for (int i = 0; i < Commander.Groups.Count; i++)
                dimensions[i] = Commander.Groups[i].Priorities.Count;

            return dimensions;
        }

        /// <summary>
        ///     this method forces all groups to calculate their priorities
        /// </summary>
        /// <param name="cancellationToken"></param>
        private static void StartCalcPriorities(CancellationToken cancellationToken)
        {
            Commander.Groups.ForEach(g => g.CalcPriorities(cancellationToken));
            Logger.Write("Priorities Calculated", true);
        }

        /// <summary>
        ///     Moves priates between group if needed (physically)
        /// </summary>
        public static void FixGroupArrangement()
        {
            //going over all pair of groups
            foreach (Group g1 in Commander.Groups)
            {
                foreach (Group g2 in Commander.Groups)
                {
                    //check if it is the same
                    if (g1.Equals(g2))
                        continue;

                    //check if they are messed out
                    if (Group.CheckIfGroupsIntersects(g1, g2))
                    {
                        //find larger group
                        if (g1.Pirates.Count > g2.Pirates.Count)
                        {
                            Group.Switch(g1, g2);
                        }
                        else
                        {
                            Group.Switch(g2, g1);
                        }
                    }
                }
            }
        }
    }
}