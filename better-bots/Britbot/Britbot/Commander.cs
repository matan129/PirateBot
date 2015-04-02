using System;
using System.Collections.Generic;
using System.Linq;

namespace Britbot
{
    /// <summary>
    /// Just stuff that makes the hard decisions
    /// </summary>
    public static class Commander
    {
        /// <summary>
        /// This static constructor will run once and initialize the commander
        /// </summary>
        static Commander()
        {
            Bot.Game.Debug("We have {0} pirates in our forces! \n", Bot.Game.AllMyPirates().Count);

            Groups = new List<Group>();

            //TODO initial config should be better then this

            if (Bot.Game.Islands().Count == 1)
            {
                Groups.Add(new Group(0, Bot.Game.AllMyPirates().Count));
                return;
            }


            switch (Bot.Game.AllMyPirates().Count)
            {
                case 3:
                    Groups.Add(new Group(0, 2));
                    Groups.Add(new Group(2, 1));
                    break;
                case 4:
                    if (Bot.Game.AllEnemyPirates().Count > 4)
                        Groups.Add(new Group(0, 4));
                    else
                    {
                        Groups.Add(new Group(0, 3));
                        Groups.Add(new Group(3, 1));
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
                        Groups.Add(new Group(0, 1));
                        Groups.Add(new Group(1, 1));
                        Groups.Add(new Group(2, 4));
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
                    Groups.Add(new Group(0, 9));
                    break;
                default:
                    for (int i = 0; i < Bot.Game.AllMyPirates().Count - Bot.Game.AllMyPirates().Count%2; i += 2)
                    {
                        Groups.Add(new Group(i, 2));
                    }

                    if (Bot.Game.AllMyPirates().Count%2 == 1)
                        Groups.Add(new Group(Bot.Game.AllMyPirates().Count, 1));
                    Groups.Add(new Group(0, Bot.Game.AllMyPirates().Count));
                    break;
            }
        }

        /// <summary>
        /// List of groups of our pirates
        /// </summary>
        public static List<Group> Groups { get; private set; }

        /// <summary>
        /// Do something!
        /// </summary>
        public static void Play()
        {
            //update the enemy info
            Enemy.Update();

            //calculate targets
            AssignTargets();

            //Move forces
            MoveForces();
        }

        /// <summary>
        /// Distribute our pirates into groups and re-arrange them at the start of the game
        /// </summary>
        /// <param name="config">The new configuration. i.e. {2,2,2} for three groups of two pirates</param>
        public static void DistributeForces(int[] config)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if a specific pirate is already occupied in some group
        /// </summary>
        /// <param name="pirate">id of the target</param>
        /// <returns>true if it is already assigned, false otherwise</returns>
        public static bool IsEmployed(int pirate)
        {
            //going over all the groups searching for the specific pirate
            foreach (Group group in Groups)
            {
                //if found return true
                if (group.Pirates.Contains(pirate))
                    return true;
            }
            //else return false
            return false;
        }

        /// <summary>
        /// Assigns targets to each group based on pure magic
        /// Also initiate local scoring
        /// </summary>
        public static void AssignTargets()
        {
            //force groups to calculate priorities
            StartCalcPriorities();

            //read dimensions of iteration
            int[] dimensions = GetTargetsDimensions();

            //read all possible target-group assignment
            Score[][] possibleAssignments = GetPossibleTargetMatrix();

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
                for (int i = 0; i < dimensions.Length; i++)
                {
                    //set the i-th score to be the current iteration value
                    //of the i-th group
                    scoreArr[i] = possibleAssignments[i][iterator.Values[i]];
                }

                //calculate new score
                int newScore = (int) GlobalizeScore(scoreArr);

                //check if the score is better
                if (newScore > maxScore)
                {
                    //replace best
                    maxScore = newScore;

                    for (int i = 0; i < maxAssignment.Length; i++)
                    {
                        maxAssignment[i] = iterator.Values[i];
                    }
                }
            } while (iterator.NextIteration());

            //no we got the perfect assignment, just set it up
            for (int i = 0; i < dimensions.Length; i++)
            {
                Groups[i].SetTarget(possibleAssignments[i][maxAssignment[i]].Target);
            }

            #region Debug Prints

            Bot.Game.Debug("----------TARGETS--------------");
            for (int i = 0; i < dimensions.Length; i++)
            {
                Bot.Game.Debug(possibleAssignments[i][maxAssignment[i]].Target.GetDescription());
            }
            Bot.Game.Debug("----------TARGETS--------------");

            #endregion
        }

        /// <summary>
        /// This function should convert an array of local scores into a numeric
        /// score based on global criteria
        /// score class is not finished yet so meanwhile it is pretty dumb
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
                {
                    score += 100*s.Value;
                }
                else if (s.Type == TargetType.EnemyGroup)
                {
                    score += 200*s.Value;
                }

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
        /// this method forces all groups to calculate their priorities
        /// </summary>
        private static void StartCalcPriorities()
        {
            foreach (Group group in Groups)
            {
                group.CalcPriorities();
            }
        }

        /// <summary>
        /// This function goes over all the groups, reads their priorities and
        /// arranges them in a 2-dimensional array: Each group has it's own row
        /// which contains all its possible targets.
        /// Note: it is a jagged array and each group may have different number of 
        /// targets.
        /// We use array because it has quick access property which we will use heavily
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
        /// Get the dimension vector which later will be used to create the iteration
        /// over all possible Group-Target assignments
        /// </summary>
        /// <returns>array of numbers of priorities for each group</returns>
        private static int[] GetTargetsDimensions()
        {
            //allocate a new array for the dimensions of each group's target
            int[] dimensions = new int[Groups.Count];

            //go over all the groups and read number of priorities to dimension
            for (int i = 0; i < Groups.Count; i++)
            {
                dimensions[i] = Groups[i].Priorities.Count;
            }

            return dimensions;
        }

        /// <summary>
        /// makes all the groups move based on their targets
        /// </summary>
        private static void MoveForces()
        {
            foreach (Group group in Groups)
                group.Move();
        }
    }
}