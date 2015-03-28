using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Britbot
{
    public static class Commander
    {
        public static List<Group> Groups
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }
    
        public static void Play()
        {
            throw new System.NotImplementedException();
        }

        public static void DistributeForces(int[] arrange)
        {
            throw new System.NotImplementedException();
        }

        public static void AssignTargets()
        {
            //read dimensions of iteration
            int[] dimensions = GetTargetsdimensions();

            //read all possible target-group assignment
            Score [][] possibleAssignments =GetPossibleTargetMatrix();

            //indecies of the best assignment yet
            int[] maxAssignment = new int[dimensions.Length];
            int maxScore = 0;

            //create new iteration object
            ExpIterator eit = new ExpIterator(dimensions);

            //Score array for calculations in each iteration
            Score [] scoreArr = new Score[dimensions.Length];
            //iterating over all possible target assignments
            do
            {
                //set score array for current iteration
                for (int i = 0; i < dimensions.Length; i++)
                {
                    //set the i'th score to be the current iteration value
                    //of the i'th group
                    scoreArr[i] = possibleAssignments[i][eit.values[i]];
                }

                //calculate new score
                int newScore = GlobalScore(scoreArr);

                //check if the score is better
                if (newScore > maxScore)
                {
                    //replace best
                    maxScore = newScore;
                    maxAssignment = eit.values;
                }

            } while (eit.AdvanceIteration());

            //no we got the perfect assignment, just set it up
            for (int i = 0; i < dimensions.Length; i++)
            {
                Groups[i].Target = possibleAssignments[i][maxAssignment[i]].target;
            }
        }


        /// <summary>
        /// This function shpud convert an array of local scores into a numeric
        /// score based on global critirions
        /// score struct is not finished yet so meanwile it is pretty dumb
        /// </summary>
        /// <param name="scoreArr">array of local scores</param>
        /// <returns></returns>
        public static int GlobalScore(Score[] scoreArr)
        {
            int score = 0;
            foreach (Score s in scoreArr)
            {
                //THIS IS COMPLETE BULLSHIT BUT THE SCORESTRUCT ISNT READY YET
                score += s.Value;
            }
            return score;
        }

        /// <summary>
        /// this method forces all groups to calculate their priorities
        /// </summary>
        private void CalcPriorities()
        {
            foreach (Group group in Groups)
            {
                group.CalcPriorities();
            }
        }

        /// <summary>
        /// This function goes over all the groups, reads their priorities and
        /// arranges them in a 2-dimentional array: Each group has it's own row
        /// which contains all its possible targets.
        /// Note: it is a jagged array and each group may have different number of 
        /// targets.
        /// We use array because it has quick access property which we will use heavily
        /// </summary>
        /// <returns>Matrix of all possible targets</returns>
        static private Score[][] GetPossibleTargetMatrix()
        {

            //allocate array of array: array for each group's possible targets
            Score[][] possibleTargets = new Score[Groups.Count][];

            for (int i = 0; i < Groups.Count; i++)
            {
                //convert the priority list to an array (to enable quick access)
                possibleTargets[i] = Groups[i].priorities.ToArray();
            }

            //return the matrix
            return possibleTargets;
        }


        /// <summary>
        /// Get the dimention vector which later will be used to create the iteration
        /// over all possible Group-Target assignments
        /// </summary>
        /// <returns>array of numbers of priorities for each group</returns>
        static private int[] GetTargetsdimensions()
        {
            //allocate a new array for the dimensions of each group's target
            int[] dimensions = new int[Groups.Count];


            //go over all the groups and read number of priorities to dimention
            for (int i = 0; i < Groups.Count; i++)
            {
                dimensions[i] = Groups[i].priorities.Count;
            }

            return dimensions;
        }
    }
}
