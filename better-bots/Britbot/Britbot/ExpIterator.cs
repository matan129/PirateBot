#region #Usings

using System;
using System.Linq;

#endregion

namespace Britbot
{
    /// <summary>
    ///     this class will be used to go over every possible group-target assignment
    ///     it is like counting in base ten with unclear amount of fingers
    ///     demo: https://www.youtube.com/watch?v=UIKGV2cTgqA
    /// </summary>
    internal class ExpIterator
    {
        #region Fields & Properies

        /// <summary>
        ///     array of dimension, given at the constructor and never changes
        /// </summary>
        public int[] Dimensions { get; private set; }

        /// <summary>
        ///     The values of the iteration vector
        /// </summary>
        public int[] Values { get; set; }

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     Assigns dimensions and initiate value of iteration
        /// </summary>
        /// <param name="dims">dimensions of the iteration</param>
        /// <exception cref="Exception">Dimensions must be strictly positive</exception>
        public ExpIterator(int[] dims)
        {
            //set the given dimensions

            this.Dimensions = dims;

            //check if dimensions are legal (meaning strictly positive)
            if (Dimensions.Any(dim => dim <= 0))
            {
                throw new InvalidIteratorDimensionException("Dimensions must be strictly positive");
            }

            //initiate count at zero
            this.Values = new int[dims.Length];
            for (int i = 0; i < this.Values.Length; i++)
            {
                Values[i] = 0;
            }
        }

        #endregion

        /// <summary>
        ///     checks if all the entries of the iteration are zero
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
        ///     Main functionality of this class: it advances iteration
        ///     it is just like long addition (bad translation?) only
        ///     you can count up to the given dimension each turn
        /// </summary>
        /// <returns>false if the iteration is finished (gone back to zero), true otherwise</returns>
        public bool NextIteration()
        {
            //going over the vector entries
            for (int i = 0; i < this.Values.Length; i++)
            {
                //if we don't need to go over the top of the dimension just add

                if (this.Values[i] < this.Dimensions[i] - 1)
                {
                    this.Values[i]++;
                    break;
                }
                this.Values[i] = 0;
            }

            //check if we are back at zero
            return !this.IsZero();
        }

        /// <summary>
        ///     Provides a textual description for this ExpIterator
        /// </summary>
        /// <returns>A textual description for this ExpIterator</returns>
        public override string ToString()
        {
            return "Dimensions: " + Dimensions + "\n" + "MultiIndex: " + Values;
        }
    }
}