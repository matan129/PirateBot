using System;
using Pirates;
using System.Threading;

namespace Britbot
{
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

            try
            {
                //Begin the commander stuff with 85% of the time we have so there will be time for fallback
                //TODO this may rarely stop the commander after it moved couple of groups but not all of them
                //So maybe we should change it so the Commander.Play() method will return instructions to a pirate mover that will be invoked anyways
                //And this way we can run the fallback code simultaneously and execute its instructions if needed
                Bot.ExecuteCommander((int)(timeout * 0.85));
            }
            catch (Exception ex)
            {
                Bot.Game.Debug("+++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                Bot.Game.Debug("Bot almost crashed because of exception: " + ex.Message);
                Bot.Game.Debug("+++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            }
        }

        /// <summary>
        /// Executes the commander with specified timeout
        /// </summary>
        /// <param name="timeout">timeout in milliseconds</param>
        public static void ExecuteCommander(int timeout)
        {
            //Setup a thread which will start the Commander.Play method when the it starts
            //This is also valid: Thread commanderThread = new Thread(() => Commander.Play());
            Thread commanderThread = new Thread(Commander.Play);

            //Start the thread
            commanderThread.Start();

            //Test if the commander is finished on time
            bool inTime = commanderThread.Join(timeout);
            
            //if it's stuck...
            if (!inTime)
            {
                //..abort it and continue to the fallback code
                commanderThread.Abort();

                Bot.Game.Debug("##############################################");
                Bot.Game.Debug("Commander time out, switching to fallback code");
                Bot.Game.Debug("##############################################");

                //TODO Execute emergency override/fallback code
            }
        }
    }
}
