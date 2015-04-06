#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
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
            Enemy.Init();

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
            
            //setup time check flag
            bool onTime = false;

            //setup time remaining
            int time;
            if (Bot.Game.GetTurn() > 1)
                time = Bot.Game.TimeRemaining();
            else
                time = 1000; //1000 ms

            int safeTimeout = (int) (time * 0.85);

            //timeout setup
            CancellationTokenSource commanderCancellationSource = new CancellationTokenSource(safeTimeout);

            //Commander task setup and start
            Task commanderTask =
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Bot._movesDictionary = Commander.Play(commanderCancellationSource.Token, out onTime);
                    }
                    catch (Exception ex)
                    {
                        Bot.Game.Debug("TOP LEVEL EXCEPTION WAS CAUGHT ON THE COMMANDER TASK ON TURN " +
                                       Bot.Game.GetTurn());
                        Bot.Game.Debug(ex.ToString());
                    }
                    
                });
                    

            //Fallback task setup and start
            Task fallbackTask =
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Bot._fallbackMoves = FallbackBot.GetFallbackTurns();
                    }
                    catch (Exception ex)
                    {
                        Bot.Game.Debug("TOP LEVEL EXCEPTION WAS CAUGHT ON THE FALLBACK TASK ON TURN " +
                                       Bot.Game.GetTurn());
                        Bot.Game.Debug(ex.ToString());
                    }
                });

            //Wait for the tasks for some time
            commanderTask.Wait(safeTimeout);
            //fallbackTask.Wait();

            //if it's stuck...
            if (!onTime)
            {
                Bot.Game.Debug("=================TIMEOUT=======================");
                Bot.Game.Debug("Commander timed out, switching to fallback code");
                Bot.Game.Debug("=================TIMEOUT=======================");
            }

            //return if the commander is on time
            return onTime;
        }
    }
}