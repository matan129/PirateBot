#region #Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     Just stuff that makes the hard decisions
    /// </summary>
    public static class Commander
    {
        #region Static Fields & Consts

        public static Stopwatch TurnTimer;

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
            Bot.Game.Debug("We have {0} pirates in our forces! \n", Bot.Game.AllMyPirates().Count);

            Commander.Groups = new List<Group>();
            Commander.TurnTimer = new Stopwatch();

            #region Terrible Switch-Case

            //TODO initial config should be better then this
            //Hookup the UltimateConfig() here


            if (Bot.Game.Islands().Count == 1)
            {
                Commander.AllocateGroups(AutoCorrectOptions.All ,Bot.Game.AllMyPirates().Count);
                return;
            }

            //TODO this is awfully specific for the game bots. We have to generalize this
            switch (Bot.Game.AllMyPirates().Count)
            {
                case 3:
                    Commander.AllocateGroups(AutoCorrectOptions.All, 2, 1);
                    break;
                case 4:
                    if (Bot.Game.AllEnemyPirates().Count > 4)
                    {
                        Commander.AllocateGroups(AutoCorrectOptions.All, 1, 2, 1);
                    }
                    else
                    {
                        Commander.AllocateGroups(AutoCorrectOptions.All, 3, 1);
                    }
                    break;
                case 5:
                    Commander.AllocateGroups(AutoCorrectOptions.All, 2, 2, 1);
                    break;
                case 6:
                    if (Bot.Game.EnemyIslands().Count > 0)
                    {
                        Commander.AllocateGroups(AutoCorrectOptions.All, 5, 1);
                    }
                    else
                    {
                        Commander.AllocateGroups(AutoCorrectOptions.All, 4, 1, 1);
                    }
                    break;
                case 7:
                    Commander.AllocateGroups(AutoCorrectOptions.All, 2, 3, 2);
                    break;
                case 8:
                    if (Bot.Game.GetMyPirate(7).Loc.Row == 39)
                    {
                        Commander.AllocateGroups(AutoCorrectOptions.All, 4, 3, 1);
                    }
                    else
                    {
                        Commander.AllocateGroups(AutoCorrectOptions.All, 3, 2, 2, 1);
                    }
                    break;
                case 9:
                    Commander.AllocateGroups(AutoCorrectOptions.All, 3, 3, 2, 1);
                    break;
                default:
                    for (int i = 0; i < Bot.Game.AllMyPirates().Count - Bot.Game.AllMyPirates().Count % 2; i += 2)
                    {
                        Commander.Groups.Add(new Group(i, 2));
                    }

                    if (Bot.Game.AllMyPirates().Count % 2 == 1)
                        Commander.Groups.Add(new Group(Bot.Game.AllMyPirates().Count, 1));

                    break;
            }

            #endregion
        }

        #endregion

        /// <summary>
        ///     This method allocates group
        /// </summary>
        /// <param name="config">The configuration of the ships (i.e {2 , 2, 1})</param>
        /// <param name="autoCorrectLevel">If to auto correct mistakes and zone splits</param>
        private static void AllocateGroups(AutoCorrectOptions autoCorrectLevel, params int[] config)
        {
            config = Commander.AutoCorrectConfig(autoCorrectLevel, config);
            
            //auto ditribute and correct by zones
            if (autoCorrectLevel >= AutoCorrectOptions.Zone) 
            {
                Bot.Game.Debug("Adjusting config according to zones...");

                //split our pirates to zones.
                List<List<int>> myZones = Commander.SplitToZones();

                //sort the zones by size (bigger first)
                myZones.Sort((a, b) => a.Count.CompareTo(b.Count));
                
                //sort the config (bigger values first)
                Array.Sort(config, (a, b) => a.CompareTo(b));

                //i.e. 4, {2,2} = zone of 4 pirates divided to 2 groups of two
                List<ZoneConfigs> zoneConfig = new List<ZoneConfigs>(myZones.Count);

                //init list
                foreach (List<int> zone in myZones)
                {
                    if(zone.Count == 0)
                        continue;

                    zoneConfig.Add(new ZoneConfigs(zone.Count));
                }
                
                //first of all, match groups which fill 100% of a zone
                foreach (ZoneConfigs zone in zoneConfig)
                {
                    if (zone.Capacity == 0)
                        continue;

                    for (int k = 0; k < config.Length; k++)
                    {
                        if(config[k] == 0)
                            continue;

                        if (config[k] == zone.Capacity)
                        {
                            zone.AddGroup(config[k]);
                            config[k] = 0;
                        }
                    }
                }
                
            firstRound:
                //first matching round
                foreach (ZoneConfigs zone in zoneConfig)
                {
                    if (zone.Capacity == 0)
                        continue;

                    for (int k = 0; k < config.Length; k++)
                    {
                        int gConf = config[k];

                        if (gConf == 0)
                            continue;

                        if (zone.Capacity >= gConf)
                        {
                            zone.AddGroup(gConf);
                            config[k] = 0;
                        }
                    }
                }

                //second match round. If required, it will fragmate the groups
                foreach (ZoneConfigs zone in zoneConfig)
                {
                    //skip full zones
                    if (zone.Capacity == 0)
                        continue;

                    for (int i = 0; i < config.Length; i++)
                    {
                        //skip over matched groups
                        if (config[i] == 0)
                            continue;

                        config[i] -= zone.Capacity;
                        zone.AddGroup(zone.Capacity);
                    }
                }

                if(zoneConfig.Any(z => z.Capacity != 0))
                    goto firstRound; //kill me.

                //finally, allocate forces
                for (int i = 0; i < zoneConfig.Count; i++)
                {
                    Commander.AllocateZone(myZones[i], zoneConfig[i].Groups);
                }
            }
        }

        private static void AllocateZone(List<int> piratesInZone, List<int> config)
        {
            if(piratesInZone.Count != config.Sum())
                throw new AllocationException(string.Format("Excepcted {0} pirates in zone config but got  {1}", piratesInZone.Count, config.Sum()));

            Pirate[] pirates = piratesInZone.ConvertAll(p => Bot.Game.GetMyPirate(p)).ToArray();

            foreach (int c in config)
            {
                
            }

        }


        /// <summary>
        ///     Auto corrects a given config
        /// </summary>
        /// <param name="autoCorrectLevel">The autocorrect level (see AutoCorrectOptions enum)</param>
        /// <param name="config">The possibly bad config </param>
        /// <returns>An autocorrected config</returns>
        private static int[] AutoCorrectConfig(AutoCorrectOptions autoCorrectLevel, int[] config)
        {
            int sum = config.Sum();
            int pirateCount = Bot.Game.AllMyPirates().Count;

                //Check # of pirates and alert if something is wrong
            if (sum != pirateCount)
                if (autoCorrectLevel == AutoCorrectOptions.None)
                    throw new AllocationException(string.Format("ALLOCATED ERROR: Expected {0} pirates, but got {1}",
                        pirateCount, sum));
                else
                {
                    Bot.Game.Debug("ALLOCATED WARNING: Expected {0} pirates, but got {1}", pirateCount, sum);

                    //Autocorrecting
                    List<int> correctedConfig = new List<int>();

                    //Sort the array by length (critical for autocrrect). Bigger values first
                    Array.Sort(config, (a, b) => a.CompareTo(b));

                    if (sum > pirateCount)
                    {
                        if (autoCorrectLevel < AutoCorrectOptions.Higher)
                            throw new AllocationException(string.Format("ALLOCATED ERROR: Expected {0} pirates, but got {1}",
                                pirateCount, sum));

                        Bot.Game.Debug("Correcting config...");

                        int roof = 0;
                        int i = 0;
                        int delta = 0;

                        while (roof <= pirateCount && i < config.Length)
                        {
                            int currGroup = config[i];

                            if (roof + currGroup <= pirateCount)
                            {
                                correctedConfig.Add(currGroup);
                                roof += currGroup;
                            }
                            else
                            {
                                delta = Math.Abs(pirateCount - roof);
                                break;
                            }

                            i++;
                        }

                        //add the delta to the
                        correctedConfig[correctedConfig.Count - 1] += delta;
                        config = correctedConfig.ToArray();
                    }
                    else //this means that the config is using less pirates than possible.
                    {
                        if (autoCorrectLevel < AutoCorrectOptions.Lower)
                            throw new AllocationException(
                                string.Format("ALLOCATED ERROR: Expected {0} pirates, but got {1}", pirateCount, sum));

                        Bot.Game.Debug("Correcting config...");

                        //So just add couple of pirates to the smallest group
                        config[config.Length - 1] += pirateCount - sum;
                    }
                }
            return config;
        }


        /// <summary>
        ///  Does analysis to OUR pirates, useful for zone detection
        /// </summary>
        /// <returns></returns>
        private static List<List<int>> SplitToZones()
        {
            throw new NotImplementedException();
        }

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
                // Bot.Game.Debug("Group {0} assinged to {1} at location {2}", i, scoreArr[i].Target.GetDescription());
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
        ///     score class is not finished yet so meanwhile it is pretty dumb
        /// </summary>
        /// <param name="scoreArr">array of local scores</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static double GlobalizeScore(Score[] scoreArr, CancellationToken cancellationToken)
        {
            //TODO this is not close to be finished + we need some smarter constants here 
            double score = 0;
            double timeAvg = 0;
            double value = 0;
            double enemyShips = 0;

            foreach (Score s in scoreArr)
            {
                //Throwing an exception if cancellation was requested.
                cancellationToken.ThrowIfCancellationRequested();


                value += s.Value;
                enemyShips += s.EnemyShips;
                /*score += Math.Pow(2, Bot.Game.MyIslands().Count + s.Value);
                score += 0.2 * Math.Pow(3, s.EnemyShips);*/
                timeAvg += s.Eta;
            }

            //check if there are two of the same target
            for (int i = 0; i < scoreArr.Length - 1; i++)
            {
                //Throwing an exception if cancellation was requested.
                cancellationToken.ThrowIfCancellationRequested();

                for (int j = i + 1; j < scoreArr.Length; j++)
                {
                    //Throwing an exception if cancellation was requested.
                    cancellationToken.ThrowIfCancellationRequested();

                    if (scoreArr[i].Target.Equals(scoreArr[j].Target))
                        score -= 1000;
                }
            }

            return score + Math.Pow(2, value) + 0.2 * Math.Pow(3, enemyShips) - timeAvg / scoreArr.Length;
        }

        /// <summary>
        ///     Checks if a specific pirate is already occupied in some group
        /// </summary>
        /// <param name="pirate">id of the target</param>
        /// <returns>true if it is already assigned, false otherwise</returns>
        public static bool IsEmployed(int pirate)
        {
            //going over all the groups searching for the specific pirate
            foreach (Group group in Commander.Groups)
            {
                //if found return true
                if (group.Pirates.Contains(pirate))
                    return true;
            }
            //else return false
            return false;
        }

        /// <summary>
        ///     Do something!
        /// </summary>
        public static Dictionary<Pirate, Direction> Play(CancellationToken cancellationToken, out bool onTime)
        {
            Commander.TurnTimer.Restart();

            //note that because this method is on a separate thread we need this try-catch although we have on our bot
            try
            {
                TheD.BeginTime("Update");
                //update the enemy info
                Enemy.Update(cancellationToken);
                TheD.StopTime("Update");

                //update smartIslands
                SmartIsland.UpdateAll();

                TheD.BeginTime("CalculateAndAssignTargets");
                //calculate targets
                Commander.CalculateAndAssignTargets(cancellationToken);
                TheD.StopTime("CalculateAndAssignTargets");


                TheD.BeginTime("GetAllMoves");
                //Get the moves for all the pirates and return them
                Dictionary<Pirate, Direction> moves = Commander.GetAllMoves(cancellationToken);
                TheD.StopTime("GetAllMoves");
                Bot.Game.Debug("Commander done doing calculations and drinking coffee after {0}ms",
                    Commander.TurnTimer.ElapsedMilliseconds);

                //we are on time!
                onTime = true;

                return moves;
            }
            catch (AggregateException ex)
            {
                Bot.Game.Debug("****** COMMANDER EXITING DUE TO AggregateException ******");
                foreach (Exception e in ex.InnerExceptions)
                    Bot.Game.Debug(e.ToString());
                onTime = false;
                TheD.Debug();
                return new Dictionary<Pirate, Direction>();
            }
            catch (OperationCanceledException) //catch task cancellation
            {
                Bot.Game.Debug("****** COMMANDER EXITING DUE TO TASK CANCELLATION ******");
                onTime = false;
                TheD.Debug();
                return new Dictionary<Pirate, Direction>();
            }
            catch (Exception ex) //catch everyting else
            {
                Bot.Game.Debug("==========COMMANDER EXCEPTION============");
                Bot.Game.Debug("Commander almost crashed because of exception: " + ex.Message);

                //Holy shit. This actually works!!
                StackTrace exTrace = new StackTrace(ex, true);
                StackFrame frame = exTrace.GetFrame(0);
                Bot.Game.Debug("The exception was thrown from method {0} at file {1} at line #{2}", frame.GetMethod(),
                    frame.GetFileName(), frame.GetFileLineNumber());

                Bot.Game.Debug("==========COMMANDER EXCEPTION============");
                onTime = false;
                TheD.Debug();
                return new Dictionary<Pirate, Direction>();
            }
        }

        /// <summary>
        ///     Gets all th moves for each pirate in each group
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A dictionary that gives each pirate a direction to move to in this turn</returns>
        private static Dictionary<Pirate, Direction> GetAllMoves(CancellationToken cancellationToken)
        {
            //A list with all the moves from all groups
            List<KeyValuePair<Pirate, Direction>> allMoves =
                new List<KeyValuePair<Pirate, Direction>>(Bot.Game.AllMyPirates().Count);
            //Get the moves from each group we have
            foreach (Group group in Commander.Groups)
                allMoves.AddRange(group.GetGroupMoves(cancellationToken));
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
            //this may calculate some of the stuff in parallel and therefore faster 
            Parallel.ForEach(Commander.Groups, g => g.CalcPriorities(cancellationToken));

            Bot.Game.Debug("Priorities Calculated");
        }
    }
}