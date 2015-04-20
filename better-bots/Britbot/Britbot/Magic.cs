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

        public static int DangerZone
        {
            get { return 6 * Bot.Game.GetAttackRadius(); }
        }

        #endregion
    }
}