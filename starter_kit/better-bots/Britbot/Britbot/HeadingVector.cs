using System;
using System.Collections;
using System.Collections.Generic;
using Pirates;

namespace Britbot
{
    /// <summary>
    /// This class accumulates direction over large number of turns
    /// we assume (0,0) is the left top corner of the screen and that
    /// </summary>
    public class HeadingVector
    {
        /// <summary>
        /// Positive X value means left and vice-versa
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Positive Y value means down and vice-versa
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Conversion constructor: takes a direction of the game and turns it into 
        /// a vector
        /// </summary>
        /// <param name="d">The game direction class(namely south,east,west...)</param>
        HeadingVector(Direction d = Direction.NOTHING)
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
                    X = 1;
                    Y = 0;
                    break;
                default:
                    X = 0;
                    Y = 0;
                    break;
            }
        }
     
        /// <summary>
        /// calculates the dot product for two vectors
        /// used to check if angles are sharp or doll (if they are the correct terms)
        /// </summary>
        /// <param name="hv1">first vector</param>
        /// <param name="hv2">second vector</param>
        /// <returns>the scalar product</returns>
        public static int operator *(HeadingVector hv1, HeadingVector hv2)
        {
            return hv1.X * hv2.X + hv1.Y * hv2.Y;
        }
        
        /// <summary>
        /// this operator updates the direction based on the new one
        /// if the new direction is not to far from the one given (up to 90 degrees)
        /// it simply adds to the previous count vector
        /// otherwise, we assume there was a change in course and so we start again
        /// </summary>
        /// <param name="hv1">the main direction, the one we comparing the other to</param>
        /// <param name="hv2">the new direction</param>
        /// <returns>itself if we need to use many operation in the same line</returns>
        public static HeadingVector operator +(HeadingVector hv1,Direction d)
        {
            //defining the result
            HeadingVector newHv = hv1;

            //cast d into vector
            HeadingVector hv2 = new HeadingVector(d);

            //if dot product is negative it means that the angle is bigger
            // then 90 so it implies change in direction: reset count
            //otherwise normal vector addition
            //WOW math is useful
            //I thought you HATED the applied math department
            if(hv1 * hv2 < 0)
            {
                newHv = hv2;
            }
            else
            {
                newHv.X += hv2.X;
                newHv.Y += hv2.Y;
            }
            //return self for a+b+c calculations
            return newHv;
        }
        
        /// <summary>
        /// Calculates a new vector perpendicular to the given one
        /// it simply rotates 90 degrees anti clockwise
        /// </summary>
        /// <returns>A vector perpendicular to the given vector</returns>
        public HeadingVector Orthogonal()
        {
            HeadingVector newHv = new HeadingVector();
            
            //like multiplying by i
            newHv.X = Y;
            newHv.Y = -X;
            return newHv;
        }

        /// <summary>
        /// Enumerates the location determined by the direction if this heading vector and the pivot supplied
        /// </summary>
        /// <param name="originPivot">The pivot to refer to</param>
        /// <returns>a collection of the relevant locations</returns>
        public IEnumerable<Location> EnumerateLocations(Location originPivot)
        {
            //A function that determines if a location is out of the map's boundaries
            Func<Location, bool> isOutOfBoundaries = delegate(Location location)
            {
                return false;
            };

            //A function that gets the next location
            Func<Location, Location> getNextLocation = delegate(Location location)
            {
                return new Location(location.Row + this.X, location.Col + this.Y);
            };

            //Emit new location while the location is not our of the map
            while (!isOutOfBoundaries.Invoke(getNextLocation.Invoke(originPivot)))
            {
                originPivot = getNextLocation.Invoke(originPivot);
                
                yield return originPivot;
            }
        }

        #region operators
        /// <summary>
        /// Check if the two objects are the same
        /// </summary>
        /// <param name="other">Other HeadingVector to compare to</param>
        /// <returns>true if equal and false otherwise</returns>
        protected bool Equals(HeadingVector other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <summary>
        /// Check if the two objects are the same
        /// </summary>
        /// <param name="obj">Other object to compare to</param>
        /// <returns>true if equal and false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HeadingVector)obj);
        }

        /// <summary>
        /// Get a hash code for this instance. Should be use by system sorting and what not.
        /// ReSharper generated code
        /// </summary>
        /// <returns>the hash code for this instance of HeadingVector</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        /// <summary>
        /// checks if two vectors are the same: compares both entries
        /// </summary>
        /// <param name="hv1">first vector</param>
        /// <param name="hv2">second vector</param>
        /// <returns>true if they are the same, else otherwise</returns>
        public static bool operator ==(HeadingVector hv1, HeadingVector hv2)
        {
            return hv1.X == hv2.X && hv1.Y == hv2.Y;
        }

        /// <summary>
        /// checks if two vectors are not the same: compares both entries
        /// </summary>
        /// <param name="hv1">first vector</param>
        /// <param name="hv2">second vector</param>
        /// <returns>true if they are not the same, else otherwise</returns>
        public static bool operator !=(HeadingVector hv1, HeadingVector hv2)
        {
            return hv1.X != hv2.X && hv1.Y != hv2.Y;
        }
        #endregion
    }
}
