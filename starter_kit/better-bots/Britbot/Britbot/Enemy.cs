using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Britbot
{
    /// <summary>
    /// This class represent the enemy bot in the game
    /// </summary>
    public static class Enemy
    {
        /// <summary>
        /// A list of the enemy's groups
        /// </summary>
        public static List<EnemyGroup> Groups { get; private set; }

        /// <summary>
        /// Split the enemy into its groups
        /// </summary>
        public static void AnalyzeConfig()
        {
            List<EnemyGroup> groups = new List<EnemyGroup>();

            //iterate over all the pirate of the enemy
            foreach (Pirate pete in Bot.Game.AllEnemyPirates().Where(p => !p.IsLost))
            {
                //create a new group and add the current pirate to it
                EnemyGroup newGroup = new EnemyGroup();
                newGroup.EnemyPirates.Add(pete.Id);

                //check if there are any older group already containing the current pirate
                List<EnemyGroup> containsPete = groups.Where(g => g.IsInGroup(pete.Id)).ToList();
                if (containsPete.Count > 0)
                {
                    //if there are, remove these groups
                    groups.RemoveAll(g => g.IsInGroup(pete.Id));

                    //Add the pirates from the groups we removed to the current new group
                    foreach (EnemyGroup gr in containsPete)
                    {
                        newGroup.EnemyPirates.AddRange(gr.EnemyPirates);
                    }
                }

                //add the new group to the list of groups
                groups.Add(newGroup);
            }

            Groups = groups;
        }
    }
}