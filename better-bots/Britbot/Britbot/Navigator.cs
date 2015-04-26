#define MAP_DEBUG
#undef MAP_DEBUG

#region #Usings

using System;
using System.Collections.Generic;
using Britbot.PriorityQueue;
using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     this class will deal with geographical calculations and direction calculation
    /// </summary>
    internal static partial class Navigator
    {
        #region Static Fields & Consts

        public static List<long> time = new List<long>();

        #endregion

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
/*

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
                
                //check if this direction is better (coefficient is larger) then the others
                if (newFitCoef > directionFitCoeff)
                {
                    //replace best
                    bestDirection = dir;
                    directionFitCoeff = newFitCoef;
                }
            }

            //return best direction found
            return bestDirection;*/
            Logger.BeginTime("CalculateDirectionToStationeryTarget");
            Direction d;
            //first check if we are already at the target
            if (myLoc.Equals(target))
                d = Direction.NOTHING;
            else
            //otherwise use the A* thing

            d = Navigator.CalculatePath(myLoc, target);

            Logger.StopTime("CalculateDirectionToStationeryTarget");
            return d;
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
            double r = Navigator.SolveStupidEquation(a, b, c, d, e);

            //finally, calculating the intersection point
            Location intersection = HeadingVector.AddvanceByVector(target, r * targetHeading);

            //returning path to intersection
            return Navigator.CalculateDirectionToStationeryTarget(myLoc, myHeading, intersection);
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
            double tMin = (-1 / dir.NormSquared()) * dir * dif;

            //calculating actual distance (see calculation)
            return (dif + tMin * dir).Norm();
            ;
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
            HeadingVector originDif = new HeadingVector(Bot.Game.GetMyPirate(p1).Loc.Col,
                Bot.Game.GetMyPirate(p1).Loc.Row);
            int coef;

            if (originDif * hv > 0)
                coef = 1;
            else
                coef = -1;
            //calculate both pirates position on the line created by hv
            double p1Dist = Navigator.CalcDistFromLine(new Location(0, 0), (Bot.Game.GetMyPirate(p1)).Loc,
                hv.Orthogonal());
            double p2Dist = Navigator.CalcDistFromLine(new Location(0, 0), (Bot.Game.GetMyPirate(p2)).Loc,
                hv.Orthogonal());

            return coef * p1Dist.CompareTo(p2Dist);
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

        /// <summary>
        ///     Calculates first direction in path according to the A* algorithem
        ///     explanation + the pseudo code used to write this can be found in
        ///     http://en.wikipedia.org/wiki/A*_search_algorithm
        /// </summary>
        /// <param name="start">the location you are in (meaning the group)</param>
        /// <param name="target">the desired location (meaning the target)</param>
        /// <returns>direction you should go to reach your target</returns>
        public static Direction CalculatePath(Location start, Location target)
        {
            Logger.BeginTime("CalculatePath");
            //first set up the for a new target calculation
            Node.SetUpCalculation(target);
            //Priority queue of the currently checked nodes. Thank You BlueRaja
            HeapPriorityQueue<Node> openset = new HeapPriorityQueue<Node>(Bot.Game.GetCols() * Bot.Game.GetRows());

            //set the beginning
            Node beginning = Node.GetLocationNodeFromMap(start);
            openset.Enqueue(beginning, beginning.F());

            beginning.Distance = 0;

            //begin the iteration
            while (openset.Count > 0)
            {
                //get the current Node from the top of the openset priority queue
                Node currentNode = openset.Dequeue();

                //check if it is out target, if so we are done
                if (currentNode.Loc.Equals(target))
                    break;

                //set current node status
                currentNode.IsEvaluated = true;

                //going over the Neighbors of the current cell
                foreach (Node neighbor in currentNode.GetNeighbors())
                {
                    //if we already calculated this neighbor, skip to the next
                    if (neighbor.IsEvaluated)
                        continue;

                    //calculate the new G score from this rout
                    double tentativeG = currentNode.G + neighbor.Weight;
                    // double tentativeG = currentNode.G + 1;

                    //if the neighbor isn't in the open set
                    //or we just found a better score for him (tentativeG < G)
                    //or if G value is default -1
                    //then add him to openset
                    if ((!openset.Contains(neighbor)) || (neighbor.G == -1) || (tentativeG < neighbor.G))
                    {
                        //update G score
                        neighbor.G = tentativeG;

                        neighbor.Distance = currentNode.Distance + 1;

                        ////if the neighbor isn't in the open set, add him
                        if (!openset.Contains(neighbor))
                        {
                            openset.Enqueue(neighbor, neighbor.F());
                        }
                        else //update
                        {
                            openset.UpdatePriority(neighbor, neighbor.F());
                        }
                    }
                }
                //update current node
                currentNode.IsEvaluated = true;
            }
            Logger.StopTime("CalculatePath");

#if MAP_DEBUG
            Node.DebugPasses();
#endif
            //now we have made the necessary calculations, just get the desired direction
            return Navigator.FindBestDirectionOutOfMap(beginning);
        }

       
        public static int CalculateDistance(Location start, Location target)
        {
            Logger.BeginTime("CalculatePath");
            //first set up the for a new target calculation
            Node.SetUpCalculation(target);
            //Priority queue of the currently checked nodes. Thank You BlueRaja
            HeapPriorityQueue<Node> openset = new HeapPriorityQueue<Node>(Bot.Game.GetCols() * Bot.Game.GetRows());

            //set the beginning
            Node beginning = Node.GetLocationNodeFromMap(start);
            openset.Enqueue(beginning, beginning.F());

            beginning.Distance = 0;

            //begin the iteration
            while (openset.Count > 0)
            {
                //get the current Node from the top of the openset priority queue
                Node currentNode = openset.Dequeue();

                //check if it is out target, if so we are done
                if (currentNode.Loc.Equals(target))
                    break;

                //set current node status
                currentNode.IsEvaluated = true;

                //going over the Neighbors of the current cell
                foreach (Node neighbor in currentNode.GetNeighbors())
                {
                    //if we already calculated this neighbor, skip to the next
                    if (neighbor.IsEvaluated)
                        continue;

                    //calculate the new G score from this rout
                    double tentativeG = currentNode.G + neighbor.Weight;
                    // double tentativeG = currentNode.G + 1;

                    //if the neighbor isn't in the open set
                    //or we just found a better score for him (tentativeG < G)
                    //or if G value is default -1
                    //then add him to openset
                    if ((!openset.Contains(neighbor)) || (neighbor.G == -1) || (tentativeG < neighbor.G))
                    {
                        //update G score
                        neighbor.G = tentativeG;

                        neighbor.Distance = currentNode.Distance + 1;

                        ////if the neighbor isn't in the open set, add him
                        if (!openset.Contains(neighbor))
                        {
                            openset.Enqueue(neighbor, neighbor.F());
                        }
                        else //update
                        {
                            openset.UpdatePriority(neighbor, neighbor.F());
                        }
                    }
                }
                //update current node
                currentNode.IsEvaluated = true;
            }
            Logger.StopTime("CalculatePath");

#if MAP_DEBUG
            Node.DebugPasses();
#endif
            //now we have made the necessary calculations, just get the desired direction
            return Node.GetLocationNodeFromMap(target).Distance;
        }
        
        
        /// <summary>
        ///     after the A* algorithm has finished, is simply finds the best neighbor of the
        ///     beginning node and returns the direction to it
        /// </summary>
        /// <param name="beginning">location of the one calling for directions</param>
        /// <returns></returns>
        private static Direction FindBestDirectionOutOfMap(Node beginning)
        {
            Node bestNextNode = null;

            //go over the neighbors of the begining
            foreach (Node neighbor in beginning.GetNeighbors())
            {
                //if bestNode is null update and skip to next iteration
                if (bestNextNode == null)
                {
                    bestNextNode = neighbor;
                    continue;
                }
                //if this neighbors score is better then update
                if (neighbor.F() < bestNextNode.F())
                {
                    bestNextNode = neighbor;
                }
            }
            //check if best node is null, if so then i am an idiot and YOU NEED TO INFORM ME OF THAT IMMIDIATELY
            if (bestNextNode == null)
            {
                Logger.Write("--------------------------------------------------------------------------");
                Logger.Write("--------------------------------------------------------------------------");
                Logger.Write("--------------------------------------------------------------------------");
                Logger.Write("            Matan K is stupid as shit, please go and tell him that");
                Logger.Write("--------------------------------------------------------------------------");
                Logger.Write("--------------------------------------------------------------------------");
                Logger.Write("--------------------------------------------------------------------------");
                return Direction.NOTHING;
                //throw new Exception("Matan K is stupid as shit, please go and tell him that");
            }
            return Bot.Game.GetDirections(beginning.Loc, bestNextNode.Loc)[0];
        }

        /// <summary>
        ///     function to update map for specific group
        ///     simply calls the Node.updateMap function
        /// </summary>
        /// <param name="groupStrength">Group whose path is being calculated</param>
        public static void UpdateMap(Group group)
        {
            Node.UpdateMap(group);
        }
    }
}