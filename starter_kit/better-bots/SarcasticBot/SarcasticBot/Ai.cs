using System;
using System.Collections.Generic;
using System.Linq;
using Pirates;

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

        /// <summary>
        /// Sets the Commander's  strategy queue according to the game status
        /// </summary>
        public static void SetCommanderStrategy()
        {
            if (Bot.Game.Islands().Count == 1)
            {
                Commander.Strategy.Enqueue(CommanderStrategy.GatherForces);
                Commander.Strategy.Enqueue(CommanderStrategy.JoinAll);
                Commander.Strategy.Enqueue(CommanderStrategy.ManueverAttack);
            }
            else if (Enemy.Causalties() > 0.75)
            {
                Commander.Strategy.Enqueue(CommanderStrategy.SplitAll);
                Commander.Strategy.Enqueue(CommanderStrategy.AgressiveAttack);
            }
            else if (Commander.Casualties() > 0.5)
            {
                Commander.Strategy.Enqueue(CommanderStrategy.GatherForces);
                Commander.Strategy.Enqueue(CommanderStrategy.JoinAll);
                Commander.Strategy.Enqueue(CommanderStrategy.AgressiveAttack);
            }
        }

        /// <summary>
        /// Splits the enemy into groups
        /// </summary>
        /// <returns>A list of the enemy group</returns>
        public static List<EnemyGroup> AnalyzeEnemyConfiguration()
        {
            List<EnemyGroup> groups = new List<EnemyGroup>();

            foreach (EnemyPirate pete in SmartGame.EnemySmartPirates.Where(p => !p.IsLost()))
            {
                EnemyGroup newGroup = new EnemyGroup();
                newGroup.EnemyPirates.Add(pete);

                //TODO Can be more efficient 
                List<EnemyGroup> containsPete = groups.Where(g => g.IsInGroup(pete)).ToList();

                if (containsPete.Count > 0)
                {
                    groups.RemoveAll(g => g.IsInGroup(pete));

                    foreach (EnemyGroup gr in containsPete)
                    {
                        newGroup.EnemyPirates.AddRange(gr.EnemyPirates);
                    }
                }
                
                groups.Add(newGroup);
            }

            return groups;
        }

    }
}