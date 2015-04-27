#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Britbot.Simulator;
using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     A top level manager that does the turn
    /// </summary>
    public static class Commander
    {
        #region Static Fields & Consts

        /// <summary>
        ///     Timer that keeps track on the time each turn takes
        /// </summary>
        private static Stopwatch _turnTimer;

        /// <summary>
        ///     List of the dead pirates indexes
        /// </summary>
        private static List<int> _deadPirates;

        #endregion

        #region Fields & Properies

        /// <summary>
        ///     List of groups of our pirates
        /// </summary>
        public static List<Group> Groups { get; private set; }

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     This static constructor will run once and initialize the commander
        /// </summary>
        static Commander()
        {
            Groups = new List<Group>();
            _turnTimer = new Stopwatch();
            _deadPirates = new List<int>();

            #region Terrible Switch-Case

            //TODO initial config should be better then this
            //Hookup the UltimateConfig() here

            if (Bot.Game.Islands().Count == 1)
            {
                Groups.Add(new Group(0, Bot.Game.AllMyPirates().Count));
                return;
            }

            //TODO this is awfully specific for the game bots. We have to generalize this
            switch (Bot.Game.AllMyPirates().Count)
            {
                case 3:
                    Groups.Add(new Group(0, 2));
                    Groups.Add(new Group(2, 1));
                    break;
                case 4:
                    if (Bot.Game.AllEnemyPirates().Count > 4)
                    {
                        Groups.Add(new Group(0, 1));
                        Groups.Add(new Group(1, 1));
                        Groups.Add(new Group(2, 2));
                    }
                    else
                    {
                        Groups.Add(new Group(0, 3));
                        Groups.Add(new Group(3, 1));
                        ;
                    }
                    break;
                case 5:
                    Groups.Add(new Group(0, 2));
                    Groups.Add(new Group(2, 2));
                    Groups.Add(new Group(4, 1));
                    break;
                case 6:
                    if (Bot.Game.EnemyIslands().Count > 0)
                    {
                        Groups.Add(new Group(0, 5));
                        Groups.Add(new Group(5, 1));
                    }
                    else
                    {
                        Groups.Add(new Group(2, 4));
                        Groups.Add(new Group(0, 1));
                        Groups.Add(new Group(1, 1));
                    }
                    break;
                case 7:
                    Groups.Add(new Group(0, 2));
                    Groups.Add(new Group(2, 3));
                    Groups.Add(new Group(5, 2));

                    break;
                case 8:
                    if (Bot.Game.GetMyPirate(7).Loc.Row == 39)
                    {
                        Groups.Add(new Group(0, 4));
                        Groups.Add(new Group(4, 3));
                        Groups.Add(new Group(7, 1));
                    }
                    else
                    {
                        Groups.Add(new Group(0, 3));
                        Groups.Add(new Group(3, 2));
                        Groups.Add(new Group(5, 2));
                        Groups.Add(new Group(7, 1));
                    }
                    break;
                case 9:
                    Groups.Add(new Group(0, 3));
                    Groups.Add(new Group(3, 3));
                    Groups.Add(new Group(6, 2));
                    Groups.Add(new Group(8, 1));
                    break;
                default:
                    for (int i = 0; i < Bot.Game.AllMyPirates().Count; i++)
                        Groups.Add(new Group(i, 1));
                    break;
            }

            #endregion
        }

        #endregion

        /// <summary>
        ///     Makes a game move
        /// </summary>
        public static Dictionary<Pirate, Direction> Play(CancellationToken cancellationToken, out bool onTime)
        {
            // Restart the timer
            Commander._turnTimer.Restart();

            // note that because this method is on a separate thread we need this try-catch 
            // although we have one on our bot
            try
            {
                /*
                 * "Initiating Commander Sequence..."
                 */

                //dump Magic configuration and current stats
                Magic.DumpLog();

                //remove dead groups
                Commander.Groups.RemoveAll(g => g.Pirates.Count == 0);

                //re allocates the new revived pirates
                Commander.AllocateRevived();

                //update the enemy analysis and info
                Enemy.Update(cancellationToken);

                //update smartIslands
                SmartIsland.UpdateAll();

                //merge similar groups for extra power
                Commander.MergeSimilar();

                //calculate targets
                Commander.CalculateAndAssignTargets(cancellationToken);

                //fix configuration
                ConfigHelper.ReConfigure();

                //swap pirates on groups the collide
                Commander.FixGroupArrangement();

                //Get the moves for all the pirates and return them
                Dictionary<Pirate, Direction> moves = GetAllMoves(cancellationToken);

                //cloacking override
                SpecialOps.DoCloak(moves);

                //update dead pirates list
                Commander._deadPirates = Bot.Game.AllMyPirates().Where(p => p.IsLost).ToList().ConvertAll(p => p.Id);

                //we are done for this turn
                Logger.Write(
                    string.Format("Commander done doing calculations and drinking coffee after {0}ms",
                        _turnTimer.ElapsedMilliseconds), true);

                //we are on time!
                onTime = true;

                //return the carefully crafted moves
                return moves;
            }
            catch (OperationCanceledException) //catch task cancellation
            {
                Logger.Write("****** COMMANDER EXITING DUE TO TASK CANCELLATION ******", true);

                //lower the max iteration bound so we will stop to timeout
                Magic.MaxIterator = (int)(Magic.MaxIterator * 0.93);

                //we are not on time
                onTime = false;

                //do some profiling
                Logger.Profile();

                //don't return null
                return new Dictionary<Pirate, Direction>();
            }
            catch (Exception ex) //catch everyting else
            {
                Logger.Write("==========COMMANDER EXCEPTION============", true);
                Logger.Write("Commander almost crashed because of exception: " + ex.Message, true);

                //print stack trace
                StackTrace exTrace = new StackTrace(ex, true);
                StackFrame frame = exTrace.GetFrame(0);
                Logger.Write(
                    string.Format("The exception was thrown from method {0} at file {1} at line #{2}", frame.GetMethod(),
                        frame.GetFileName(), frame.GetFileLineNumber()), true);

                Logger.Write("==========COMMANDER EXCEPTION============", true);

                //we are no on time
                onTime = false;

                //di some prodiling
                Logger.Profile();

                //don't return null
                return new Dictionary<Pirate, Direction>();
            }
        }

        /// <summary>
        ///     Re-allocated revived pirates
        /// </summary>
        private static void AllocateRevived()
        {
            List<int> alive = Bot.Game.AllMyPirates().Where(p => !p.IsLost).ToList().ConvertAll(p => p.Id);
            List<int> revived = alive.Intersect(_deadPirates).ToList();

            foreach (int pid in revived)
                Groups.Add(new Group(new[] {pid}));
        }

        /// <summary>
        ///     Max proirities we can afford to compute and iterate over
        /// </summary>
        /// <returns>The number of maximun priorites each group can have</returns>
        public static int CalcMaxPrioritiesNum()
        {
            return (int) (Math.Pow(Magic.MaxIterator, 1.0 / Commander.Groups.Count));
        }

        /// <summary>
        ///     Assigns targets to each group based on pure magic
        ///     Also initiate local scoring
        /// </summary>
        /// <param name="cancellationToken"></param>
        private static void CalculateAndAssignTargets(CancellationToken cancellationToken)
        {
            //force groups to calculate priorities
            Commander.StartCalcPriorities(cancellationToken);

            //read dimensions of iteration
            int[] dimensions = Commander.GetTargetsDimensions();

            //read all possible target-group assignment
            Score[][] possibleAssignments = Commander.GetPossibleTargetMatrix();

            //indexes of the best assignment yet
            int[] maxAssignment = new int[dimensions.Length];

            //maxScore setup
            double maxScore = Int32.MinValue;

            //create new iteration object
            ExpIterator iterator = new ExpIterator(dimensions);

            //Score array for calculations in each iteration
            Score[] scoreArr;

            //create new simulated game
            SimulatedGame sg = new SimulatedGame();

            //iterating over all possible target assignments
            do
            {
                //Throwing an exception if cancellation was requested.
                cancellationToken.ThrowIfCancellationRequested();

                //set score array for current iteration
                scoreArr = Commander.GetSpecificAssignmentScores(possibleAssignments, iterator.Values);

                //calculate new score
                double newScore = Commander.GlobalizeScore(sg, scoreArr, cancellationToken);

                //check if the score is better
                if (newScore > maxScore)
                {
                    //replace best
                    maxScore = newScore;

                    //copy the array to the maxAssigment array
                    Array.Copy(iterator.Values, maxAssignment, iterator.Values.Length);
                }
            } while (iterator.NextIteration());

            //read the "winning" assignment
            scoreArr = Commander.GetSpecificAssignmentScores(possibleAssignments, maxAssignment);

            //now we got the best assignment, so just set it up
            for (int i = 0; i < dimensions.Length; i++)
                Commander.Groups[i].SetTarget(scoreArr[i].Target);
        }

        /// <summary>
        ///     This function should convert an array of local scores into a numeric
        ///     score based on global criteria
        /// </summary>
        /// <param name="sg">The simulatedGame instace to evaluate the score with</param>
        /// <param name="scoreArr">array of local scores</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static double GlobalizeScore(SimulatedGame sg, Score[] scoreArr, CancellationToken cancellationToken)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (Magic.UseBasicGlobalizing) //use simple globalizing if needed
            {
                #region Basic Globalizing
                double totalDensityBonus = 0;

                double score = 0;
                score += Math.Pow(2, scoreArr.Sum(a => a.Value)) * 20;

                foreach (Score s in scoreArr)
                {
                    score -= s.Eta;
                    score += totalDensityBonus * Magic.DensityBonusCoefficient;
                }

                for (int i = 0; i < scoreArr.Length - 1; i++)
                {
                    for (int j = i + 1; j < scoreArr.Length; j++)
                    {
                        if (scoreArr[i].Target.Equals(scoreArr[j].Target))
                            score -= 5000;
                    }
                }

                for (int i = 0; i < scoreArr.Length; i++)
                {
                    if (scoreArr[i].Target == Groups[i].Target)
                    {
                        int bonus = (int) Math.Abs(score * Magic.DecisivenessBonus);
                        score += bonus;
                    }
                    if (scoreArr[i].EnemyShips >= Groups[i].Pirates.Count)
                        score -= 5000;
                }

                return score;
                #endregion
            }
            else //actually simulated the game and determine the score by it
            {
                //reset simulation
                sg.ResetSimulation();

                //setup simulation events
                for (int i = 0; i < scoreArr.Length; i++)
                {
                    switch (scoreArr[i].Type)
                    {
                        case TargetType.Island:
                            sg.AddEvent(new GroupArrivalEvent((int) scoreArr[i].Eta,
                                sg.Islands[((SmartIsland) (scoreArr[i].Target)).Id], sg.MyGroups[Commander.Groups[i].Id]));
                            break;
                        case TargetType.EnemyGroup:
                            sg.AddEvent(new BattleEvent((int) scoreArr[i].Eta,
                            sg.EnemyGroups[((EnemyGroup) (scoreArr[i].Target)).Id], sg.MyGroups[Commander.Groups[i].Id]));
                            break;
                    }
                }

                //run the simulation and return its score
                return sg.RunSimulation(cancellationToken);
            }
        }

        /// <summary>
        ///     Merges similar groups
        /// </summary>
        private static void MergeSimilar()
        {
            //setup merging lists
            List<List<Group>> allMerges = new List<List<Group>>();

            //iterate over all groups
            foreach (Group group in Commander.Groups)
            {
                //create a merging list and add the current group to it
                List<Group> toMerge = new List<Group> {group};

                //check if there are any mering list that should contain the current group
                List<List<Group>> sameTarget =
                    allMerges.Where(
                        gList =>
                            //Basically join all group sharing the same target and in reasonable distance from each other
                            gList.Any(g => g.Target.Equals(group.Target) && g.MinDistance(group) < Magic.MaxJoinDistance))
                        .ToList();

                //if the sameTarget list constains some merging lists
                if (sameTarget.Count > 0)
                {
                    //if there are, remove these merging lists
                    allMerges.RemoveAll(
                        gList =>
                            gList.Any(g => g.Target.Equals(group.Target) && g.MinDistance(group) < Magic.MaxJoinDistance));
                    
                    //merge these lists
                    foreach (List<Group> gList in sameTarget)
                        toMerge.AddRange(gList);
                }

                //add the new group to the list of merges
                allMerges.Add(toMerge);
            }

            //actually merge the group according to the merging lists
            foreach (List<Group> mergeList in allMerges)
            {
                Group first = mergeList.First();

                for (int i = 1; i < mergeList.Count; i++)
                    first.Join(mergeList[i], false);
            }
        }

        /// <summary>
        ///     Gets all th moves for each pirate in each group
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A dictionary that gives each pirate a direction to move to in this turn</returns>
        private static Dictionary<Pirate, Direction> GetAllMoves(CancellationToken cancellationToken)
        {
            //Remove all empty groups
            Commander.Groups.RemoveAll(g => g.Pirates.Count == 0);

            //Update all groups
            Parallel.ForEach(Commander.Groups, g => g.Update());

            //A list with all the moves from all groups
            List<KeyValuePair<Pirate, Direction>> allMoves =
                new List<KeyValuePair<Pirate, Direction>>(Bot.Game.AllMyPirates().Count);

            //Get the moves from each group we have
            foreach (Group group in Commander.Groups)
                allMoves.AddRange(group.GetGroupMoves(cancellationToken).ToList());

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
        private static Score[] GetSpecificAssignmentScores(IReadOnlyList<Score[]> possibleAssignments, int[] assignment)
        {
            //declare the array to later be returned
            Score[] scoreArr = new Score[possibleAssignments.Count];

            //fill the array with the appropriate values
            for (int i = 0; i < scoreArr.Length; i++)
                //fill the i'th place of the score array with the target of the i'th group in the assignment index
                scoreArr[i] = possibleAssignments[i][assignment[i]];

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
            for (int i = 0; i < Groups.Count; i++)
                dimensions[i] = Commander.Groups[i].Priorities.Count;

            return dimensions;
        }

        /// <summary>
        ///     this method forces all groups to calculate their priorities
        /// </summary>
        /// <param name="cancellationToken"></param>
        private static void StartCalcPriorities(CancellationToken cancellationToken)
        {
            Parallel.ForEach(Commander.Groups,g => g.CalcPriorities(cancellationToken));
            Logger.Write("Priorities Calculated");
        }

        /// <summary>
        ///     Moves pirates between groups if needed (physically)
        /// </summary>
        private static void FixGroupArrangement()
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

        /// <summary>
        ///     Checks if the Commander is in defensive or or offensive mode
        /// </summary>
        /// <returns></returns>
        public static bool IsDefensive()
        {
            double ePPT = Math.Floor(Math.Pow(2, Bot.Game.EnemyIslands().Count - 1));
            double myPPT = Math.Floor(Math.Pow(2, Bot.Game.MyIslands().Count - 1));

            double eN = Bot.Game.GetEnemyScore();
            double myN = Bot.Game.GetMyScore();

            double maxTurns = 1000;

            double turnUntilEnemy = (maxTurns - eN) / ePPT;
            double turnUntilMe = (maxTurns - myN) / myPPT;

            if (turnUntilMe < turnUntilEnemy - 50)
                return true;
            return false;
        }
    }
}