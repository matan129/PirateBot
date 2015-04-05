#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using System.Threading;
using Britbot.Fallback;
using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     This class is a bot.
    /// </summary>
    public class Bot : IPirateBot
    {
        #region Static Fields & Consts

        /// <summary>
        ///     The moves the commander decided on
        /// </summary>
        private static Dictionary<Pirate, Direction> _movesDictionary = new Dictionary<Pirate, Direction>();

        /// <summary>
        ///     The fallback moves the fallback bot generated
        /// </summary>
        private static Dictionary<Pirate, Direction> _fallbackMoves = new Dictionary<Pirate, Direction>();

        #endregion

        #region Fields & Properies

        /// <summary>
        ///     The current game, which is accessible to all the types in the assembly
        /// </summary>
        public static IPirateGame Game { get; private set; }

        #endregion

        #region Interface Implementations

        /// <summary>
        ///     Invokes out bot. Yeah.
        /// </summary>
        /// <param name="state">The current game state</param>
        public void DoTurn(IPirateGame state)
        {
            //update the game so other classes will get updated data
            Bot.Game = state;

            //Initialize all the stuff
            //Note that there is a flag in these classes making sure that Init() will run only once
            SmartIsland.Init();
            Commander.Init();

            bool commanderOk = false;

            try
            {
                /*                 
                 * NOTE: I changed the commander and Group classes so the do not move anything by themselves, they just return
                 * moving instructions in the form of a Dictionary<Pirate,Direction>. This is because we do not want to abort the commander 
                 * after it moved couple of pirates but not all of them                            
                 */
                commanderOk = Bot.ExecuteBot();
            }
            catch (Exception ex)
            {
                Bot.Game.Debug("=================BOT ERROR=====================");
                Bot.Game.Debug("Bot almost crashed because of exception: " + ex.Message);

                StackTrace exTrace = new StackTrace(ex, true);
                StackFrame frame = exTrace.GetFrame(0);
                Bot.Game.Debug("The exception was thrown from method {0} at file {1} at line #{2}", frame.GetMethod(),
                    frame.GetFileName(), frame.GetFileLineNumber());

                Bot.Game.Debug("=================BOT ERROR=====================");
            }
            finally
            {
                //Actually move stuff
                if (commanderOk)
                    Mover.MoveAll(Bot._movesDictionary);
                else
                    Mover.MoveAll(Bot._fallbackMoves);
            }
        }

        #endregion

        /// <summary>
        ///     Executes the commander with specified timeout, and the fallback in parallel
        /// </summary>
        /// <returns>if the commander is on time or not</returns>
        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        private static bool ExecuteBot()
        {
            //clear the last moves
            Bot._fallbackMoves.Clear();
            Bot._movesDictionary.Clear();

            //setup the threads
            Thread commanderThread = new Thread(() =>
            {
                try
                {
                    Dictionary<Pirate, Direction> moves = Commander.Play();
                    if (moves != null)
                        Bot._movesDictionary = moves;
                }
                catch
                {
                    // this will catch the ThreadAbortException
                }
            })
            {
                IsBackground = true,
                Name = "CommanderThread on turn " + Bot.Game.GetTurn()
            };

            Thread fallbackThread = new Thread(() =>
            {
                try
                {
                    Bot._fallbackMoves = FallbackBot.GetFallbackTurns();
                }
                catch
                {
                    // this will catch the ThreadAbortException
                }
            })
            {
                IsBackground = true,
                Name = "FallbackThread on turn " + Bot.Game.GetTurn()
            };

            //Start the threads simultaneously
            commanderThread.Start();
            fallbackThread.Start();

            //Test if the commander is finished on time. Give it 85% of the time remaining to be sure we won't timeout
            bool inTime = commanderThread.Join((int) (Bot.Game.TimeRemaining() * 0.85));

            //if it's stuck...
            if (!inTime)
            {
                //..Abort the commander thread. 
                //TODO this is dangerous, we have to switch to something a bit more reliable
                commanderThread.Abort();

                Bot.Game.Debug("=================TIMEOUT=======================");
                Bot.Game.Debug("Commander timed out, switching to fallback code");
                Bot.Game.Debug("=================TIMEOUT=======================");
            }

            //return if the commander is on time
            return inTime;
        }
    }
}