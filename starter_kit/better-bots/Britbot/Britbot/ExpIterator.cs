using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Britbot
{
    /// <summary>
    /// this class will be used to go over every possible group-target assignment
    /// it is like counting in base ten with unclear amount of fingers
    /// </summary>
    class ExpIterator
    {
        //array of dimention, given at the constructor and never changes
        private int[] dimentions;
        //the values of the iteration vector
        public int[] values;

        /// <summary>
        /// assignes dimentions and initiate value of iteration
        /// </summary>
        /// <param name="dimentions">dimentions of the iteration</param>
        public ExpIterator(int[] dimentions)
        {
            //set the given dimentions
            this.dimentions = dimentions;

            //initiate count at zero
            this.values = new int[dimentions.Length];
            for (int i = 0; i < this.values.Length; i++)
            {
                values[i] = 0;
            }
        }

        /// <summary>
        /// checks if all the entries of the iteration are zero
        /// </summary>
        /// <returns>true if it is so, false otherwise</returns>
        public bool isZero()
        {
            //going over the list searching for nonzero
            foreach (int value in this.values)
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
        /// you can count up to the given dimention each turn
        /// </summary>
        /// <param name="eit"></param>
        /// <returns>false if the iteration is finished (gone back to zero), true otherwise</returns>
        public static bool operator ++(ExpIterator eit)
        {
            //going over the vector entries
            for (int i = 0; i < eit.values.Length; i++)
            {
                //if we don't need to go over the top of the dimention just add
                if (eit.values[i] < eit.dimentions[i] - 1)
                {
                    eit.values[i]++;
                    break;
                }
                else //carry the one
                {
                    eit.values[i] = 0;
                }
            }
            //check if we are back at zero
            return !eit.isZero();
        }
    }
}
