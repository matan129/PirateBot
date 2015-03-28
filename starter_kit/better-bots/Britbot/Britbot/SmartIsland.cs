﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pirates;

namespace Britbot
{
    public class SmartIsland : Island, ITarget
    {
        public SmartIsland(int Id, Location Loc, int Owner, int TeamCapturing, int TurnsBeingCaptured, int CaptureTurns,
            int Value)
            : base(Id, Loc, Owner, TeamCapturing, TurnsBeingCaptured, CaptureTurns, Value)
        {

        }

        public int ID
        {
            get
            {
                return base.Id;
            }

        }


        public int CaptureTurns { get; private set; }

        public int Value { get; private set; }
        public static List<SmartIsland> IslandList { get; private set; } //IMPLIMENT!


        /// <summary>
        /// Calculates the score of this island relative to a certain group by calculating how many potential points it will generate.
        /// </summary>
        /// <param name="origin"></param>
        /// <returns>Returns the Score for the Target</returns>
        public Score GetScore(Group origin)
        {
            //calculates the minimum distance between a group and said island
            int Distance = origin.Pirates.ConvertAll(p => Bot.Game.GetMyPirate(p)).Select(e => Bot.Game.Distance(e.Loc, this.Loc)).Concat(new int[] { }).Min();
            int CaptureTime = this.CaptureTurns; //Amount of turns it takes to capture an island
            int TheoreticalIslandCount = this.Value + Bot.Game.MyIslands().Count; //The amount of theoretical islands that we own
            int PointsPerTurn = (int)Math.Pow(2, TheoreticalIslandCount) - 1; //The amount of points we would earn per turn if we owned said island
            int MaxOwnershipTurns = 0; //The Amount of turns we will probably own said island
            int TurnNumber = 0; //The amount of future turns
            int ProjectedPoints = 0; //Approximation of number of points we will have in the future
            while (TurnNumber < MaxOwnershipTurns)
            {
                ProjectedPoints = PointsPerTurn * (TurnNumber - (Distance + CaptureTime));
            }
            Score myScore = new Score(origin, ProjectedPoints);
            return myScore;
            //MaxOwnerShipTurns will be calculated using enemy group's heading and proximity to island
        }
        
        /// <summary>
        /// Overrides "==" to include objects
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(SmartIsland a, SmartIsland b)
        {
            return a.ID == b.ID;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(SmartIsland a, SmartIsland b)
        {
            return a.ID != b.ID;
        }

        /// <summary>
        /// 
        /// </summary>
        public int NearbyEnemyCount()
        {
            int enemyCount = 0;
            int closestIslandDistance = 0;
            foreach(SmartIsland eisland in SmartIsland.IslandList)
            {
                int temp = Bot.Game.Distance(eisland.Loc,this.Loc);
                if ( temp < closestIslandDistance)
                {
                    closestIslandDistance = temp;
                }
            }
            int dangerRadius = closestIslandDistance / 2;
            foreach(EnemyGroup egroup in Enemy.Groups)
            {
                if ( egroup.MinimalDistanceTo(this.Loc) <=dangerRadius && egroup.GuessTarget() == this)
                {
                    enemyCount = enemyCount + egroup.EnemyPirates.Count;
                }
            }
            return enemyCount;
        }
    }
}

