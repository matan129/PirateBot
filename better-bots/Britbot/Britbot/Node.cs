#region Usings

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
        #region Fields & Properies

        /// <summary>
        /// private class representing each cell of the map
        /// </summary>
        internal class Node : PriorityQueueNode
        {
            #region Static Fields & Consts

            /// <summary>
            /// this weight represents an impassable cell
            /// </summary>
            public const double Infinity = 1025583;

            /// <summary>
            /// A static array of nodes representing the map
            /// ------------IMPORTANT: like with locations the access to this Map is Map[y,x]-------
            /// </summary>
            public static Node[,] Map = new Node[Bot.Game.GetRows(), Bot.Game.GetCols()];

            /// <summary>
            /// This determines if we are using the Euclidian huristic or the Manhaten one
            /// </summary>
            public static bool EuclidianHuristic = false;


            #endregion

            #region Fields & Properies

            /// <summary>
            /// the calculated G function score of the algorithm for this specific node
            /// it will be updated during the algorithm
            /// </summary>
            public double G;

            /// <summary>
            /// The heuristic coefficient, it will simply be the euclidean distance
            /// </summary>
            public double H;

            /// <summary>
            /// true if G value has been calculated
            /// </summary>
            public bool IsEvaluated;

            /// <summary>
            /// True if we already went over this node
            /// </summary>
            public bool IsTraveled;

            /// <summary>
            /// The X coordinate of the Node
            /// </summary>
            public Location Loc;

            /// <summary>
            /// the weight of the cell, should be higher near enemies and un-passable places
            /// </summary>
            public double Weight;

            #endregion

            #region Constructors & Initializers

            /// <summary>
            /// One time initializing of the Node array
            /// </summary>
            static Node()
            {
                for (int y = 0; y < Node.Map.GetLength(0); y++)
                {
                    for (int x = 0; x < Node.Map.GetLength(1); x++)
                    {
                        Node.Map[y, x] = new Node();

                        //set locations
                        Node.Map[y, x].Loc = new Location(y, x);
                    }
                }
            }

            #endregion

            /// <summary>
            /// This function should be called ONCE PER GROUP in the CalculatePriorities function
            /// it updates the map data based on the current game state
            /// it sets what areas are passable and what are dangerous
            /// </summary>
            /// <param name="groupStrength">amount of pirates in the group</param>
            public static void UpdateMap(int groupStrength)
            {
                //reading board size
                int cols = Bot.Game.GetCols();
                int rows = Bot.Game.GetRows();

                //get the radius of the group
                int groupRadius = Group.GetRingCount(groupStrength);

                //go over the entire map, set locations and their wight
                for (int x = 0; x < cols; x++)
                {
                    for (int y = 0; y < rows; y++)
                    {
                        //check if this is passable
                        //if (!Bot.Game.IsPassableEnough(Node.Map[y, x].Loc, groupRadius))
                        if(!Bot.Game.IsPassableEnough(Node.Map[y, x].Loc,groupRadius))
                        {
                            //set the weight of the node to "infinity"
                            Node.Map[y, x].Weight = Node.Infinity;

                            //if so then we are finished here, move to next Node
                            continue;
                        }

                        Node.Map[y, x].Weight = Node.CalcEnemyFactor(Node.Map[y, x].Loc, groupStrength);
                       // Node.Map[y, x].Weight = 1;
                        //now set the wight based on enemyGroups
                        //double enemyFactor = Node.CalcEnemyFactor(Node.Map[y, x].Loc, groupStrength);
                    }
                }
            }

            /// <summary>
            /// This function should be called PER TARGET in the Navigator.CalculatePath method
            /// it updates the heuristic values based on distance from the target
            /// also sets G value to default -1
            /// </summary>
            /// <param name="target"></param>
            public static void SetUpCalculation(Location target)
            {
                //going over all the cells in the Map updating their heuristic value to be 
                //distance from target
                for (int y = 0; y < Bot.Game.GetRows(); y++)
                {
                    for (int x = 0; x < Bot.Game.GetCols(); x++)
                    {
                        Node.Map[y, x].H = Node.HuristicFunction(Node.Map[y, x].Loc, target);

                        //set the calculated G parameter to -1
                        Node.Map[y, x].G = -1;

                        //set IsTraveled status and IsEvaluated
                        Node.Map[y, x].IsTraveled = false;

                        Node.Map[y, x].IsEvaluated = false;
                    }
                }
            }

            /// <summary>
            /// this determines the huristic function used for the A* algorithm based on the
            /// EuclidianHuristic constant in Node
            /// </summary>
            /// <param name="loc1">first location</param>
            /// <param name="loc2">second location</param>
            /// <returns>huristic cost of going between them</returns>
            public static double HuristicFunction(Location loc1, Location loc2)
            {
                //if we chose euclidian huristic
                if (Node.EuclidianHuristic)
                    return Bot.Game.EuclidianDistanceSquared(loc1, loc2);
                // Manhatten huristic
                else
                    return Bot.Game.Distance(loc1, loc2);
            }

            /// <summary>
            /// helper function to calculate how dangerous is curtain location for a specific group
            /// it calculation is based on distances from strong enemy groups and lots of stupid constants 
            /// </summary>
            /// <param name="loc">the location we are testing</param>
            /// <param name="GroupStrength">the group strength (because danger is relative)</param>
            /// <returns>the weight of the location</returns>
            private static double CalcEnemyFactor(Location loc, int GroupStrength)
            {
                TheD.BeginTime("CalcEnemyFactor");
                //constant defining The addvantage factor we have because of our great structure
                int addvantageFactor = 0;
                //if the structure gives us addvantage 
                if (GroupStrength > 2)
                    addvantageFactor = 1;
                
                //read attack radious
                double attackRadius = Bot.Game.GetAttackRadius();

                //constant representing enemyships negative radius on the map
                double dangerZone = 4 * attackRadius;

                //We would like to avoid enemy groups bigger then our selves and move towords smaller one
                //But there might be some smaller ones near one another and we won't know that till we 
                //go over all off them so we keep them in a separate tab of smaller group and if everything
                //goes well we subtract them

                
                //go over all the enemy groups 
                foreach (EnemyGroup eGroup in Enemy.Groups)
                {
                    //get distance of eGroup to the tested location
                    double distanceSquared = eGroup.MinimalSquaredDistanceTo(loc);

                    //check if this group isn't a threat
                    if (distanceSquared > dangerZone)
                    {
                        continue;
                    }

                    //read enemy strength
                    int enemyStrength = eGroup.EnemyPirates.Count;                    

                    //we remove one Attack Radious as precaution
                    //distanceSquared = Math.Max(0, distanceSquared - Bot.Game.GetAttackRadius());
                    
                    //if they are stronger than us
                    if (enemyStrength - addvantageFactor >= GroupStrength)
                    {
                        //add to bad factor

                        //we add in an "inverse to the distant" way and proportional to the enemy group strength
                        //we also add coefficient so at the edge of the dangerzone it will be badDangerSpreadCoeff * infinity
                        //eBadFactor = badDangerSpreadCoeff * enemyStrength / distanceSquared;
                        return Node.Infinity;
                    }
                }
                TheD.StopTime("CalcEnemyFactor");
                return 1;
            }

            /// <summary>
            /// Returns the Node in the map corresponding to the locations specified
            /// </summary>
            /// <param name="loc">the location we want</param>
            /// <returns>The node in the map corresponding to the location</returns>
            public static Node GetLocationNodeFromMap(Location loc)
            {
                return Node.Map[loc.Row, loc.Col];
            }

            /// <summary>
            /// calculates the F function (what we are minimizing)
            /// </summary>
            /// <returns>F value</returns>
            public double F()
            {
                return  (this.H + this.G);
            }

            /// <summary>
            /// checks if a curtain node is passible, meaning its wight isn't infinity
            /// </summary>
            /// <returns>True if it has a finite weight, else otherwise</returns>
            public bool IsPassable()
            {
                return this.Weight < Node.Infinity;
            }


            public static void DebugPasses()
            {
                string line = "";
                for (int i = 0; i < Node.Map.GetLength(0); i++)
                {
                    for (int j = 0; j < Node.Map.GetLength(1); j++)
                    {
                        if (Node.Map[i, j].Weight >= 3)
                            line += "x";
                        else
                            line += "-";
                    }
                    Bot.Game.Debug(line);
                    line = "";
                }
            }
            /// <summary>
            /// returns the neighbors of the Node, meaning the adjacent location wich are not
            /// impassible
            /// </summary>
            /// <returns></returns>
            public List<Node> GetNeighbors()
            {
                //allocating a new list
                List<Node> neighbors = new List<Node>();

                //array to help iterating over the four neighbors
                int[] coeff = {-1, 1};

                //read X and Y from this.loc
                int X = this.Loc.Col;
                int Y = this.Loc.Row;


                //going over neighbors
                for (int delata = 0; delata < 2; delata++)
                {
                    //get the iterated neighbor coordinates
                    int neighborX = X + coeff[delata];
                    int neighborY = Y ;

                    //check if out of bounderie, if so, continue to next iteration
                    if ((neighborX < 0) || (neighborX >= Bot.Game.GetCols()) ||
                        (neighborY < 0) || (neighborY >= Bot.Game.GetRows()))
                        continue;

                    //check if impassable, if so, continue to next iteration
                    /*if (!Node.Map[neighborY, neighborX].IsPassable())
                        continue;*/

                    //if we are here it means that the neighbor is ok, so add him to the list
                    neighbors.Add(Node.Map[neighborY, neighborX]);
                }
                for (int delata = 0; delata < 2; delata++)
                {
                    //get the iterated neighbor coordinates
                    int neighborX = X ;
                    int neighborY = Y + coeff[delata];

                    //check if out of bounderie, if so, continue to next iteration
                    if ((neighborX < 0) || (neighborX >= Bot.Game.GetCols()) ||
                        (neighborY < 0) || (neighborY >= Bot.Game.GetRows()))
                        continue;

                    //check if impassable, if so, continue to next iteration
                   /* if (!Node.Map[neighborY, neighborX].IsPassable())
                        continue;*/

                    //if we are here it means that the neighbor is ok, so add him to the list
                    neighbors.Add(Node.Map[neighborY, neighborX]);
                }

                return neighbors;
            }

            //-------------------operators----------------------

            /// <summary>
            /// Equals operator, compares the locations
            /// </summary>
            /// <param name="obj">the object we are comparing to</param>
            /// <returns>true if the locations are the same, else otherwise</returns>
           /* public override bool Equals(object obj)
            {
                //check if it is even a Node
                if (obj.GetType() == this.GetType())
                {
                    return (this.Loc == ((Node) obj).Loc);
                }
                //otherwise
                return false;
            }*/
            /*
            /// <summary>
            /// compares two nodes
            /// first checks if null, else compares locations
            /// </summary>
            /// <param name="n1">first node</param>
            /// <param name="n2">second node</param>
            /// <returns></returns>
            public static override bool operator ==(Node n1, Node n2)
            {
                //check nulls
                if (object.ReferenceEquals(n1, null))
                {
                    return object.ReferenceEquals(n2, null);
                }

                return n1.Equals(n2);
            }*/
        }

        #endregion
    }
}