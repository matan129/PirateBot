using System;
using System.Collections.Generic;
using System.Linq;

namespace SarcasticBot
{
    public static class Ai
    {
        /// <summary>
        /// This method estimates the best configuration to go with depending on various game parameters
        /// </summary>
        /// <param name="enemyConfig"></param>
        /// <returns>A suggested configuration</returns>
        public static List<int> GetBestConfig(List<int> enemyConfig)
        {
            List<int> config = new List<int>();
            int myForces = SmartGame.MySmartPirates.Count;
            int eForces = SmartGame.EnemySmartPirates.Count;

            if (SmartGame.SmartIslands.Count == 1)
            {
                config.Add(SmartGame.MySmartPirates.Count);
            }
            else
            {
                if (eForces > myForces)
                {
                    config.Add(1);
                    config.Add(myForces - 1);
                }
                else if (eForces == myForces)
                {
                    while (myForces > 3)
                    {
                        config.Add(2);
                        myForces -= 2;
                    }
                    config.Add(3);
                }
                else if (eForces < myForces)
                {
                    config.AddRange(Enumerable.Repeat(1,myForces));
                }
            }

            return config;
        }
    }
}