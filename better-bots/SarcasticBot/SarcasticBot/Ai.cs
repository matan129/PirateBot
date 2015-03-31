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
        /// <returns>A queue of actions</returns>
        public static Queue<CommanderAction> SuggestCommanderActions()
        {
            Queue<CommanderAction> actions = new Queue<CommanderAction>();
            if (Bot.Game.Islands().Count == 1)
            {
                actions.Enqueue(CommanderAction.JoinAll);
                actions.Enqueue(CommanderAction.GatherForces);
                actions.Enqueue(CommanderAction.ManeuverAttack);
            }
            else if (Enemy.Causalties() > 0.75)
            {
                actions.Enqueue(CommanderAction.SplitAll);
                actions.Enqueue(CommanderAction.AggressiveConquest);
            }
            else if (Commander.Casualties() > 0.5)
            {
                actions.Enqueue(CommanderAction.JoinAll);
                actions.Enqueue(CommanderAction.GatherForces);
                actions.Enqueue(CommanderAction.AggressiveConquest);
            }
            else if (Bot.Game.GetTurn() > 25)
            {
                actions.Enqueue(CommanderAction.AskForNewConfig);
            }
            
            return actions;
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

        /// <summary>
        /// Prioritize the islands
        /// </summary>
        /// <returns>The prioritized queue of islands</returns>
        public static PriorityQueue<int, int> PrioritizeTargets()
        {
            PriorityQueue<int,int> targetQueue = new PriorityQueue<int, int>();
            
            foreach (Island island in Bot.Game.Islands())
            {
                targetQueue.Enqueue(island.Id, GetIslandPriority(island));
            }

            return targetQueue;
        }

        /// <summary>
        /// Get a priority for an island (lower means faster dequeue-ing)
        /// </summary>
        /// <param name="island">The island to evaluate</param>
        /// <returns>The priority score for the island</returns>
        private static int GetIslandPriority(Island island)
        {
            //Lower priority is better
            if (island.Owner == Consts.ME && island.TeamCapturing == Consts.NO_OWNER)
                return 1000;
            else if (island.Owner == Consts.ME && island.TeamCapturing == Consts.ENEMY)
            {
                //Killing the enemy bonus
                return -(island.Value - Bot.Game.Distance(island.Loc, new Location(0, 0)) + 100);
            }
            else if (island.Owner == Consts.ENEMY)
            {
                //Conquering an enemy island bonus
                return -(island.Value - Bot.Game.Distance(island.Loc, new Location(0, 0)) + 200);
            }
            
            return -(island.Value - Bot.Game.Distance(island.Loc, new Location(0, 0)));
        }
    }
}