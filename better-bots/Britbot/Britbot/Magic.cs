namespace Britbot
{
    /// <summary>
    ///     This class holds the magic numbers we have and may be used for smart in game updates
    /// </summary>
    internal static class Magic
    {
        #region Static Fields & Consts

        /// <summary>
        ///     Max safe iterator iterations per turn
        /// </summary>
        public static int MaxIterator = 10000;

        /// <summary>
        ///     Max distace two groups can be from eachother and still be joint
        /// </summary>
        public static int MaxDistance;

        #endregion

        #region Fields & Properies


        /// <summary>
        /// The radious of the area around enemy ships which we avoid
        /// </summary>
        public static double DangerZone
        {
            get { return 6 * Bot.Game.GetAttackRadius(); }
        }

        /// <summary>
        /// The radious of the maximum difference from enemy ships trajectory to an island so we will consider it to be going to the island
        /// </summary>
        public static double EnemyPredictionSensitivity
        {
            get { return 1.5 * Bot.Game.GetAttackRadius(); }
        }
        #endregion
    }
}