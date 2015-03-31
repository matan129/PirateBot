using System;
using Pirates;

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
            try
            {
                //update the game so other classes will get updated data
                Game = state;

                //play!
                //note that we do not have to explicitly initialize the commander, 
                //since we have a static constructor there
                Commander.Play();

            }
            catch (Exception ex)
            {
                Bot.Game.Debug("++++++++++++++++++++++++++++++++++++++++");
                Bot.Game.Debug("Almost crashed because of " + ex.Message);
                Bot.Game.Debug("++++++++++++++++++++++++++++++++++++++++");
            }
        }

    }
}