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
        /// Static enemy constructor
        /// </summary>
        static Enemy()
        {
           Groups = new List<EnemyGroup>();
        }

        /// <summary>
        /// Split the enemy into its groups
        /// Should be invoked every turn to re-analyze
        /// </summary>
        public static List<EnemyGroup> AnalyzeEnemyGroups()
        {
            //TODO: check that you don't recreate enemy groups if you don't need to

            List<EnemyGroup> updatedGroups = new List<EnemyGroup>();

            //iterate over all the pirate of the enemy
            foreach (Pirate pete in Bot.Game.AllEnemyPirates().Where(p => !p.IsLost))
            {
                //create a new group and add the current pirate to it
                EnemyGroup newGroup = new EnemyGroup();
                newGroup.EnemyPirates.Add(pete.Id);

                //check if there are any older group already containing the current pirate
                List<EnemyGroup> containsPete = updatedGroups.Where(g => g.IsInGroup(pete.Id)).ToList();
                if (containsPete.Count > 0)
                {
                    //if there are, remove these groups
                    updatedGroups.RemoveAll(g => g.IsInGroup(pete.Id));

                    //Add the pirates from the groups we removed to the current new group
                    foreach (EnemyGroup gr in containsPete)
                    {
                        newGroup.EnemyPirates.AddRange(gr.EnemyPirates);
                    }
                }

                //TODO: important, it must be here or direction cant be calculated
                //Set location
                newGroup.PrevLoc = newGroup.GetLocation();

                //add the new group to the list of groups
                updatedGroups.Add(newGroup);
            }
            
            return updatedGroups;
        }

        /// <summary>
        /// Does every turn updating
        /// Should be called every turn 
        /// </summary>
        public static void Update()
        {
            Groups = AnalyzeEnemyGroups();
            foreach (EnemyGroup eGroup in Groups)
                eGroup.UpdateHeading();
        }

        /// <summary>
        /// Tranfers enemy configuration to int[] form
        /// </summary>
        /// <returns>Enemy configuration in the form of int array </returns>
        public static int[] GetConfig()
        {
            //Creates the config array
            int[] config = new int[Enemy.Groups.Count];

            //Fills the array according to enemy config
            for (int i = 0; i < Enemy.Groups.Count; i++)
            {
                config[i] = Enemy.Groups[i].EnemyPirates.Count;
            }

            return config;

        }
    }
}