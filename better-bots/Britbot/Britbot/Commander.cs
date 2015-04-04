namespace Britbot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Pirates;

    /// <summary>
    ///     Just stuff that makes the hard decisions
    /// </summary>
    public static class Commander
    {
        /// <summary>
        ///     List of groups of our pirates
        /// </summary>
        public static List<Group> Groups { get; private set; }


        private static bool _initFlag = false;
        /// <summary>
        ///     This static constructor will run once and initialize the commander
        /// </summary>
        public static void Init()
        {
            if (Commander._initFlag) return;

            Commander._initFlag = true;
            try
            {
                Bot.Game.Debug("We have {0} pirates in our forces! \n", Bot.Game.AllMyPirates().Count);

                Commander.Groups = new List<Group>();

                //TODO initial config should be better then this

                if (Bot.Game.Islands().Count == 1)
                {
                    Commander.Groups.Add(new Group(0, Bot.Game.AllMyPirates().Count));
                    return;
                }

                #region Terrible Switch-Case

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
                            //Commander.Groups.Add(new Group(2, 1));
                        }
                        else
                        {
                            Commander.Groups.Add(new Group(0, 3));
                            Commander.Groups.Add(new Group(3, 1));
                        }
                        break;
                    case 5:
                        Commander.Groups.Add(new Group(0, 2));
                        Commander.Groups.Add(new Group(2, 2));
                        Commander.Groups.Add(new Group(4, 1));
                        break;
                    case 6:
                        if(Bot.Game.EnemyIslands().Count > 0)
                        {
                            Commander.Groups.Add(new Group(0, 5));
                            Commander.Groups.Add(new Group(5, 1));
                        }
                        else
                        {
                            Commander.Groups.Add(new Group(0, 1));
                            Commander.Groups.Add(new Group(1, 3));
                            Commander.Groups.Add(new Group(4, 1));
                            Commander.Groups.Add(new Group(5, 1));
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
                        Commander.Groups.Add(new Group(0, 9));
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
            catch (Exception ex)
            {
                Bot.Game.Debug("==========COMMANDER EXCEPTION============");
                Bot.Game.Debug("Commander almost crashed because of exception: " + ex.Message);
                Bot.Game.Debug("==========COMMANDER EXCEPTION============");
            }
        }

        /// <summary>
        ///     Assigns targets to each group based on pure magic
        ///     Also initiate local scoring
        /// </summary>
        public static void CalculateAndAssignTargets()
        {
            //force groups to calculate priorities
            Commander.StartCalcPriorities();

            //read dimensions of iteration
            int[] dimensions = Commander.GetTargetsDimensions();

            //read all possible target-group assignment
            Score[][] possibleAssignments = Commander.GetPossibleTargetMatrix();

            //indexes of the best assignment yet
            int[] maxAssignment = new int[dimensions.Length];
            int maxScore = 0;

            //create new iteration object
            ExpIterator iterator = new ExpIterator(dimensions);

            //Score array for calculations in each iteration
            Score[] scoreArr = new Score[dimensions.Length];

            //iterating over all possible target assignments
            do
            {
                //set score array for current iteration
                scoreArr = Commander.GetSpeciphicAssignmentScores(possibleAssignments, iterator.Values);

                //calculate new score
                int newScore = (int) Commander.GlobalizeScore(scoreArr);

                //check if the score is better
                if (newScore > maxScore)
                {
                    //replace best
                    maxScore = newScore;
                    Array.Copy(iterator.Values, maxAssignment, iterator.Values.Length);
                }
            } while (iterator.NextIteration());

            //read the "winning" assignment
            scoreArr = Commander.GetSpeciphicAssignmentScores(possibleAssignments, maxAssignment);

            //no we got the perfect assignment, just set it up
            for (int i = 0; i < dimensions.Length; i++)

                Commander.Groups[i].SetTarget(scoreArr[i].Target);

            #region Debug Prints

            Bot.Game.Debug("=====================TARGETS===================");
            for (int i = 0; i < dimensions.Length; i++)
                Bot.Game.Debug(possibleAssignments[i][maxAssignment[i]].Target.GetDescription());
            Bot.Game.Debug("=====================TARGETS===================");

            #endregion
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
        /// <returns></returns>
        public static double GlobalizeScore(Score[] scoreArr)
        {
            //TODO this is not finished + we need some smarter constants here 
            double score = 0;
            double timeAvg = 0;

            foreach (Score s in scoreArr)
            {
                if (s.Type == TargetType.Island)
                    score += 100*s.Value;
                else if (s.Type == TargetType.EnemyGroup)
                    score += 200*s.Value;

                timeAvg += s.Eta;
            }

            //check if there are two of the same target
            for (int i = 0; i < scoreArr.Count() - 1; i++)
            {
                for (int j = i + 1; j < scoreArr.Length; j++)
                {
                    if (scoreArr[i].Target.Equals(scoreArr[j].Target))
                        score -= 100000000;
                }
            }

            return (score*scoreArr.Length)/(timeAvg/scoreArr.Length);
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
        public static Dictionary<Pirate, Direction> Play()
        {
            int enteringTurn = Bot.Game.GetTurn();

            //note that because this method is on a separate thread we need this try-catch although we have on our bot
            try
            {
                //update the enemy info
                Enemy.Update();

                //calculate targets
                Commander.CalculateAndAssignTargets();

                //Get the moves for all the pirates and return them
                Dictionary<Pirate,Direction> moves = Commander.GetAllMoves();

                if (Bot.Game.GetTurn() == enteringTurn)
                    return moves;
                else
                    return null;
            }
            catch (Exception ex)
            {
                Bot.Game.Debug("==========COMMANDER EXCEPTION============");
                Bot.Game.Debug("Commander almost crashed because of exception: " + ex.Message);
                Bot.Game.Debug("==========COMMANDER EXCEPTION============");
            
                return null;
            }
        }

        /// <summary>
        ///     Gets all th moves for each pirate in each group
        /// </summary>
        /// <returns>A dictionary that gives each pirate a direction to move to in this turn</returns>
        private static Dictionary<Pirate, Direction> GetAllMoves()
        {
            //A list with all the moves from all groups
            List<KeyValuePair<Pirate, Direction>> allMoves =
                new List<KeyValuePair<Pirate, Direction>>(Bot.Game.AllMyPirates().Count);

            //Get the moves from each group we have
            foreach (Group group in Commander.Groups)
                allMoves.AddRange(group.GetGroupMoves());

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
        private static Score[] GetSpeciphicAssignmentScores(Score[][] possibleAssignments, int[] assignment)
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
        private static void StartCalcPriorities()
        {
            foreach (Group group in Commander.Groups)
                group.CalcPriorities();
        }
    }
}