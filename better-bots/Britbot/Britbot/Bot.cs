#region Usings

using System;
using System.Collections.Generic;
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
            Game = state;

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
                commanderOk = ExecuteBot();
            }
            catch (Exception ex)
            {
                Game.Debug("=================BOT ERROR=====================");
                Game.Debug("Bot almost crashed because of exception: " + ex.Message);
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
        [SecurityPermission(SecurityAction.Demand)]
        private static bool ExecuteBot()
        {
            //clear the last moves
            _fallbackMoves.Clear();
            _movesDictionary.Clear();

            //setup the threads
            Thread commanderThread = new Thread(() =>
            {
                try
                {
                    Dictionary<Pirate, Direction> moves = Commander.Play();
                    if (moves != null)
                        _movesDictionary = moves;
                }
                catch
                {
                    // this will catch the ThreadAbortException
                }
            }) {IsBackground = true};

            Thread fallbackThread = new Thread(() =>
            {
                try
                {
                    _fallbackMoves = FallbackBot.GetFallbackTurns();
                }
                catch
                {
                    // this will catch the ThreadAbortException
                }
            })
            {
                IsBackground
                    = true
            };

            //Start the threads simultaneously
            commanderThread.Start();
            fallbackThread.Start();

            //Test if the commander is finished on time
            bool inTime = commanderThread.Join((int) (Game.TimeRemaining() * 0.85));

            //if it's stuck...
            if (!inTime)
            {
                commanderThread.Abort();
                Game.Debug("=================TIMEOUT=======================");
                Game.Debug("Commander timed out, switching to fallback code");
                Game.Debug("=================TIMEOUT=======================");
            }

            //return if the commander is on time
            return inTime;
        }
    }
}