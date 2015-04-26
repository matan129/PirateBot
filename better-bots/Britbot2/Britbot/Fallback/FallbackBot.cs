#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
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
        public static volatile bool IsCloning = false;

        /// <summary>
        ///     This is the group that the previous commander had
        /// </summary>
        private static List<Group> Groups;

        /// <summary>
        ///     static constructor
        /// </summary>
        static FallbackBot()
        {
            FallbackBot.Groups = new List<Group>();
        }

        /// <summary>
        ///     Updates the fallback group according to the commander groups
        /// </summary>
        public static void SyncGroups()
        {
            FallbackBot.IsCloning = true;
            
            FallbackBot.IsCloning = false;
        }

        /// <summary>
        ///     Generates the fallback moves
        ///     This is basically the weaker sister of Commander.Play()
        /// </summary>
        /// <returns></returns>
        public static Dictionary<Pirate, Direction> GetFallbackTurns(CancellationToken cToken)
        {
            try
            {
                //FallbackBot.SyncGroups();
                FallbackBot.Groups.RemoveAll(g => g.Pirates.Count == 0);

                // Update all groups
                FallbackBot.Groups.ForEach(g => g.Update());

                //A list with all the moves from all groups
                List<KeyValuePair<Pirate, Direction>> allMoves =
                    new List<KeyValuePair<Pirate, Direction>>(Bot.Game.AllMyPirates().Count);

                //Get the moves from each group we have
                foreach (Group group in FallbackBot.Groups)
                {
                    List<KeyValuePair<Pirate, Direction>> mvs = group.GetGroupMoves(cToken).ToList();
                    allMoves.AddRange(mvs);
                }

                Logger.Write("===============FALLBACK READY=================");

                //Convert the moves list to dictionary
                return allMoves.ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            catch (Exception ex)
            {
                Logger.Write("======FALLBACK EXCEPTION=======",true);
                Logger.Write("Fallback almost crashed because of exception: " + ex.Message, true);
                
                StackTrace exTrace = new StackTrace(ex, true);
                StackFrame frame = exTrace.GetFrame(0);
                Logger.Write(
                    string.Format("The exception was thrown from method {0} at file {1} at line #{2}", frame.GetMethod(),
                        frame.GetFileName(), frame.GetFileLineNumber()), true);
                Logger.Write("======FALLBACK EXCEPTION=======", true);

                return FallbackBot.SuperFuckedFallback();
            }
        }

        /// <summary>
        ///     A really really bad super last resort that we use if we crash super hard
        /// </summary>
        /// <returns></returns>
        private static Dictionary<Pirate, Direction> SuperFuckedFallback()
        {
            Dictionary<Pirate, Direction> fallback = new Dictionary<Pirate, Direction>();
            foreach (Pirate pirate in Bot.Game.AllMyPirates())
            {
                if (Bot.Game.GetTurn() % 2 == 0)
                    fallback.Add(pirate, Direction.NORTH);
                else
                    fallback.Add(pirate, Direction.SOUTH);
            }

            return fallback;
        }
    }
}