namespace Britbot
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fallback;
    using Pirates;

    /// <summary>
    ///     This class is a bot.
    /// </summary>
    public class Bot : IPirateBot
    {
        /// <summary>
        /// The current game, which is accessible to all the types in the assembly
        /// </summary>
        public static IPirateGame Game { get; private set; }

        /// <summary>
        /// The moves the commander decided on
        /// </summary>
        private static Dictionary<Pirate, Direction> _movesDictionary = new Dictionary<Pirate, Direction>();

        /// <summary>
        /// The fallback moves the fallback bot generated
        /// </summary>
        private static Dictionary<Pirate, Direction> _fallbackMoves = new Dictionary<Pirate, Direction>();
        
        /// <summary>
        /// Executes the commander with specified timeout, and the fallback in parallel
        /// </summary>
        /// <returns>if the commander is on time or not</returns>
        private static bool ExecuteBot()
        {
            //setup the threads
            Thread commanderThread = new Thread(() => Bot._movesDictionary = Commander.Play());
            Thread fallbackThread = new Thread(() => Bot._fallbackMoves = FallbackBot.GetFallbackTurns());

            //Start the threads simultaneously
            commanderThread.Start();
            fallbackThread.Start();

            //Test if the commander is finished on time
            bool inTime = commanderThread.Join((int) (Bot.Game.TimeRemaining()*0.5));
            //Bot._fallbackThread.Join();

            //if it's stuck...
            if (!inTime)
            {
                //..abort it and continue to the fallback code
                //commanderThread.Abort();

                Bot.Game.Debug("=================TIMEOUT=======================");
                Bot.Game.Debug("Commander timed out, switching to fallback code");
                Bot.Game.Debug("=================TIMEOUT=======================");
                
            }
            
            //return if the commander is on time
            return inTime;
        }

        /// <summary>
        /// Invokes out bot. Yeah.
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
                commanderOk = Bot.ExecuteBot();
            }
            catch (Exception ex)
            {
                Bot.Game.Debug("==========ERROR==========");
                Bot.Game.Debug("Bot almost crashed because of exception: " + ex.Message);
                Bot.Game.Debug("==========ERROR==========");
            }
            finally
            {
                //Actually move stuff
                if(commanderOk)
                    Mover.MoveAll(Bot._movesDictionary);
                else
                    Mover.MoveAll(Bot._fallbackMoves);
            }
        }
    }
}