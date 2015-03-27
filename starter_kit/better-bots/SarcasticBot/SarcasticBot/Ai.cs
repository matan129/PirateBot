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
        public static List<int> GetBestConfig()
        {
            List<int> config = new List<int>();
            List<int> enemyConfig = AnalyzeEnemyConfiguration().ConvertAll(x => x.EnemyPirates.Count);
            int myForces = Bot.Game.AllMyPirates().Count;
            int eForces = Bot.Game.AllEnemyPirates().Count;

            //Bigger groups go first
            enemyConfig.Sort((a,b) => a.CompareTo(b));
            
            if(eForces == myForces)
                config.AddRange(enemyConfig);

            return config;
        }

        /// <summary>
        /// Sets the Commander's  strategy queue according to the game status
        /// </summary>
        public static void SetCommanderActions()
        {
            if (Bot.Game.Islands().Count == 1)
            {
                Commander.Actions.Enqueue(CommanderAction.JoinAll);
                Commander.Actions.Enqueue(CommanderAction.GatherForces);
                Commander.Actions.Enqueue(CommanderAction.ManeuverAttack);
            }
            else if (Enemy.Causalties() > 0.75)
            {
                Commander.Actions.Enqueue(CommanderAction.SplitAll);
                Commander.Actions.Enqueue(CommanderAction.AggressiveConquest);
            }
            else if (Commander.Casualties() > 0.5)
            {
                Commander.Actions.Enqueue(CommanderAction.JoinAll);
                Commander.Actions.Enqueue(CommanderAction.GatherForces);
                Commander.Actions.Enqueue(CommanderAction.AggressiveConquest);
            }
            else if (Bot.Game.GetTurn() > 25)
            {
                Commander.Actions.Enqueue(CommanderAction.AskForNewConfig);
            }
        }

        /// <summary>
        /// Splits the enemy into groups
        /// </summary>
        /// <returns>A list of the enemy group</returns>
        public static List<EnemyGroup> AnalyzeEnemyConfiguration()
        {
            List<EnemyGroup> groups = new List<EnemyGroup>();

            foreach (Pirate pete in Bot.Game.AllEnemyPirates().Where(p => !p.IsLost))
            {
                EnemyGroup newGroup = new EnemyGroup();
                newGroup.EnemyPirates.Add(pete.Id);

                //TODO Can be more efficient 
                List<EnemyGroup> containsPete = groups.Where(g => g.IsInGroup(pete.Id)).ToList();

                if (containsPete.Count > 0)
                {
                    groups.RemoveAll(g => g.IsInGroup(pete.Id));

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