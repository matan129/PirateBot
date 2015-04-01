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
            List<EnemyGroup> analysis = AnalyzeFull();
            List<EnemyGroup> veteranGroups = new List<EnemyGroup>(analysis.Count);

            if (Groups.Count == 0)
                return analysis;

            bool[] removeAtAnalysis = new bool[analysis.Count];

            for (int i = 0; i < analysis.Count; i++)
            {
                EnemyGroup enemyGroup = analysis[i];
                foreach (EnemyGroup veteran in Groups)
                {
                    /*
                     * check if the groups are the same.
                     * Note that Equals() does a deep comparison 
                     * (I overrided it to check if the pirates in each enemy group are the same)                    
                     */
                    if (Equals(veteran, enemyGroup))
                    {
                        /* 
                         * note that we are adding the group already in the old Groups list
                         * it's the same object
                         */
                        veteranGroups.Add(veteran);
                        removeAtAnalysis[i] = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < analysis.Count; i++)
            {
                analysis.RemoveAt(i);
            }
            
            analysis.AddRange(veteranGroups);
            return analysis;
        }

        /// <summary>
        /// Normal analysis of enemy groups, without considering the previous configurations
        /// Note that using this method alone will break the heading mechanism because this method
        /// technically return new groups each time (although they might be the same config) 
        /// </summary>
        /// <returns>A list of enemy groups</returns>
        private static List<EnemyGroup> AnalyzeFull()
        {
            List<EnemyGroup> updatedGroups = new List<EnemyGroup>();
            IEnumerable<Pirate> enemyAlivePirates = Bot.Game.AllEnemyPirates().Where(p => !p.IsLost);

            //iterate over all the alive pirate of the enemy
            foreach (Pirate pete in  enemyAlivePirates)
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

                //important, it must be here or direction cant be calculated
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
            List<EnemyGroup> updated = AnalyzeEnemyGroups();
            Groups = Groups.Intersect(updated).ToList();
            Groups = Groups.Union(updated).ToList();

            foreach (EnemyGroup eGroup in Groups)
                eGroup.UpdateHeading();
        }

        /// <summary>
        /// Transfers enemy configuration to int[] form
        /// </summary>
        /// <returns>Enemy configuration in the form of int array </returns>
        public static int[] GetConfig()
        {
            //Creates the config array
            int[] config = new int[Groups.Count];

            //Fills the array according to enemy config
            for (int i = 0; i < Groups.Count; i++)
            {
                config[i] = Groups[i].EnemyPirates.Count;
            }

            return config;
        }
    }
}