using System;
using Pirates;


namespace Bots
{
    public class MyBot : Pirates.IPirateBot
    {
        public void DoTurn(IPirateGame game)
        {
            if (MyBotUtils.IsTurnMultipleOf100(game)) {
                game.Debug("Turn number divides in 100.");
            }
        }

    }
}
