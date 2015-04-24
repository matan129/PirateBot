#region #Usings

using System;
using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     This class accumulates direction over large number of turns
    ///     we assume (0,0) is the left top corner of the screen and that
    /// </summary>
    public class HeadingVector
    {
        #region Fields & Properies

        /// <summary>
        ///     Positive X value means left and vice-versa
        /// </summary>
        public double X { get; private set; }

        /// <summary>
        ///     Positive Y value means down and vice-versa
        /// </summary>
        public double Y { get; private set; }

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     Conversion constructor: takes a direction of the game and turns it into
        ///     a vector
        /// </summary>
        /// <param name="d">The game direction class(namely south,east,west...)</param>
        public HeadingVector(Direction d = Direction.NOTHING)
        {
            switch (d)
            {
                case Direction.NORTH:
                    X = 0;
                    Y = -1;
                    break;
                case Direction.EAST:
                    X = 1;
                    Y = 0;
                    break;
                case Direction.SOUTH:
                    X = 0;
                    Y = 1;
                    break;
                case Direction.WEST:
                    X = -1;
                    Y = 0;
                    break;
                default:
                    X = 0;
                    Y = 0;
                    break;
            }
        }

        /// <summary>
        ///     Copy constructor
        /// </summary>
        /// <param name="toCopy"></param>
        public HeadingVector(HeadingVector toCopy)
        {
            this.X = toCopy.X;
            this.Y = toCopy.Y;
        }

        /// <summary>
        ///     Simple assignment constructor
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y Value</param>
        public HeadingVector(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        #endregion

        /// <summary>
        ///     Gets a unique-ish hascide for the instance
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.X.GetHashCode() * 397) ^ this.Y.GetHashCode();
            }
        }

        /// <summary>
        ///     regular set function for convenience
        ///     sets both X and Y simultaneously
        /// </summary>
        /// <param name="x">X value, default is 0</param>
        /// <param name="y">Y value, default is 0</param>
        public void SetCoordinates(double x = 0, double y = 0)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        ///     Copies the parameters of a given heading vector
        /// </summary>
        /// <param name="hv">the heading vector to copy from</param>
        public void SetCoordinates(HeadingVector hv)
        {
            this.X = hv.X;
            this.Y = hv.Y;
        }

        /// <summary>
        ///     calculates the dot product for two vectors
        ///     used to check if angles are sharp or doll (if they are the correct terms)
        /// </summary>
        /// <param name="hv1">first vector</param>
        /// <param name="hv2">second vector</param>
        /// <returns>the scalar product</returns>
        public static double operator *(HeadingVector hv1, HeadingVector hv2)
        {
            return hv1.X * hv2.X + hv1.Y * hv2.Y;
        }

        /// <summary>
        ///     this static method updates the direction based on the new one
        ///     if the new direction is not to far from the one given (up to 90 degrees)
        ///     it simply adds to the previous count vector
        ///     otherwise, we assume there was a change in course and so we start again
        /// </summary>
        /// <param name="hv1">the main direction, the one we comparing the other to</param>
        /// <param name="hv2">the new heading</param>
        /// <returns>itself if we need to use many operation in the same line</returns>
        public static HeadingVector adjustHeading(HeadingVector hv1, HeadingVector hv2)
        {
            //defining the result
            HeadingVector newHv = new HeadingVector(hv1);


            //if dot product is negative it means that the angle is bigger
            // then 90 so it implies change in direction: reset count
            //otherwise normal vector addition
            //WOW math is useful
            //I thought you HATED the applied math department
            if (hv1 * hv2 < 0)
            {
                newHv = hv2;
            } //also check that new direction isn't nothing
            else if (hv2.Norm() == 0)
            {
                newHv = hv2;
            }
            else
            {
                newHv.X += hv2.X;
                newHv.Y += hv2.Y;
            }
            //return the new heading vector
            return newHv;
        }

        /// <summary>
        ///     same as the previous one just gets a direction insted of a heading vector
        /// </summary>
        /// <param name="hv1">the main direction, the one we comparing the other to</param>
        /// <param name="dir">the new direction</param>
        /// <returns></returns>
        public static HeadingVector adjustHeading(HeadingVector hv1, Direction dir)
        {
            return HeadingVector.adjustHeading(hv1, new HeadingVector(dir));
        }

        /// <summary>
        ///     same as the previous function just used as a method
        /// </summary>
        /// <param name="Dir">new Direction</param>
        /// <returns></returns>
        public HeadingVector adjustHeading(HeadingVector Dir)
        {
            //just use the existing function
            this.SetCoordinates(HeadingVector.adjustHeading(this, Dir));

            //return this for a+b+c calculations
            return this;
        }

        /// <summary>
        ///     same as the previous function just gets a direction
        /// </summary>
        /// <param name="Dir">new Direction</param>
        /// <returns></returns>
        public HeadingVector adjustHeading(Direction Dir)
        {
            //just use the existing function
            this.SetCoordinates(HeadingVector.adjustHeading(this, new HeadingVector(Dir)));

            //return this for a+b+c calculations
            return this;
        }

        /// <summary>
        ///     Calculates the direction vector between two points
        /// </summary>
        /// <param name="source">the point of beginning</param>
        /// <param name="target">the target (end point)</param>
        /// <returns></returns>
        public static HeadingVector CalcDifference(Location source, Location target)
        {
            //assigning new variable
            return new HeadingVector(target.Col - source.Col, target.Row - source.Row);
        }

        /// <summary>
        ///     given your location and your heading this function calculates your new position after
        ///     going with the vector. consider the bounderies of the game
        /// </summary>
        /// <param name="loc">your location</param>
        /// <param name="hv">your heading</param>
        /// <returns>new location after moving vy the heading vector</returns>
        public static Location AddvanceByVector(Location loc, HeadingVector hv)
        {
            //calculate new col
            int col = loc.Col + (int) hv.X;
            int row = loc.Row + (int) hv.Y;

            //check if out of boundries
            //first check negative valuse
            col = Math.Max(col, 0);
            row = Math.Max(row, 0);

            //check to big boundries
            col = Math.Min(col, Bot.Game.GetCols() - 1);
            row = Math.Min(row, Bot.Game.GetRows() - 1);

            //return new location
            return new Location(row, col);
        }

        /// <summary>
        ///     Calculates a new vector perpendicular to the given one
        ///     it simply rotates 90 degrees anti clockwise
        /// </summary>
        /// <returns>A vector perpendicular to the given vector</returns>
        public HeadingVector Orthogonal()
        {
            //like multiplying by -1
            return new HeadingVector {X = this.Y, Y = -this.X};
        }

        //------------NORMS------------

        /// <summary>
        ///     Returns the square of the vector's length
        ///     might be used to normalize vectors for some computation
        /// </summary>
        /// <returns>the length of the vector squared</returns>
        public double NormSquared()
        {
            return X * X + Y * Y;
        }

        /// <summary>
        ///     returns the length of the vector (as a double)
        ///     might be used to normalize vectors for some computation
        /// </summary>
        /// <returns>the length of the vector</returns>
        public double Norm()
        {
            return Math.Sqrt(NormSquared());
        }

        /// <summary>
        ///     returns a new normalized vector
        /// </summary>
        /// <returns>new normalized vector</returns>
        public HeadingVector Normalize()
        {
            return (1 / this.Norm()) * this;
        }

        /// <summary>
        ///     returns the sum of X and Y meaning the duration of time
        ///     of the calculation. this can be used as a creditability measure
        /// </summary>
        /// <returns>duration of time since last nullifying data</returns>
        public double Norm1()
        {
            return Math.Abs(X) + Math.Abs(Y);
        }

        /// <summary>
        ///     To string function...
        /// </summary>
        /// <returns>a string :)</returns>
        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }

        /// <summary>
        ///     Enumerates the location determined by the direction if this heading vector and the pivot supplied
        /// </summary>
        /// <param name="originPivot">The pivot to refer to</param>
        /// <returns>a collection of the relevant locations</returns>
        /// <summary>
        ///     Check if the two objects are the same
        /// </summary>
        /// <param name="other">Other HeadingVector to compare to</param>
        /// <returns>true if equal and false otherwise</returns>
        protected bool Equals(HeadingVector other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <summary>
        ///     Check if the two objects are the same
        /// </summary>
        /// <param name="obj">Other object to compare to</param>
        /// <returns>true if equal and false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj))
                return false;
            if (object.ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((HeadingVector) obj);
        }

        /// <summary>
        ///     Get a hash code for this instance. Should be use by system sorting and what not.
        ///     ReSharper generated code
        /// </summary>
        /// <returns>the hash code for this instance of HeadingVector</returns>
        /// <summary>
        ///     checks if two vectors are the same: compares both entries
        /// </summary>
        /// <param name="hv1">first vector</param>
        /// <param name="hv2">second vector</param>
        /// <returns>true if they are the same, else otherwise</returns>
        public static bool operator ==(HeadingVector hv1, HeadingVector hv2)
        {
            return hv1.X == hv2.X && hv1.Y == hv2.Y;
        }

        /// <summary>
        ///     checks if two vectors are not the same: compares both entries
        /// </summary>
        /// <param name="hv1">first vector</param>
        /// <param name="hv2">second vector</param>
        /// <returns>true if they are not the same, else otherwise</returns>
        public static bool operator !=(HeadingVector hv1, HeadingVector hv2)
        {
            return !(hv1 == hv2);
        }

        /// <summary>
        ///     regular "algebraic" sum of two vectors
        /// </summary>
        /// <param name="hv1">first vector</param>
        /// <param name="hv2">second vector</param>
        /// <returns>the sum of the two vectors</returns>
        public static HeadingVector operator +(HeadingVector hv1, HeadingVector hv2)
        {
            //defining the result
            HeadingVector newHv = new HeadingVector(hv1);

            //adding up
            newHv.X += hv2.X;
            newHv.Y += hv2.Y;

            return newHv;
        }

        /// <summary>
        ///     regular "algebraic" difference between two vectors
        /// </summary>
        /// <param name="hv1">first vector</param>
        /// <param name="hv2">second vector</param>
        /// <returns>the difference between the two vectors</returns>
        public static HeadingVector operator -(HeadingVector hv1, HeadingVector hv2)
        {
            //defining the result
            HeadingVector newHv = new HeadingVector(hv1);

            //Substraction
            newHv.X -= hv2.X;
            newHv.Y -= hv2.Y;

            return newHv;
        }

        /// <summary>
        ///     just scalar multipication
        /// </summary>
        /// <param name="scalar">a scalar</param>
        /// <param name="hv">a vector</param>
        /// <returns></returns>
        public static HeadingVector operator *(double scalar, HeadingVector hv)
        {
            //defining the result
            HeadingVector newHv = new HeadingVector(scalar * hv.X, scalar * hv.Y);

            return newHv;
        }
    }
}