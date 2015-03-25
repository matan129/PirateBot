using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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

            int enemyForces = Bot.Game.AllEnemyPirates().Count;

            List<int> config = new List<int>();

            if (forces > 3 && Bot.Game.Islands().Count > 1)
            {
                while (forces > 3)
                {
                    config.Add(2);
                    forces -= 2;
                }
                config.Add(forces);
            }
            else
            {
                config.Add(forces);   
            }

            return config;
        }

        public static int EstimatePiratesNearIsland(int index)
        {
            int count = 0;
            Island isle = Bot.Game.GetIsland(index);
            foreach (Pirate enemyPirate in Bot.Game.EnemyPirates())
            {
                if(Bot.Game.InRange(enemyPirate,isle.Loc))
                {
                    count++;
                }
            }

            return 0;
        }
    }
}
