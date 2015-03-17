using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace WeekOneBot
{
    static class PiratesExtensions
    {
        public static Island GetIsland(this IPirateGame Game, Location Location)
        {
            foreach (Island isle in Game.Islands())
            {
                if (isle.Loc == Location)
                {
                    return isle;
                }
            }
            return null;
        }
    }
}
