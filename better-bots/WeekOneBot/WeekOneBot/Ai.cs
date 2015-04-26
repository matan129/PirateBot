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

            if (forces == 5)
            {
                return new List<int>() {2, 3};
            }
            
            

            if (forces == 6)
            {
                return new List<int>() {2,2,2};
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

        public static int EnemiesNearLocation(int Radius, Location loc)
        {
            int c = 0;
            foreach (Pirate pirate in Bot.Game.AllEnemyPirates().Where(p => !p.IsLost))
            {
                if (Bot.Game.Distance(loc, pirate.Loc) <= Radius)
                {
                    c++;
                }
            }
            return c;
        }
    }
}
