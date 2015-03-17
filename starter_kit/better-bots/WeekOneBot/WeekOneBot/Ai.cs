using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace WeekOneBot
{
    static class Ai
    {
        public static IPirateGame game;

        public static int[] GetBestConfig()
        {
            return new int[] {2,3};
        }
    }
}
