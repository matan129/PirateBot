#region #Usings

using System;

#endregion

namespace Britbot
{
    /// <summary>
    ///     This class holds the magic numbers we have and may be used for smart in game updates
    /// </summary>
    internal static class Magic
    {
        #region Static Fields & Consts

        public const double VectorTolerance = 0.001;
        internal const int OutOfDateNumber = 10;
        public const double EnemyBaseFactor = 0;
        public const double FriendlyBaseFactor = 5;
        public const double densityBonusCoefficient = 0;

        /// <summary>
        ///     Max safe iterator iterations per turn
        /// </summary>
        public static int MaxIterator = 1500;

        /// <summary>
        ///     Max distace two groups can be from eachother and still be joind
        /// </summary>
        public static int MaxDistance = 30;

        /// <summary>
        ///     Length of global score's simulation
        /// </summary>
        public static readonly int SimulationLength = 80;

        /// <summary>
        ///     The range a cloaked pirate has to be from a taget to be revealed
        /// </summary>
        public static int CloakRange = 1;

        /// <summary>
        ///     Max groups to allow in the config
        /// </summary>
        public static int MaxGroups = 5;

        public static double ScoreConssitencyFactor = 0.0;

        public static int DecisivenessBonus = 10000;
        public static int LowerIteratorBound = 1000;

        #endregion

        #region Fields & Properies

        /// <summary>
        ///     The radious of the area around enemy ships which we avoid
        /// </summary>
        public static double DangerZone
        {
            get { return 9 * Bot.Game.GetAttackRadius(); }
        }

        /// <summary>
        ///     The radious of the maximum difference from enemy ships trajectory to an island so we will consider it to be going
        ///     to the island
        /// </summary>
        public static double EnemyPredictionSensitivity
        {
            get { return 2.5 * Math.Sqrt(Bot.Game.GetAttackRadius()); }
        }

        /// <summary>
        ///     The distance where a group shpuld make a maneuver (get off the island and attack the enemy)
        /// </summary>
        public static double ManeuverDistance
        {
            get { return Math.Sqrt(1.5 * Bot.Game.GetAttackRadius()); }
        }

        /// <summary>
        ///     the maximum distance of enemy from island that we can say with any certainty that he can possibly try to capture
        ///     This actually means the maximum time period we try to predict
        /// </summary>
        public static double MaxCalculableDistance
        {
            get { return Bot.Game.GetCols() + Bot.Game.GetRows(); }
        }

        /// <summary>
        ///     the distance that we consider that a stationary target are approaching an island
        /// </summary>
        public static double ApproachDistanceSquaredOfStationaryTarget
        {
            get { return Bot.Game.GetAttackRadius(); }
        }

        /// <summary>
        ///     The distance under wich two groups will be considered intersected
        /// </summary>
        public static double GroupIntersectionDistance
        {
            get { return Bot.Game.GetAttackRadius(); }
        }

        public static double MaxEnemyPredictionDistnace
        {
            get { return ((Bot.Game.GetCols() + Bot.Game.GetRows()) / 2 );}
        }

        #endregion
    }
}