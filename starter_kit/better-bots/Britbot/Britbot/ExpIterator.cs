namespace Britbot
{
    /// <summary>
    /// this class will be used to go over every possible group-target assignment
    /// it is like counting in base ten with unclear amount of fingers
    /// </summary>
    class ExpIterator
    {
        //array of dimension, given at the constructor and never changes
        private int[] dimentions;
        //the values of the iteration vector
        public int[] values;

        /// <summary>
        /// assigns dimensions and initiate value of iteration
        /// </summary>
        /// <param name="dimensions">dimensions of the iteration</param>
        public ExpIterator(int[] dimensions)
        {
            //set the given dimensions
            this.dimentions = dimensions;

            //initiate count at zero
            this.values = new int[dimensions.Length];
            for (int i = 0; i < this.values.Length; i++)
            {
                values[i] = 0;
            }
        }

        /// <summary>
        /// checks if all the entries of the iteration are zero
        /// </summary>
        /// <returns>true if it is so, false otherwise</returns>
        public bool IsZero()
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
        /// you can count up to the given dimension each turn
        /// </summary>
        /// <returns>false if the iteration is finished (gone back to zero), true otherwise</returns>
        public bool IterationFinished()
        {
            //MATAN K - you cannot overload unary operator (like ++) without returning this class's type
            //I mean, its signature should be public static ExpIterator operator ++(ExpIterator) { ... }

            //going over the vector entries
            for (int i = 0; i < this.values.Length; i++)
            {
                //if we don't need to go over the top of the dimension just add
                if (this.values[i] < this.dimentions[i] - 1)
                {
                    this.values[i]++;
                    break;
                }
                else //carry the one
                {
                    this.values[i] = 0;
                }
            }

            //check if we are back at zero
            return !this.IsZero();
        }
    }
}
