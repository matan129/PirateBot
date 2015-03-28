using System;
using System.Collections.Generic;

namespace Britbot
{
    /// <summary>
    /// Just stuff that makes the hard decisions
    /// </summary>
    public static class Commander
    {
        public static List<Group> Groups { get; private set; }

        /// <summary>
        /// This static constructor (yeah, I know it's odd) will run once and initialize the commander
        /// </summary>
        static Commander()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Do something!
        /// </summary>
        public static void Play()
        {
            throw new NotImplementedException();
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
        /// Assigns targets to each group based on pure magic
        /// </summary>
        public static void AssignTargets()
        {
            //read dimensions of iteration
            int[] dimensions = GetTargetsDimensions();

            //read all possible target-group assignment
            Score[][] possibleAssignments = GetPossibleTargetMatrix();


            //WHAT is going on here? more explanations plese Matan (Kom)
            //indecies of the best assignment yet
            int[] maxAssignment = new int[dimensions.Length];
            int maxScore = 0;

            //create new iteration object
            ExpIterator eit = new ExpIterator(dimensions);

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
                    scoreArr[i] = possibleAssignments[i][eit.Values[i]];
                }

                //calculate new score
                int newScore = GlobalScore(scoreArr);

                //check if the score is better
                if (newScore > maxScore)
                {
                    //replace best
                    maxScore = newScore;
                    maxAssignment = eit.Values;
                }
            } while (eit.AdvanceIteration());

            //no we got the perfect assignment, just set it up
            for (int i = 0; i < dimensions.Length; i++)
            {
                Groups[i].SetTarget(possibleAssignments[i][maxAssignment[i]].Target);
            }
        }

        /// <summary>
        /// This function shpud convert an array of local scores into a numeric
        /// score based on global criteria
        /// score class is not finished yet so meanwile it is pretty dumb
        /// </summary>
        /// <param name="scoreArr">array of local scores</param>
        /// <returns></returns>
        public static int GlobalScore(Score[] scoreArr)
        {
            //TODO implement this
            int score = 0;
            foreach (Score s in scoreArr)
            {
                //THIS IS COMPLETE BULLSHIT BUT THE SCORE CLASS ISNT READY YET
                score += s.Value;
            }
            return score;
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
    }
}