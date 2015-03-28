using System;
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

        public static List<SmartIsland> IslandList { get; private set; }                 
        
        public Location IslandLocation { get; private set; }

        /// <summary>
        /// Calculates the score of this island relative to a certain group by calculating how many potential points it will generate.
        /// </summary>
        /// <param name="origin">The original group of pirates of the type "Group"</param>
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

            if (ProjectedPoints < 0) //As there is no such thing as negative points, Show 0.
                ProjectedPoints = 0;

            Score myScore = new Score(origin, ProjectedPoints);
            return myScore;
            //MaxOwnerShipTurns will be calculated using enemy group's heading and proximity to island
        }
        
        /// <summary>
        /// Checks if 2 smart Islands are equal
        /// </summary>
        /// <param name="a">Island 1</param>
        /// <param name="b">Island 2</param>
        /// <returns>True or False</returns>
        public static bool operator ==(SmartIsland a, SmartIsland b)
        {
            return a.ID == b.ID;
        }
        /// <summary>
        /// Checks if 2 smart islands are different
        /// </summary>
        /// <param name="a">Island 1</param>
        /// <param name="b">Island 2</param>
        /// <returns>True or false</returns>
        public static bool operator !=(SmartIsland a, SmartIsland b)
        {
            return a.ID != b.ID;
        }

        /// <summary>
        /// Checks if there are enemies near said Island that will probably attack it
        /// </summary>
        /// <returns>The amount of enemies near the target</returns>
        public int NearbyEnemyCount()
        {
            int enemyCount = 0; //amount of enemy pirates in proximity to the Island
            int closestIslandDistance = 0; //The distance between this Island and the one nearest too it
            foreach(SmartIsland eisland in SmartIsland.IslandList) //Calculates the distance between this island and the one nearest
            {
                int temp = Bot.Game.Distance(eisland.Loc,this.Loc);
                if ( temp < closestIslandDistance)
                {
                    closestIslandDistance = temp;
                }
            }
            int dangerRadius = closestIslandDistance / 2; // All enemy pirates are heading towards a certain island and it is safe to assume that they are heading towards the one nearest to them
            foreach(EnemyGroup egroup in Enemy.Groups)
            {
                if ( egroup.MinimalDistanceTo(this.Loc) <=dangerRadius && egroup.GuessTarget() == this) //Checks if the group of islands is near the island and if they are getting closer or farther
                {
                    enemyCount = enemyCount + egroup.EnemyPirates.Count; //Calculates the sum of pirates in proximity to the island
                }
            }
            return enemyCount;
        }
    }
}

