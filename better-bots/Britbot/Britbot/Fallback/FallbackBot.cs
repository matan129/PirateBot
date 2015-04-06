#region Usings

using System;
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
        ///     This is basically the weaker sister of Commander.Play()
        /// </summary>
        /// <returns></returns>
        public static Dictionary<Pirate, Direction> GetFallbackTurns()
        {
            Dictionary<Pirate, Direction> fallback = new Dictionary<Pirate, Direction>();

            if (Commander.Groups == null || Commander.Groups.Count == 0)
                //the commander isn't inted so we have to create the groups ourselves
                Commander.Groups = FallbackBot.GetFallbackGroups();

            Bot.Game.Debug("******* FALLBACK READY *******");

            return fallback;
        }

        private static List<Group> GetFallbackGroups()
        {
            throw new NotImplementedException();
        }
    }
}