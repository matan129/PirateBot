using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace SarcasticBot
{
    static class Extensions
    {
        public static Direction GetDirections(this IPirateGame game, FriendPirate pirate, Location loc)
        {
            return game.GetDirections(game.GetMyPirate(pirate.Index), loc).First();
        }
    }
}
