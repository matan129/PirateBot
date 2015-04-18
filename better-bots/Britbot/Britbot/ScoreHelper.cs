#region #Usings

using System;

#endregion

namespace Britbot
{
    /// <summary>
    ///     Score calculation utilities
    /// </summary>
    internal static class ScoreHelper
    {
        /// <summary>
        ///     Calculates the score for each turn
        ///     (Points Per Turn)
        /// </summary>
        /// <param name="totalIslandValues">Totals island value (i.e. there are islands worth two, etc)</param>
        /// <returns></returns>
        internal static int ComputePPT(int totalIslandValues)
        {
            return (int) Math.Floor(Math.Pow(2, totalIslandValues - 1));
        }

        /// <summary>
        ///     Calculates the score for each turn
        ///     (Points Per Turn)        ///
        /// </summary>
        /// <param name="totalIslandValues">Total added island value (i.e. there are islands worth two, etc)</param>
        /// <returns></returns>
        internal static double ComputePPT(double totalIslandValues)
        {
            //check if we have no islands
            if(Bot.Game.MyIslands().Count == 0)
                return Math.Floor(Math.Pow(2, totalIslandValues - 1));
            //otherise we multiply with what we already get each turn
            return Math.Floor(Math.Pow(2,Bot.Game.MyIslands().Count + totalIslandValues - 1));
        }
    }
}