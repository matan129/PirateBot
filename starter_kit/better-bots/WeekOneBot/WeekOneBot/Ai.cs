using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace WeekOneBot
{
    static class Ai
    {

        public static int[] GetBestConfig()
        {
            //TODO Actually analyze the game
            return new int[] {2,2,1};
        }

        public static int EstimatePiratesNearIsland(int index)
        {
            int count = 0;
            Island isle = Bot.Game.GetIsland(index);
            foreach (Pirate enemyPirate in Bot.Game.EnemyPirates())
            {
                if (Bot.Game.Distance(enemyPirate, isle) < 8)
                {
                    count++;
                }
            }

            return 0;
        }
    }
}
