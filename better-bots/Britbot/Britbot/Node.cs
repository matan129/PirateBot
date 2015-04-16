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
            /// //---------------#Magic_Numbers--------------------
            /// </summary>
            public const double Infinity = 100;

            /// <summary>
            /// A static array of nodes representing the map
            /// ------------IMPORTANT: like with locations the access to this Map is Map[y,x]-------
            /// </summary>
            public static Node[,] Map = new Node[Bot.Game.GetRows(), Bot.Game.GetCols()];

            /// <summary>
            /// This determines if we are using the Euclidian huristic or the Manhaten one
            /// </summary>
            public static bool EuclidianHuristic = true;


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
                int groupRadius = Group.GetRingCount(groupStrength) + 1;

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
                            Node.Map[y, x].Weight = Node.Infinity ;

                            //if so then we are finished here, move to next Node
                            continue;
                        }

                       // Node.Map[y, x].Weight = Node.CalcEnemyFactor(Node.Map[y, x].Loc, groupStrength);
                        Node.Map[y, x].Weight = 1;
                        //now set the wight based on enemyGroups
                        //double enemyFactor = Node.CalcEnemyFactor(Node.Map[y, x].Loc, groupStrength);
                    }
                }
                //update enemy threat
                Node.CalculateEnemyWeight(groupStrength);
            }

            /// <summary>
            /// This adds the wight of the enemies wich pose a threat to 
            /// the specified group size
            /// </summary>
            /// <param name="strength">strength of the group</param>
            public static void CalculateEnemyWeight(int strength)
            {
                //---------------#Magic_Numbers--------------------
                //Important constants of the functions
                //the radious under wich to define locations in danget of enemy groups
                //the higher it is, the performence are worse
                int DangerRadious = 4 * Bot.Game.GetAttackRadius();

                //going over enemy fleets and giving their location negative scores
                foreach (EnemyGroup eGroup in Enemy.Groups)
                {
                    if (eGroup.GetMaxFightPower() >= strength)
                    {
                        Node.BlockLocation(eGroup.GetLocation(), DangerRadious, eGroup.GetHeading());
                    }
                }
            }

            /// <summary>
            /// Creates a gradiante of bad score around a specific location with a given radious
            /// </summary>
            /// <param name="loc">the location of the danger</param>
            /// <param name="radSquared">the square of the danger radious</param>
            /// <param name="heading">the heading of the danger, to make the danger zone elipse like</param>
            private static void BlockLocation(Location loc, int radSquared, HeadingVector heading)
            {
                //---------------#Magic_Numbers--------------------
                //important constants:
                //this determines how much we consider the heading of the danger, meaning how elipsy the 
                //danger zone will be
                const double headingFactor = 0.25;

                //reading some important parameters
                int rad = (int) Math.Sqrt(radSquared);
                int maxX = Bot.Game.GetCols() - 1;
                int maxY = Bot.Game.GetRows() - 1;

                //going over a square centered at loc
                for (int x = Math.Max(loc.Col - rad,0); x <= Math.Min(loc.Col + rad,maxX); x++)
                {
                    for (int y = Math.Max(loc.Row - rad, 0); y <= Math.Min(loc.Row + rad, maxY); y++)
                    {
                        //the current location
                        Location currLoc = new Location(y,x);

                        //blocking stuff according to their radious and direction
                        HeadingVector diffVector = HeadingVector.CalcDifference(loc, currLoc);

                        //calculate the distance considering the heading of the danger
                        //if the current location is straight in the direction of the danger we add by proportion to headingFactor
                        //if it is in the opposite direction we subtract according to headingFactor
                        double distandeSquare = Bot.Game.EuclidianDistanceSquared(new Location(y, x), loc) + headingFactor * heading.Normalize() * diffVector;
                        if(distandeSquare <= radSquared)
                            Node.Map[y, x].Weight = Node.Infinity * (radSquared - distandeSquare) / radSquared;
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
                TheD.BeginTime("SetUpCalculation");
                //going over all the cells in the Map updating their heuristic value to be 
                //distance from target
                for (int y = 0; y < Bot.Game.GetRows(); y++)
                {
                    for (int x = 0; x < Bot.Game.GetCols(); x++)
                    {
                        Node.Map[y, x].H = Node.HuristicFunction(Node.Map[y, x].Loc, target);

                        //set the calculated G parameter to -1
                        Node.Map[y, x].G = -1;
                        
                        Node.Map[y, x].IsEvaluated = false;
                    }
                }
                TheD.StopTime("SetUpCalculation");
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
                        if (Node.Map[i, j].H == 0)
                            line += "O";
                        else if (Node.Map[i, j].G == -1)
                            line += "S";
                        else if (Node.Map[i, j].Weight >= 3)
                            line += "X";
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