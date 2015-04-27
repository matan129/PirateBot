#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        /// <summary>
        ///     The task the commander runs on
        /// </summary>
        private static Task _commanderTask;

        /// <summary>
        ///     The task the fallback bot runs on
        /// </summary>
        private static Task _fallbackTask;

        #endregion

        #region Fields & Properies

        /// <summary>
        ///     The current game
        /// </summary>
        public static IPirateGame Game { get; private set; }

        #endregion

        #region Interface Implementations

        /// <summary>
        ///     Invokes the bot. Called from cshRunner.
        /// </summary>
        /// <param name="state">The current game state</param>
        public void DoTurn(IPirateGame state)
        {
            //update the game so other classes will get updated data
            Bot.Game = state;
            
            bool commanderOk = false;

            try
            {
                /*                 
                 * NOTE: I changed the commander and Group classes so the do not move anything by themselves, they just return
                 * moving instructions in the form of a Dictionary<Pirate,Direction>. This is because we do not want to abort the commander 
                 * after it moved couple of pirates but not all of them                            
                 */
                //run the commander and check if it returned true (so it went OK)
                commanderOk = Bot.ExecuteBot();
            }
            catch (Exception ex) //catch any FATAL errors from the ExecuteBot method
            {
                Logger.Write("=================BOT ERROR=====================", true);
                Logger.Write("Bot almost crashed because of exception: " + ex.Message, true);

                //print the exception stack trace
                StackTrace exTrace = new StackTrace(ex, true);
                StackFrame frame = exTrace.GetFrame(0);
                Logger.Write(
                    string.Format("The exception was thrown from method {0} at file {1} at line #{2}", frame.GetMethod(),
                        frame.GetFileName(), frame.GetFileLineNumber()), true);

                Logger.Write("=================BOT ERROR=====================", true);
            }
            finally //whatever has happened there, move pirates so we will not crash
            {
                //Actually move stuff
                if (commanderOk) //if the commander was OK, use its results
                    Mover.MoveAll(Bot._movesDictionary);
                else //else use the fallback results
                    Mover.MoveAll(Bot._fallbackMoves);
            }
        }

        #endregion

        /// <summary>
        ///     Executes the commander with specified timeout, and the fallback in parallel
        /// </summary>
        /// <returns>if the commander exited gracefully or not</returns>
        private static bool ExecuteBot()
        {
            //clear the last moves
            Bot._fallbackMoves.Clear();
            Bot._movesDictionary.Clear();

            //setup time check flag
            bool onTime = false;

            //setup time remaining
            int time = Bot.Game.TimeRemaining();

            //setup safe timeout
            int safeTimeout = (int)(time * 0.65);
            CancellationTokenSource cancellationSource = new CancellationTokenSource(safeTimeout);

            //Commander task setup and start
            Bot._commanderTask =
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Bot._movesDictionary = Commander.Play(cancellationSource.Token, out onTime);
                    }
                    catch (Exception ex)
                    {
                        Logger.Write("TOP LEVEL EXCEPTION WAS CAUGHT ON THE COMMANDER TASK ON TURN " +
                                     Bot.Game.GetTurn(), true);
                        Logger.Write(ex.ToString(), true);
                    }
                });

            //Fallback task setup and start
            Bot._fallbackTask =
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Bot._fallbackMoves = FallbackBot.GetFallbackTurns(cancellationSource.Token);
                    }
                    catch (Exception ex)
                    {
                        Logger.Write("TOP LEVEL EXCEPTION WAS CAUGHT ON THE FALLBACK TASK ON TURN " +
                                     Bot.Game.GetTurn(), true);
                        Logger.Write(ex.ToString(), true);
                    }
                });

            //Wait for the tasks until the same timeout is over.
            //bloacks this thread until safeTimeout elapses
            Task.WaitAll(new Task[] {Bot._commanderTask, Bot._fallbackTask}, safeTimeout);

            //if it's stuck...
            if (!onTime)
            {
                Logger.Write("=================TIMEOUT=======================", true);
                Logger.Write("Commander timed out or errorer, switching to fallback code", true);
                Logger.Write("Time remaining: " + Bot.Game.TimeRemaining());
                Logger.Write("=================TIMEOUT=======================", true);
            }

            //do some profiling
            Logger.Profile();

            //return if the commander is on time / error status
            return onTime;
        }
    }
}