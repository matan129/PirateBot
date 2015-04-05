#region Usings

using System.Collections.Generic;
using Pirates;

#endregion

namespace Britbot.Fallback
{
    /// <summary>
    ///     This is the fallback bot
    ///     its execution time must be low
    /// </summary>
    public class FallbackBot
    {
        /// <summary>
        ///     Generates the fallback moves
        /// </summary>
        /// <returns></returns>
        public static Dictionary<Pirate, Direction> GetFallbackTurns()
        {
            Dictionary<Pirate, Direction> fallback = new Dictionary<Pirate, Direction>();
            foreach (Pirate pirate in Bot.Game.AllMyPirates())
            {
                if (Bot.Game.GetTurn() % 2 == 0)
                    fallback.Add(pirate, Direction.NORTH);
                else
                    fallback.Add(pirate, Direction.SOUTH);
            }

            Bot.Game.Debug("===============FALLBACK READY=================");

            return fallback;
        }
    }
}