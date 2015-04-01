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
        /// Static enemy constructor
        /// </summary>
        static Enemy()
        {
            Groups = new List<EnemyGroup>();
        }

        /// <summary>
        /// A list of the enemy's groups
        /// </summary>
        public static List<EnemyGroup> Groups { get; private set; }

        /// <summary>
        /// Split the enemy into its groups
        /// Should be invoked every turn to re-analyze
        /// </summary>
        public static List<EnemyGroup> AnalyzeEnemyGroups()
        {
            //If there are no groups yet
            if (Groups.Count == 0)
            {
                return AnalyzeFull();
            }


            /*TODO this IS not working! 
             * the logic here is incorrect
             * 1. the retiredGroup never changes here and hence the do-while loop will go on forever
             * 2. What happenes if an enemygroup splits into three groups?
             * 3. WHat happenes if someone jons a group?
            */
            //the updated list
            List<EnemyGroup> newEnemyList = new List<EnemyGroup>();

            //first split group if we need to 
            //go over all the existing groups and update them if needed
            foreach (EnemyGroup eGroup in Groups)
            {
                //assuming eGroup isn't empty
                EnemyGroup retiredGroup;
                List<int> retiredPirates = new List<int>();

                //split the group for smaller group if needed
                do
                {
                    //setting pivot
                    retiredPirates.Add(eGroup.EnemyPirates[0]);

                    //going over all the others
                    for (int i = 1; i < eGroup.EnemyPirates.Count; i++)
                    {
                        //if the new pirate is close to any of the previous, add him
                        if (EnemyGroup.IsInGroup(retiredPirates, eGroup.EnemyPirates[i]))
                            retiredPirates.Add(eGroup.EnemyPirates[i]);
                    }

                    //retire those pirates
                    retiredGroup = eGroup.Split(retiredPirates);

                    //check if there were actually pirates retired or just the entire groups stayed together
                    if (retiredGroup != null)
                        newEnemyList.Add(retiredGroup);
                } while (retiredPirates != null);
                //here we add the pirates left in the last iteration
                newEnemyList.Add(eGroup);
            }

            //TODO: now combine them if we need to
            return newEnemyList;
        }

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
            List<EnemyGroup> updated = AnalyzeEnemyGroups();
            Groups = Groups.Intersect(updated).ToList();
            Groups = Groups.Union(updated).ToList();

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