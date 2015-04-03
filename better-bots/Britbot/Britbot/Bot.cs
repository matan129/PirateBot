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
            const int TIMEOUT = 100; //The amount of time per turn. Remember to leave time for fallback bot
            try
            {
                //update the game so other classes will get updated data
                Game = state;

                //Begin Timing turn
                
                Timer (TIMEOUT);

                //play!
                //note that we do not have to explicitly initialize the commander, 
                //since we have a static constructor there
                Commander.Play();
            }
            catch (Exception ex)
            {
                Game.Debug("++++++++++++++++++++++++++++++++++++++++");
                Game.Debug("Almost crashed because of " + ex.Message);
                Game.Debug("++++++++++++++++++++++++++++++++++++++++");
            }
        }
       public static void Timer(int timeout)
        {
            Thread thread = new Thread(() => Thread.Sleep(100));
           thread.Start();
           Boolean intime = thread.Join(timeout);
           if (!intime)
           {
               thread.Abort();
               //Execute emergency override
               //Fallback code
           }
        }
    }
}