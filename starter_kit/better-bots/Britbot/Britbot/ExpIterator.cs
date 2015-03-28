﻿namespace Britbot
{
    /// <summary>
    /// this class will be used to go over every possible group-target assignment
    /// it is like counting in base ten with unclear amount of fingers
    /// </summary>
    class ExpIterator
    {
        //array of dimension, given at the constructor and never changes
        public int[] Dimensions { get; private set; }

        //the values of the iteration vector
        public int[] Values;

        /// <summary>
        /// assigns dimensions and initiate value of iteration
        /// </summary>
        /// <param name="dims">dimensions of the iteration</param>
        public ExpIterator(int[] dims)
        {
            //set the given dimensions
            this.Dimensions = dims;

            //initiate count at zero
            this.Values = new int[dims.Length];
            for (int i = 0; i < this.Values.Length; i++)
            {
                Values[i] = 0;
            }
        }

        /// <summary>
        /// checks if all the entries of the iteration are zero
        /// </summary>
        /// <returns>true if it is so, false otherwise</returns>
        public bool IsZero()
        {
            //going over the list searching for nonzero
            foreach (int value in this.Values)
            {
                if (value != 0)
                    return false;
            }

            //if here it means that all the entries are zero
            return true;
        }

        /// <summary>
        /// Main functionality of this class: it advances iteration
        /// it is just like long addition (bad translation?) only
        /// you can count up to the given dimension each turn
        /// </summary>
        /// <returns>false if the iteration is finished (gone back to zero), true otherwise</returns>
        public bool IterationFinished()
        {
            //MATAN K - you cannot overload unary operator (like ++) without returning this class's type
            //I mean, its signature should be public static ExpIterator operator ++(ExpIterator) { ... }

            //going over the vector entries
            for (int i = 0; i < this.Values.Length; i++)
            {
                //if we don't need to go over the top of the dimension just add
                if (this.Values[i] < this.Dimensions[i] - 1)
                {
                    this.Values[i]++;
                    break;
                }
                else //carry the one
                {
                    this.Values[i] = 0;
                }
            }

            //check if we are back at zero
            return !this.IsZero();
        }
    }
}
