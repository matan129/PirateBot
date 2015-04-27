#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

        private static List<int> _deadPirates;

        /// <summary>
        ///     Makes a game move
        /// </summary>
        public static Dictionary<Pirate, Direction> Play(CancellationToken cancellationToken, out bool onTime)
        {
            // Restart the timer
            _turnTimer.Restart();

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
                
                //override, for cloak stuff
                ConfigHelper.DoCloak(moves);

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

                //LOWER THE ITERATION NUMBER!!
                Magic.MaxIterator -= 250;

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

        private static void AllocateRevived()
        {
            List<int> alive = Bot.Game.AllMyPirates().Where(p => !p.IsLost).ToList().ConvertAll(p => p.Id);
            List<int> revived = alive.Intersect(Commander._deadPirates).ToList();

            foreach (int pid in revived)
                Commander.Groups.Add(new Group(new[] {pid}));
        }

        /// <summary>
        ///     Max proirities we can affort to compute and iterate over
        /// </summary>
        /// <returns></returns>
        public static int CalcMaxPrioritiesNum()
        {
            return (int) (Math.Pow(Magic.MaxIterator, 1.0 / Groups.Count));
        }

        /// <summary>
        ///     Assigns targets to each group based on pure magic
        ///     Also initiate local scoring
        /// </summary>
        /// <param name="cancellationToken"></param>
        public static void CalculateAndAssignTargets(CancellationToken cancellationToken)
        {
            //force groups to calculate priorities
            StartCalcPriorities(cancellationToken);

            //read dimensions of iteration
            int[] dimensions = GetTargetsDimensions();

            //read all possible target-group assignment
            Score[][] possibleAssignments = GetPossibleTargetMatrix();

            //indexes of the best assignment yet
            int[] maxAssignment = new int[dimensions.Length];
            double maxScore = -9999999999999;

            //create new iteration object
            ExpIterator iterator = new ExpIterator(dimensions);

            //Score array for calculations in each iteration
            Score[] scoreArr = new Score[dimensions.Length];

            //create new simulated game
            SimulatedGame sg = new SimulatedGame();

            //iterating over all possible target assignments
            do
            {
                //Throwing an exception if cancellation was requested.
                cancellationToken.ThrowIfCancellationRequested();

                //set score array for current iteration
                scoreArr = GetSpecificAssignmentScores(possibleAssignments, iterator.Values);

                //calculate new score
                double newScore = GlobalizeScore(sg, scoreArr, cancellationToken);

                //check if the score is better
                if (newScore > maxScore)
                {
                    //replace best
                    maxScore = newScore;
                    Array.Copy(iterator.Values, maxAssignment, iterator.Values.Length);
                }
            } while (iterator.NextIteration());
            
            //read the "winning" assignment
            scoreArr = GetSpecificAssignmentScores(possibleAssignments, maxAssignment);

            //no we got the perfect assignment, just set it up
            for (int i = 0; i < dimensions.Length; i++)
            {
                //Logger.Write(string.Format("Group {0} assinged to {1}", i, scoreArr[i].Target.GetDescription()), true);
                Groups[i].SetTarget(scoreArr[i].Target);
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
            //Enemy configuration
            int[] eConfig = Enemy.Groups.ConvertAll(group => group.EnemyPirates.Count).ToArray();

            //sort the enemy configuration by size
            Array.Sort(eConfig, (a, b) => b.CompareTo(a));

            //prepare return value
            List<int> ret = new List<int>();

            //count of all of our pirates
            int myPirates = Bot.Game.AllMyPirates().Count;
            
            for (int i = 0; i < eConfig.Length && myPirates > 0; i++)
            {
                if (((eConfig[i] + 1) < myPirates)&&((eConfig[i] +1)<Magic.MaxGroupSize))
                {
                    ret.Add(eConfig[i] + 1);
                    myPirates -= eConfig[i] + 1;
                }
                
            }

            while (myPirates > 0)
            {
                ret.Add(1);
                myPirates--;
            }

            while (ret.Count > Bot.Game.Islands().Count || ret.Count > Magic.MaxGroups)
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
        private static double GlobalizeScore(SimulatedGame sg, Score[] scoreArr, CancellationToken cancellationToken)
        {
            double totalDensityBonus = 0;
            if (Magic.UseBasicGlobalizing)
            {
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
            }
            else
            {
                //reset simulation
                sg.ResetSimulation();

                for (int i = 0; i < scoreArr.Length; i++)
                {
                    if (scoreArr[i].Type == TargetType.Island)
                        sg.AddEvent(new GroupArrivalEvent((int) scoreArr[i].Eta,
                            sg.Islands[((SmartIsland) (scoreArr[i].Target)).Id], sg.MyGroups[Groups[i].Id]));
                    if (scoreArr[i].Type == TargetType.EnemyGroup)
                        sg.AddEvent(new BattleEvent((int) scoreArr[i].Eta,
                            sg.EnemyGroups[((EnemyGroup) (scoreArr[i].Target)).Id], sg.MyGroups[Groups[i].Id]));
                }

                double score = sg.SimulateGame();

                for (int i = 0; i < scoreArr.Length; i++)
                {
                    ITarget it = Groups[i].Target;
                    if (it != null && it.Equals(scoreArr[i].Target))
                    {
                        int bonus= (int)Math.Abs(score * Magic.ScoreConssitencyFactor);
                        if (scoreArr[i].Type == TargetType.Island)
                        {
                            SmartIsland isle = (SmartIsland)scoreArr[i].Target;
                            if (isle.Owner == Consts.ME)
                            {
                                bonus = 0;
                            }
                             
                        }
                        score += bonus;

                //        totalDensityBonus += scoreArr[i].Density;
                    }
                }

                return score /*+ totalDensityBonus * Magic.DensityBonusCoefficient*/;
            }

        }

        /// <summary>
        ///     Merges similar groups
        /// </summary>
        private static void MergeSimilar()
        {
            List<List<Group>> updatedGroups = new List<List<Group>>();

            //iterate over all the alive pirate of the enemy
            foreach (Group group in Groups)
            {
                //create a new group and add the current pirate to it
                List<Group> toMerge = new List<Group> {group};

                //check if there are any older group already containing the current pirate
                List<List<Group>> sameTarget =
                    updatedGroups.Where(
                        gList =>
                            gList.Any(g => g.Target.Equals(group.Target) && g.MinDistance(group) < Magic.MaxJoinDistance))
                        .ToList();

                if (sameTarget.Count > 0)
                {
                    //if there are, remove these groups
                    updatedGroups.RemoveAll(
                    gList =>
                        gList.Any(g => g.Target.Equals(group.Target) && g.MinDistance(group) < Magic.MaxJoinDistance));
                

                    //Add the pirates from the groups we removed to the current new group
                    foreach (List<Group> gList in sameTarget)
                    {
                        toMerge.AddRange(gList);
                    }
                }

                //add the new group to the list of groups
                updatedGroups.Add(toMerge);
            }

            foreach (List<Group> mergeList in updatedGroups)
            {
                Group temp = new Group();

                foreach (Group t in mergeList)
                {
                    temp.Join(t,false);
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
            Groups.RemoveAll(g => g.Pirates.Count == 0);
            
            // Update all groups
            Groups.ForEach(g => g.Update());

            //A list with all the moves from all groups
            List<KeyValuePair<Pirate, Direction>> allMoves  =
                new List<KeyValuePair<Pirate, Direction>>(Bot.Game.AllMyPirates().Count);

            //Get the moves from each group we have
            foreach (Group group in Groups)
            {
                List<KeyValuePair<Pirate, Direction>> v = group.GetGroupMoves(cancellationToken).ToList();
                allMoves.AddRange(v);
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
            Score[][] possibleTargets = new Score[Groups.Count][];

            for (int i = 0; i < Groups.Count; i++)
            {
                //convert the priority list to an array (to enable quick access)
                possibleTargets[i] = Groups[i].Priorities.ToArray();
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
            int[] dimensions = new int[Groups.Count];

            //go over all the groups and read number of priorities to dimension
            for (int i = 0; i < Groups.Count; i++)
                dimensions[i] = Groups[i].Priorities.Count;

            return dimensions;
        }

        /// <summary>
        ///     this method forces all groups to calculate their priorities
        /// </summary>
        /// <param name="cancellationToken"></param>
        private static void StartCalcPriorities(CancellationToken cancellationToken)
        {
            Groups.ForEach(g => g.CalcPriorities(cancellationToken));
            Logger.Write("Priorities Calculated", true);
        }

        /// <summary>
        ///     Moves pirates between group if needed (physically)
        /// </summary>
        private static void FixGroupArrangement()
        {
            //going over all pair of groups
            foreach (Group g1 in Groups)
            {
                foreach (Group g2 in Groups)
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

        public static bool UpdateMood()
        {
            double ePPT = Math.Floor(Math.Pow(2, Bot.Game.EnemyIslands().Count - 1));
            double myPPT = Math.Floor(Math.Pow(2, Bot.Game.MyIslands().Count - 1));

            double eN = Bot.Game.GetEnemyScore();
            double myN = Bot.Game.GetMyScore();

            double max = 1000;

            double turnUntilEnemy = (max - eN ) /ePPT;
            double turnUntilMe = (max - myN) / myPPT;

            if (turnUntilMe < turnUntilEnemy - 50)
                return true;
            return false;
        }
    }
}