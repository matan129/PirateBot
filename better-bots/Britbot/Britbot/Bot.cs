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
        /// Array of the tasks. These are the normal commander task and the fallback task. See below.
        /// </summary>
        private static Task[] _tasks = new Task[2];

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
            Game = state;            

            bool commanderOk = false;

            try
            {
                /*                 
                 * NOTE: I changed the commander and Group classes so the do not move anything by themselves, they just return
                 * moving instructions in the form of a Dictionary<Pirate,Direction>. This is because we do not want to abort the commander 
                 * after it moved couple of pirates but not all of them                            
                 */
                commanderOk = ExecuteBot();
            }
            catch (Exception ex)
            {
                Game.Debug("=================BOT ERROR=====================");
                Game.Debug("Bot almost crashed because of exception: " + ex.Message);

                StackTrace exTrace = new StackTrace(ex, true);
                StackFrame frame = exTrace.GetFrame(0);
                Game.Debug("The exception was thrown from method {0} at file {1} at line #{2}", frame.GetMethod(),
                    frame.GetFileName(), frame.GetFileLineNumber());

                Game.Debug("=================BOT ERROR=====================");
            }
            finally
            {
                //Actually move stuff
                if (commanderOk)
                    Mover.MoveAll(_movesDictionary);
                else
                    Mover.MoveAll(_fallbackMoves);
            }
        }

        #endregion

        /// <summary>
        ///     Executes the commander with specified timeout, and the fallback in parallel
        /// </summary>
        /// <returns>if the commander is on time or not</returns>
        private static bool ExecuteBot()
        {
            //clear the last moves
            _fallbackMoves.Clear();
            _movesDictionary.Clear();

            //setup time check flag
            bool onTime = false;

            //setup time remaining
            int time;
            if (Game.GetTurn() > 1)
                time = Game.TimeRemaining();
            else
                time = 1000; //1000 ms

            time = 1000;
            int safeTimeout = (int)(time * 1000.65);

            //timeout setup
            using (CancellationTokenSource commanderCancellationSource = new CancellationTokenSource(safeTimeout))
            {
                //Commander task setup and start
                _tasks[0] =
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            _movesDictionary = Commander.Play(commanderCancellationSource.Token, out onTime);
                        }
                        catch (Exception ex)
                        {
                            Game.Debug("TOP LEVEL EXCEPTION WAS CAUGHT ON THE COMMANDER TASK ON TURN " +
                                       Game.GetTurn());
                            Game.Debug(ex.ToString());
                        }
                    });

                //Fallback task setup and start
                _tasks[1] =
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            _fallbackMoves = FallbackBot.GetFallbackTurns();
                        }
                        catch (Exception ex)
                        {
                            Game.Debug("TOP LEVEL EXCEPTION WAS CAUGHT ON THE FALLBACK TASK ON TURN " +
                                       Game.GetTurn());
                            Game.Debug(ex.ToString());
                        }
                    });

                //Wait for the tasks until the same timeout is over.
                Task.WaitAll(_tasks, safeTimeout);

                //if it's stuck...
                if (!onTime)
                {
                    Game.Debug("=================TIMEOUT=======================");
                    Game.Debug("Commander timed out, switching to fallback code");
                    Game.Debug("=================TIMEOUT=======================");
                }
            }
            //return if the commander is on time
            return onTime;
        }
    }
}