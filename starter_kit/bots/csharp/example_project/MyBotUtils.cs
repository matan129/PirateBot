using System;
using System.Collections.Generic;
using Pirates;


namespace Bots
{
    public class MyBotUtils
    {
        public static bool IsTurnMultipleOf100(IPirateGame game)
        {
            return game.GetTurn() % 100 == 0;
        }

    }
}
