using System;
using Pirates;

namespace SarcasticBot
{
    public class Bot
    {
        public static IPirateGame Game;

        private bool CommanderInited = false;

        public void DoTurn(IPirateGame state)
        {
            if (CommanderInited != true)
            {
                Commander.Initialize();
                CommanderInited = true;
            }

            Commander.Play();
        }
    }
}