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

            //going over all directions
            foreach (Direction dir in Bot.Game.GetDirections(myLoc, target))
            {
                //calculate new heading vector if we choose this direction
                HeadingVector newHeading = HeadingVector.adjustHeading(myHeading, dir);
                //calculate the dot product with the desired direction and normalize, if it is close to 1 it 
                //means that we are almost in the right direction
                double newFitCoef = newHeading.Normalize() * desiredVector.Normalize();
                
                //check if this direction is better (coefficient is larget) then the others
                if (newFitCoef > directionFitCoeff)
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
            //first check if no direction
            if (dir.NormSquared() == 0)
                return Bot.Game.Distance(point, linePoint);

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
            //if the target is stationary then return true
            if (targetHeading.Norm() == 0)
                return true;
            //first check if it is running away (its direction is about the same as the difference between you
            if (HeadingVector.CalcDifference(group, target) * targetHeading > 0)
                return false;


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



        //-------------------------------------------------------------------------------------------------------------------
        //--------------------------------------------SERIOUS SHIT OVER HERE-------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------

        //------------------------------------------ A* path finding algorithem ---------------------------------------------
        
        /// <summary>
        /// private class representing each cell of the map
        /// </summary>
        private class Node
        {
            /// <summary>
            /// The X coordinate of the Node
            /// </summary>
            public Location Loc;

            /// <summary>
            /// the weight of the cell, should be higher near enemies and unpassable places
            /// </summary>
            public double Weight;

            /// <summary>
            /// the calculated G function score of the algorithem for this specific node
            /// it will be updated during the algorithem
            /// </summary>
            public double G;

            /// <summary>
            /// The huristic coefficient, it will simply be the euclidian distance
            /// </summary>
            public double H;

            /// <summary>
            /// this weight represents an impassable cell
            /// </summary>
            public static double infinity = 1025583;

            //-------------------operators----------------------

            /// <summary>
            /// Equals operator, compares the locations
            /// </summary>
            /// <param name="obj">the object we are comparing to</param>
            /// <returns>true if the locations are the same, else otherwise</returns>
            public override bool Equals(object obj)
            {
                //check if it is even a Node
                if(obj.GetType() == this.GetType())
                {
                    return (this.Loc == ((Node) obj).Loc);
                }
                //otherwise
                return false;
            }
        }

        private Node[,] InitialNodes(int groupStrength, Location target)
        {
            //reading board size
            int cols = Bot.Game.GetCols();
            int rows = Bot.Game.GetRows();

            //get the radius of the group
            int groupRadius = Group.GetRingCount(groupStrength);

            //create array of nodes corresponding to the actual locations in the map
            //--------IMPORTANT: the access to the map is like with location- first Y than X !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Node[,] Map = new Node[rows, cols];

            //go over the entire map, set locations and their wight
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    //set the location inside the node
                    Map[y, x].Loc = new Location(y, x);

                    //set the huristic parameter to be the regular distance
                    Map[y, x].H = Bot.Game.Distance(Map[y, x].Loc, target);

                    //set the calculated G parameter to 0
                    Map[y, x].G = 0;

                    //check if this is passable
                    if (!Bot.Game.IsPassableEnough(Map[y, x].Loc, groupRadius))
                    {
                        //set the weight of the node to "infinity"
                        Map[y, x].Weight = Node.infinity;

                        //if so then we are finished here, move to next Node
                        continue;
                    }

                    //now set the wight based on enemyGroups
                    double EnemyFactor = 1;


                }
            }


            return Map;
        }

        private static double CalcEnemyFactor(Location loc, int GroupStrength)
        {
            //constant defining The addvantage factor we have because of our great structure
            const int addvantageFactor = 1;

            //constants representing the danger distibution across the map (good for enemies we can kill, bad otherwise)
            const double badDangerSpreadCoeff = 0.5;
            const double goodDangerSpreadCoeff = 0.1;

            //read attack radious
            double attackRadius = Bot.Game.GetAttackRadius();

            //constant representing enemyships negative radius on the map
            double dangerZone = 9 * attackRadius;

            //We would like to avoid enemy groups begger then our selfs and move towords smaller one
            //But there might be some smaller ones near one another and we won't know that till we 
            //go over all off them so we keep them in a seperate tab of smaller group and if everything
            //goes well we subrtact them

            //initialize count
            double eBadFactor = 1;
            double eGoodFactor = 1;
            double eGoodTurnsToBadFactor = 0;

            //enemy count
            int enemyCount = 0;

            //go over all the enemy groups 
            foreach (EnemyGroup eGroup in Enemy.Groups)
            {
                //get distance of eGroup to the tested location
                double distanceSquared = eGroup.MinimalSquaredDistanceTo(loc);
                
                //read enemy strength
                int enemyStrength = eGroup.EnemyPirates.Count;

                //check if this group is a threat
                if(distanceSquared <= dangerZone)
                {
                    enemyCount += enemyStrength;
                }
                else //otherwise we can skip it
                {
                    continue;
                }

                //we remove one Attack Radious as precaution
                distanceSquared = Math.Max(0 , distanceSquared - Bot.Game.GetAttackRadius());

                //if they are stronger than us
                if (enemyStrength - addvantageFactor > GroupStrength)
                {
                    //add to bad factor

                    //first we normalize the distance
                    distanceSquared = distanceSquared / (Node.infinity * (dangerZone - Bot.Game.GetAttackRadius()));

                    //we add in an "inverse to the distant" way and proportional to the enemy group strength
                    //we also add coefficient so at the edge of the dangerzone it will be badDangerSpreadCoeff * infinity
                    eBadFactor += badDangerSpreadCoeff * enemyStrength / distanceSquared;
                }
                else
                {
                    //we add to both the good and the good gone bad just in case

                    //first we normalize the distance
                    distanceSquared = distanceSquared / (Node.infinity * (dangerZone - Bot.Game.GetAttackRadius()));

                    //we add in an "inverse to the distant" way and proportional to the enemy group strength
                    //we also add coefficient so at the edge of the dangerzone it will be badDangerSpreadCoeff * infinity
                    eGoodFactor += goodDangerSpreadCoeff * enemyStrength / distanceSquared;
                    eGoodTurnsToBadFactor += badDangerSpreadCoeff * enemyStrength / distanceSquared;
                }

            }

            //check if we are good or naughty 
            if (enemyCount - addvantageFactor > GroupStrength)
            {
                //good
                return eGoodFactor;
            }
            else
            {
                return eBadFactor + eGoodTurnsToBadFactor;
            }
        }
    }
}