using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public static List<int> GetBestConfig(int forces = -1)
        {
            //TODO Actually analyze the game
            if (forces == -1)
            {
                forces = Bot.Game.AllMyPirates().Count;
                Bot.Game.Debug(forces.ToString());
            }

            List<int> config = new List<int>();

            int newFleet = 0;
            if (forces % 2 == 0 && forces != 2)
            {
                newFleet = forces/2;
            }
            else if (forces == 2)
            {
                newFleet = forces;
            }
            else if(forces != 0)
            {
                newFleet = 1;
            }

            config.Add(newFleet);
            if (forces - newFleet != 0)
            {
                config.AddRange(GetBestConfig(forces - newFleet));
            }

            return config;
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
