using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pirates;

namespace Britbot
{
    public static class Enemy
    {
        public static List<EnemyGroup> Groups { get; private set; }
    
        /// <summary>
        /// Split the enemy into its groups
        /// </summary>
        public static void AnalyzeConfig()
        {
            List<EnemyGroup> groups = new List<EnemyGroup>();
            foreach (Pirate pete in Bot.Game.AllEnemyPirates().Where(p => !p.IsLost))
            {
                EnemyGroup newGroup = new EnemyGroup();
                newGroup.EnemyPirates.Add(pete.Id);
                
                //TODO May be more efficient
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

            Groups = groups;
        }
    }
}
