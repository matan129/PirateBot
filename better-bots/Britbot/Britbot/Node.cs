#region Usings

using System;
using Pirates;

#endregion

namespace Britbot
{
    partial class Navigator
    {
        //------------------------------------------ A* path finding algorithm ---------------------------------------------

        /// <summary>
        /// private class representing each cell of the map
        /// </summary>
        private class Node
        {
            #region Static Fields & Consts

            /// <summary>
            /// this weight represents an impassable cell
            /// </summary>
            public const double Infinity = 1025583;

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
            /// The X coordinate of the Node
            /// </summary>
            public Location Loc;

            /// <summary>
            /// the weight of the cell, should be higher near enemies and not passable places
            /// </summary>
            public double Weight;

            #endregion

            #region Constructors & Initializers

            private static Node[,] InitialNodes(int groupStrength, Location target)
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
                            Map[y, x].Weight = Node.Infinity;

                            //if so then we are finished here, move to next Node
                            continue;
                        }

                        //now set the wight based on enemyGroups
                        double EnemyFactor = 1;
                    }
                }


                return Map;
            }

            #endregion

            //-------------------operators----------------------

            /// <summary>
            /// Equals operator, compares the locations
            /// </summary>
            /// <param name="obj">the object we are comparing to</param>
            /// <returns>true if the locations are the same, else otherwise</returns>
            public override bool Equals(object obj)
            {
                //check if it is even a Node
                if (obj.GetType() == this.GetType())
                {
                    return (this.Loc == ((Node) obj).Loc);
                }

                //otherwise
                return false;
            }

            private static double CalcEnemyFactor(Location loc, int groupStrength)
            {
                //constant defining The addvantage factor we have because of our great structure
                const int addvantageFactor = 1;

                //constants representing the danger distribution across the map (good for enemies we can kill, bad otherwise)
                const double badDangerSpreadCoeff = 0.5;
                const double goodDangerSpreadCoeff = 0.1;

                //read attack radious
                double attackRadius = Bot.Game.GetAttackRadius();

                //constant representing enemyships negative radius on the map
                double dangerZone = 9 * attackRadius;

                //We would like to avoid enemy groups bigger then our selves and move towords smaller one
                //But there might be some smaller ones near one another and we won't know that till we 
                //go over all off them so we keep them in a separate tab of smaller group and if everything
                //goes well we subtract them

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
                    if (distanceSquared <= dangerZone)
                    {
                        enemyCount += enemyStrength;
                    }
                    else //otherwise we can skip it
                    {
                        continue;
                    }

                    //we remove one Attack Radious as precaution
                    distanceSquared = Math.Max(0, distanceSquared - Bot.Game.GetAttackRadius());

                    //if they are stronger than us
                    if (enemyStrength - addvantageFactor > groupStrength)
                    {
                        //add to bad factor

                        //first we normalize the distance
                        distanceSquared = distanceSquared / (Node.Infinity * (dangerZone - Bot.Game.GetAttackRadius()));

                        //we add in an "inverse to the distant" way and proportional to the enemy group strength
                        //we also add coefficient so at the edge of the dangerzone it will be badDangerSpreadCoeff * infinity
                        eBadFactor += badDangerSpreadCoeff * enemyStrength / distanceSquared;
                    }
                    else
                    {
                        //we add to both the good and the good gone bad just in case

                        //first we normalize the distance
                        distanceSquared = distanceSquared / (Node.Infinity * (dangerZone - Bot.Game.GetAttackRadius()));

                        //we add in an "inverse to the distant" way and proportional to the enemy group strength
                        //we also add coefficient so at the edge of the dangerzone it will be badDangerSpreadCoeff * infinity
                        eGoodFactor += goodDangerSpreadCoeff * enemyStrength / distanceSquared;
                        eGoodTurnsToBadFactor += badDangerSpreadCoeff * enemyStrength / distanceSquared;
                    }
                }

                //check if we are good or naughty 
                if (enemyCount - addvantageFactor > groupStrength)
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
}