using System;
using Pirates;
using System.Threading;

namespace Britbot
{
    using System.Collections.Generic;

    /// <summary>
    /// This class is a bot.
    /// </summary>
    public class Bot : IPirateBot
    {
        /// <summary>
        /// The current game, which is accessible to all the types in the assembly
        /// </summary>
        public static IPirateGame Game { get; private set; }

        /// <summary>
        /// Makes a move. Basically invokes the commander.
        /// </summary>
        /// <param name="state">The current game state</param>
        public void DoTurn(IPirateGame state)
        {
            //update the game so other classes will get updated data
            Bot.Game = state;

            //The amount of time per turn. Remember to leave time for fallback bot
            int timeout = Bot.Game.TimeRemaining();

            //A dictionary of moves that will be executed later
            Dictionary<Pirate,Direction> allMoves = new Dictionary<Pirate, Direction>();

            try
            {
                /*
                 * Begin the commander stuff with 85% of the time we have so there will be time for fallback
                 * NOTE: I changed the commander and Group classes so the do not move anything by themselves, they just return
                 * moving instructions in the form of a Dictionary<Pirate,Direction>. This is because we do not want to abort the commander 
                 * after it moved couple of pirates but not all of them           
                 */
                allMoves = Bot.ExecuteCommander((int)(timeout * 0.85));
            }
            catch (Exception ex)
            {
                Bot.Game.Debug("+++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                Bot.Game.Debug("Bot almost crashed because of exception: " + ex.Message);
                Bot.Game.Debug("+++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            }
            finally
            {
                if (allMoves == null)
                    allMoves = DoFallback();

                //Actually move stuff
                Mover.MoveAll(allMoves);
            }
            
        }

        /// <summary>
        /// Executes the commander with specified timeout
        /// </summary>
        /// <param name="timeout">timeout in milliseconds</param>
        public static Dictionary<Pirate,Direction> ExecuteCommander(int timeout)
        {
            Dictionary<Pirate,Direction> movesDictionary = new Dictionary<Pirate, Direction>();

            //Setup a thread which will start the Commander.Play method when the it starts
            Thread commanderThread = new Thread(() => movesDictionary = Commander.Play());

            //Start the thread
            commanderThread.Start();

            //Test if the commander is finished on time
            bool inTime = commanderThread.Join(timeout);
            
            //if it's stuck...
            if (!inTime)
            {
                //..abort it and continue to the fallback code
                commanderThread.Abort();

                Bot.Game.Debug("###############################################");
                Bot.Game.Debug("Commander timed out, switching to fallback code");
                Bot.Game.Debug("###############################################");

                //TODO Execute emergency override/fallback code
                //i.e. return GetFallbackMoves();
                
                //I will throw an exception in the meanwhile so this code will properly compile
                //throw new NotImplementedException();
                return Bot.DoFallback();
            }
            else
            {
                return movesDictionary;
            }
        }

        static Dictionary<Pirate,Direction> DoFallback()
        {
            Dictionary<Pirate, Direction> fallback= new Dictionary<Pirate, Direction>();
            foreach (Pirate pirate in Bot.Game.AllMyPirates())
            {
                fallback.Add(pirate,Direction.NOTHING);
            }   
            return fallback;
        }
    }
}
