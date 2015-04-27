#region #Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     This class represent the enemy bot in the game
    /// </summary>
    public static class Enemy
    {
        #region Static Fields & Consts

        /// <summary>
        ///     counter to check if we should try to catch targets
        /// </summary>
        public static int EnemyIntelligenceSuspicionCounter = 0;

        #endregion

        #region Fields & Properies

        /// <summary>
        ///     A list of the enemy's groups
        /// </summary>
        public static List<EnemyGroup> Groups { get; private set; }

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     Static enemy constructor
        /// </summary>
        static Enemy()
        {
            Enemy.Groups = new List<EnemyGroup>();
        }

        #endregion

        /// <summary>
        ///     Split the enemy into its groups
        ///     Should be invoked every turn to re-analyze
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
        public static List<EnemyGroup> AnalyzeEnemyGroups(CancellationToken cancellationToken)
        {
            EnemyGroup[] analysis = Enemy.AnalyzeFull(cancellationToken).ToArray();

            if (Enemy.Groups.Count == 0)
                return analysis.ToList();

            List<EnemyGroup> veteranGroups = new List<EnemyGroup>(analysis.Length);
            bool[] removeAtAnalysis = new bool[analysis.Length];

            for (int i = 0; i < analysis.Length; i++)
            {
                //Throwing an exception if cancellation was requested.
                cancellationToken.ThrowIfCancellationRequested();

                EnemyGroup enemyGroup = analysis[i];
                foreach (EnemyGroup veteran in Enemy.Groups)
                {
                    //Throwing an exception if cancellation was requested.
                    cancellationToken.ThrowIfCancellationRequested();


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

            string newGroupsInfo = "";

            for (int i = 0; i < analysis.Length; i++)
            {
                //Throwing an exception if cancellation was requested.
                cancellationToken.ThrowIfCancellationRequested();

                if (!removeAtAnalysis[i])
                {
                    veteranGroups.Add(analysis[i]);
                    newGroupsInfo += analysis[i] + ",";
                }
            }


            return veteranGroups;
        }

        /// <summary>
        ///     Tells you if we need to try and chaise enemies
        /// </summary>
        /// <returns></returns>
        public static bool ShouldWeTryToCatchEnemyShips()
        {
            return Enemy.EnemyIntelligenceSuspicionCounter <= Magic.NumberOfTimesTillWeLearn;
        }

        /// <summary>
        ///     Normal analysis of enemy groups, without considering the previous configurations
        ///     Note that using this method alone will break the heading mechanism because this method
        ///     technically return new groups each time (although they might be the same config)
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A list of enemy groups</returns>
        private static List<EnemyGroup> AnalyzeFull(CancellationToken cancellationToken)
        {
            List<EnemyGroup> updatedGroups = new List<EnemyGroup>();
            IEnumerable<Pirate> enemyAlivePirates = Bot.Game.AllEnemyPirates().Where(p => !p.IsLost);

            //iterate over all the alive pirate of the enemy
            foreach (Pirate pete in enemyAlivePirates)
            {
                //Throwing an exception if cancellation was requested.
                cancellationToken.ThrowIfCancellationRequested();

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
                        //Throwing an exception if cancellation was requested.
                        cancellationToken.ThrowIfCancellationRequested();

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
        ///     Does every turn updating
        ///     Should be called every turn
        /// </summary>
        /// <param name="cancellationToken"></param>
        public static void Update(CancellationToken cancellationToken)
        {
                //update the enemy data
                List<EnemyGroup> updated = Enemy.AnalyzeEnemyGroups(cancellationToken);

                //update the enemyGroups by logical stuff
                Enemy.Groups = Enemy.Groups.Intersect(updated).ToList();
                Enemy.Groups = Enemy.Groups.Union(updated).ToList();

                //update heading in all groups
                Enemy.Groups.ForEach(eGroup => eGroup.Update());

                Enemy.Debug();
        }

        private static void Debug()
        {
            Logger.Write("------------ENEMY GROUPS-----------------");
            foreach (EnemyGroup eg in Enemy.Groups)
            {
                Logger.Write(eg.ToString());
            }
        }
    }
}