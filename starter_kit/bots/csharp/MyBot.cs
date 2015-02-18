using System;
using System.Collections.Generic;
using Pirates;
using System.Linq;

namespace MyBot
{
    public class MyBot : Pirates.IPirateBot
    {
        public void DoTurn(IPirateGame game)
        {

            if (game.NotMyIslands().Count() == 0)
            {
                return;
            }

            Island island = game.NotMyIslands().First();

            //game.Debug("going to island " + island.Id.ToString());

            foreach (Pirate pirate in game.MyPirates())
            {
                Direction dir = game.GetDirections(pirate, island).First();
                game.SetSail(pirate, dir);
            }
        }

    }
}
