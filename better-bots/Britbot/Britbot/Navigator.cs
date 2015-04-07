#region Usings

using System;
using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     this class will deal with geographical calculations and direction calculation
    /// </summary>
    internal static class Navigator
    {
        /// <summary>
        ///     Given your location, you current direction and the target's location, this method
        ///     calculates the best direction for you to move in order to simulate a straight line of
        ///     motion to to your target
        /// </summary>
        /// <param name="myLoc">Location of the one calling for direction</param>
        /// <param name="myHeading">current direction</param>
        /// <param name="target">Targets location</param>
        /// <returns>optimal direction</returns>
        public static Direction CalculateDirectionToStationeryTarget(Location myLoc, HeadingVector myHeading,
            Location target)
        {
            //get the desired direction
            HeadingVector desiredVector = HeadingVector.CalcDifference(myLoc, target);

            //variable for the best direction so far
            Direction bestDirection = Direction.NOTHING;
            double directionFitCoeff = -1;
            Bot.Game.Debug("---------------NAVIGATOR------------------");
            //going over all directions
            foreach (Direction dir in Bot.Game.GetDirections(myLoc, target))
            {
                //calculate new heading vector if we choose this direction
                HeadingVector newHeading = HeadingVector.adjustHeading(myHeading, dir);
                //calculate the dot product with the desired direction and normalize, if it is close to 1 it 
                //means that we are almost in the right direction
                double newFitCoef = newHeading.Normalize() * desiredVector.Normalize();

                
                Bot.Game.Debug("desired " + desiredVector.ToString());
                Bot.Game.Debug("newHeading " + newHeading.ToString());
                Bot.Game.Debug("newFitCoef " + newFitCoef);
                Bot.Game.Debug("dir " + dir.ToString());

                //check if this direction is better (coefficient is larget) then the others
                if (newFitCoef > directionFitCoeff)
                {
                    //replace best
                    bestDirection = dir;
                    directionFitCoeff = newFitCoef;
                }
            }
            Bot.Game.Debug("------------------------------------------");
            //return best direction found
            return bestDirection;
        }

        /// <summary>
        ///     This function calculates the directions to a moving target
        ///     it does so by solving the intersection point equation as appears in
        ///     calculation sheet 1 and then using the above CalculateDirectionToStationeryTarget
        ///     i am very likely to be wrong here
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
            double a = targetHeading.Norm1();
            double b = target.Col - myLoc.Col;
            double c = targetHeading.X;
            double d = target.Row - myLoc.Row;
            double e = targetHeading.Y;

            //calculating r, hopefully
            double r = SolveStupidEquation(a, b, c, d, e);

            //finally, calculating the intersection point
            Location intersection = HeadingVector.AddvanceByVector(target, r * targetHeading);

            //returning path to intersection
            return CalculateDirectionToStationeryTarget(myLoc, myHeading, intersection);
        }

        /// <summary>
        ///     This function should solve equation (*) in calculation sheet 2
        ///     I am not sure if this works, the solution was annoying as hell, hope i
        ///     did everything right
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
                    double r = -(cSign * b + eSign * d) / (a + cSign * c + eSign * e);

                    //check if r isn't positive, if so we can skip it
                    if (r <= 0)
                        continue;

                    //else check the critirions
                    if ((cSign * c * r <= -cSign * b) && (eSign * e * r <= -eSign * d))
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
        ///     calculates distance (in turns) from ship's trajectory to a given island location (point)
        ///     uses the calculation in calculation sheet 3
        /// </summary>
        /// <param name="point">the point outside the trajectory (island)</param>
        /// <param name="linePoint">the point on the trajectory (ship)</param>
        /// <param name="dir">direction of the line (direction of the ship)</param>
        /// <returns></returns>
        public static double CalcDistFromLine(Location point, Location linePoint, HeadingVector dir)
        {
            //TODO fix this - Matan Kom
            //Find the difference vector between the point and the line point
            HeadingVector dif = HeadingVector.CalcDifference(point, linePoint);

            //find the minimum t parameter
            double tMin = dir.Normalize() * dif;

            //calculating actual distance (see calculation)
            return (dif - tMin * dir).Norm1();
        }

        /// <summary>
        ///     Given two pirates (id) it tells you who is "more" in a specific direction than the other
        /// </summary>
        /// <param name="p1">first pirate</param>
        /// <param name="p2">second pirate</param>
        /// <param name="hv">the direction we compare in</param>
        /// <returns></returns>
        public static int ComparePirateByDirection(int p1, int p2, HeadingVector hv)
        {
            //calculate both pirates position on the line created by hv
            double p1Dist = CalcDistFromLine(new Location(0, 0), Bot.Game.GetMyPirate(p1).Loc, hv.Orthogonal());
            double p2Dist = CalcDistFromLine(new Location(0, 0), Bot.Game.GetMyPirate(p2).Loc, hv.Orthogonal());

            return (int) (p2Dist - p1Dist);
        }

        /// <summary>
        ///     This method calculates if you would be able to reach an enemy group or it would run away
        /// </summary>
        /// <param name="group">your location</param>
        /// <param name="target">Enemy group location</param>
        /// <param name="targetHeading">Enemy direction</param>
        /// <returns>true if it is possible to reach, else otherwise</returns>
        public static bool IsReachable(Location group, Location target, HeadingVector targetHeading)
        {
            //----------------------calculation of naive maximum intersection----------------------
            HeadingVector diffVector = HeadingVector.CalcDifference(target, group);

            double cosAlpha = diffVector.Normalize() * targetHeading.Normalize();
            double sacleCoeff = diffVector.Norm() * cosAlpha;

            Location maxIntersection = HeadingVector.AddvanceByVector(target, sacleCoeff * targetHeading.Normalize());
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
    }
}