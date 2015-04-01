using System;
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
        /// Conversion constructor: takes a direction of the game and turns it into 
        /// a vector
        /// </summary>
        /// <param name="d">The game direction class(namely south,east,west...)</param>
        private HeadingVector(Direction d = Direction.NOTHING)
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
        /// Simple assignment constructor
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y Value</param>
        public HeadingVector(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Simple Conversion from Location class
        /// </summary>
        /// <param name="loc">location</param>
        public HeadingVector(Location loc)
        {
            this.X = loc.Col;
            this.Y = loc.Row;
        }

        /// <summary>
        /// Positive X value means left and vice-versa
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Positive Y value means down and vice-versa
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// regular set function for convenience
        /// sets both X and Y simultaneously
        /// </summary>
        /// <param name="x">X value, default is 0</param>
        /// <param name="y">Y value, default is 0</param>
        public void SetCoordinates(int x = 0, int y = 0)
        {
            this.X = x;
            this.Y = y;
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
            return hv1.X*hv2.X + hv1.Y*hv2.Y;
        }

        /// <summary>
        /// this operator updates the direction based on the new one
        /// if the new direction is not to far from the one given (up to 90 degrees)
        /// it simply adds to the previous count vector
        /// otherwise, we assume there was a change in course and so we start again
        /// </summary>
        /// <param name="hv1">the main direction, the one we comparing the other to</param>
        /// <param name="d">the new direction</param>
        /// <returns>itself if we need to use many operation in the same line</returns>
        public static HeadingVector operator +(HeadingVector hv1, Direction d)
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
            if (hv1*hv2 < 0)
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
        /// regular "algebraic" sum of two vectors
        /// </summary>
        /// <param name="hv1">first vector</param>
        /// <param name="hv2">second vector</param>
        /// <returns>the sum of the two vectors</returns>
        public static HeadingVector operator +(HeadingVector hv1, HeadingVector hv2)
        {
            //defining the result
            HeadingVector newHv = hv1;

            //adding up
            newHv.X += hv2.X;
            newHv.Y += hv2.Y;

            return newHv;
        }

        /// <summary>
        /// regular "algebraic" difference between two vectors
        /// </summary>
        /// <param name="hv1">first vector</param>
        /// <param name="hv2">second vector</param>
        /// <returns>the difference between the two vectors</returns>
        public static HeadingVector operator -(HeadingVector hv1, HeadingVector hv2)
        {
            //defining the result
            HeadingVector newHv = hv1;

            //Substraction
            newHv.X -= hv2.X;
            newHv.Y -= hv2.Y;

            return newHv;
        }

        /// <summary>
        /// Calculates the direction vector between two points
        /// </summary>
        /// <param name="source">the point of beginning</param>
        /// <param name="target">the target (end point)</param>
        /// <returns></returns>
        public static HeadingVector CalcDifference(Location source, Location target)
        {
            //assigning new variable
            HeadingVector newHv = new HeadingVector(target);

            //subtraction
            newHv.X -= source.Col;
            newHv.Y -= source.Row;

            return newHv;
        }

        /// <summary>
        /// Calculates a new vector perpendicular to the given one
        /// it simply rotates 90 degrees anti clockwise
        /// </summary>
        /// <returns>A vector perpendicular to the given vector</returns>
        public HeadingVector Orthogonal()
        {
            //like multiplying by -1
            return new HeadingVector {X = this.Y, Y = -this.X};
        }

        //------------NORMS------------

        /// <summary>
        /// Returns the square of the vector's length
        /// might be used to normalize vectors for some computation
        /// </summary>
        /// <returns>the length of the vector squared</returns>
        public int NormSquared()
        {
            return X*X + Y*Y;
        }

        /// <summary>
        /// returns the length of the vector (as a double)
        /// might be used to normalize vectors for some computation
        /// </summary>
        /// <returns>the length of the vector</returns>
        public double Norm()
        {
            return Math.Sqrt(NormSquared());
        }

        /// <summary>
        /// returns the sum of X and Y meaning the duration of time
        /// of the calculation. this can be used as a creditability measure
        /// </summary>
        /// <returns>duration of time since last nullifying data</returns>
        public int Norm1()
        {
            return Math.Abs(X) + Math.Abs(Y);
        }

        /// <summary>
        /// To string function...
        /// </summary>
        /// <returns>a string :)</returns>
        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }

        /// <summary>
        /// Enumerates the location determined by the direction if this heading vector and the pivot supplied
        /// </summary>
        /// <param name="originPivot">The pivot to refer to</param>
        /// <returns>a collection of the relevant locations</returns>
        public IEnumerable<Location> EnumerateLocations(Location originPivot)
        {
            //A function that determines if a location is out of the map's boundaries
            Func<Location, bool> isOutOfBoundaries = delegate { return false; };

            //A function that gets the next location
            Func<Location, Location> getNextLocation =
                location => new Location(location.Row + this.X, location.Col + this.Y);

            //Emit new location while the location is not our of the map
            while (!isOutOfBoundaries.Invoke(getNextLocation.Invoke(originPivot)))
            {
                originPivot = getNextLocation.Invoke(originPivot);

                yield return originPivot;
            }
        }

        /// <summary>
        /// Given your location, you current direction and the target's location, this method
        /// calculates the best direction for you to move in order to simulate a straight line of
        /// motion to to your target
        /// </summary>
        /// <param name="myLoc">Location of the one calling for direction</param>
        /// <param name="myHeading">current direction</param>
        /// <param name="target">Targets location</param>
        /// <returns></returns>
        public static Direction CalculateDirectionToStationeryTarget(Location myLoc, HeadingVector myHeading,
            Location target)
        {
            //get the desired direction
            HeadingVector desiredVector = CalcDifference(myLoc, target);

            //variable for the best direction so far
            Direction bestDirection = Direction.NOTHING;
            double directionFitCoeff = -1;

            //going over all directions
            foreach (Direction dir in Bot.Game.GetDirections(myLoc, target))
            {
                //calculate new heading vector if we choose this direction
                HeadingVector newHeading = myHeading + dir;
                //calculate the dot product with the desired direction and normalize, if it is close to 1 it 
                //means that we are almost in the right direction
                double newFitCoef = 1 - (newHeading*desiredVector)/(newHeading.Norm()*desiredVector.Norm());

                //check if this direction is better (coefficient is smaller) then the others
                if (newFitCoef < directionFitCoeff)
                {
                    //replace best
                    bestDirection = dir;
                    directionFitCoeff = newFitCoef;
                }
            }

            //return best direction found
            return bestDirection;
        }

        /// <summary>
        /// This function calculates the directions to a moving target
        /// it does so by solving the intersection point equation as appears in 
        /// calculation sheet 1 and then using the above CalculateDirectionToStationeryTarget
        /// i am very likely to be wrong here
        /// </summary>
        /// <param name="myLoc">location of the one asking for directions</param>
        /// <param name="myHeading">direction vector of the one looking for direction</param>
        /// <param name="target">the location of the moving target</param>
        /// <param name="targetHeading">direction vector of the moving target</param>
        /// <returns></returns>
        public static Direction CalculateDirectionToMovingTarget(Location myLoc, HeadingVector myHeading,
            Location target, HeadingVector targetHeading)
        {
            //defining parameters for calculation (see image 1 under calculations)
            int a = targetHeading.Norm1();
            int b = target.Col - myLoc.Col;
            int c = targetHeading.X;
            int d = target.Row - myLoc.Row;
            int e = targetHeading.Y;

            //calculating r, hopefully
            double r = SolveStupidEquation(a, b, c, d, e);

            //finally, calculating the intersection point
            Location intersection = new Location(target.Row + (int) (r*targetHeading.Y),
                target.Col + (int) (r*targetHeading.X));

            //returning path to intersection
            return CalculateDirectionToStationeryTarget(myLoc, myHeading, intersection);
        }

        /// <summary>
        /// This function should solve equation (*) in calculation sheet 2
        /// I am not sure if this works, the solution was annoying as hell, hope i
        /// did everything right
        /// </summary>
        /// <param name="a">|d|_{1}</param>
        /// <param name="b">t_{x} - u_{x}</param>
        /// <param name="c">d_{x}</param>
        /// <param name="d">t_{y} - u_{y}</param>
        /// <param name="e">d_{y}</param>
        /// <returns></returns>
        public static double SolveStupidEquation(double a, double b, double c, double d, double e)
        {
            //TODO fix this
            int[] signs = {-1, 1};
            //there are 4 options, going over them 2 by 2
            for (int i = 0; i <= 1; i++) //i is the sign of c in r
            {
                int cSign = signs[i];
                for (int j = 0; j <= 1; j++) //j is the sign of e in r
                {
                    int eSign = signs[j];
                    double r = -(cSign*b + eSign*d)/(a + cSign*c + eSign*e);

                    //check if r isn't positive, if so we can skip it
                    if (r <= 0)
                        continue;

                    //else check the critirions
                    if ((cSign*c*r <= -cSign*b) && (eSign*e*r <= -eSign*d))
                    {
                        //it is (probably?) the correct r
                        return r;
                    }
                }
            }

            //if we are here then i am wrong :)
            throw new Exception("Matan K is an idiot");
        }

        /// <summary>
        /// calculates distance (in turns) from ship's trajectory to a given island location (point)
        /// uses the calculation in calculation sheet 3
        /// </summary>
        /// <param name="point">the point outside the trajectory (island)</param>
        /// <param name="linePoint">the point on the trajectory (ship)</param>
        /// <param name="dir">direction of the line (direction of the ship)</param>
        /// <returns></returns>
        public static double CalcDistFromLine(Location point, Location linePoint, HeadingVector dir)
        {
            //TODO fix this - Matan Kom
            //Find the difference vector between the point and the line point
            HeadingVector dif = CalcDifference(point, linePoint);

            //find the minimum t parameter
            double tMin = dir*dif/dir.Norm();

            //calculating actual distance (see calculation)
            return (Math.Abs(dif.X - tMin*dir.X) + Math.Abs(dif.Y - tMin*dir.Y));
        }

        /// <summary>
        /// Given two pirates (id) it tells you who is "more" in a specific direction than the other
        /// </summary>
        /// <param name="p1">first pirate</param>
        /// <param name="p2">second pirate</param>
        /// <returns></returns>
        public int ComparePirateByDirection(int p1, int p2)
        {
            //calculate both pirates position on the line created by hv
            double p1Dist = CalcDistFromLine(new Location(0, 0), Bot.Game.GetMyPirate(p1).Loc, this.Orthogonal());
            double p2Dist = CalcDistFromLine(new Location(0, 0), Bot.Game.GetMyPirate(p2).Loc, this.Orthogonal());

            return (int) (p2Dist - p1Dist);
        }

        /// <summary>
        /// This method calculates if you would be able to reach an enemy group or it would run away
        /// 
        /// </summary>
        /// <param name="group">your location</param>
        /// <param name="target">Enemy group location</param>
        /// <param name="targetHeading">Enemy direction</param>
        /// <returns>true if it is possible to reach, else otherwise</returns>
        public static bool IsReachable(Location group, Location target, HeadingVector targetHeading)
        {
            //----------------------calculation of naive maximum intersection----------------------
            HeadingVector diffVector = CalcDifference(target, group);

            double cosAlpha = diffVector*targetHeading/(diffVector.Norm()*targetHeading.Norm());

            Location maxIntersection =
                new Location(target.Row + (int) (targetHeading.Y*diffVector.Norm()*cosAlpha/targetHeading.Norm()),
                    target.Col + (int) (targetHeading.X*diffVector.Norm()*cosAlpha/targetHeading.Norm()));
            //----------------------------------------------------------------------------------------

            //to account for numeric mistakes we take antitolerance coefficient
            const int antiToleranceCoeff = 2;

            //compare distances
            //check who is closer
            if (Bot.Game.Distance(group, maxIntersection) <
                Bot.Game.Distance(target, maxIntersection) - antiToleranceCoeff)
                return true;

            return false;
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
            return Equals((HeadingVector) obj);
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
                return (X*397) ^ Y;
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
            return !(hv1 == hv2);
        }

        #endregion
    }
}