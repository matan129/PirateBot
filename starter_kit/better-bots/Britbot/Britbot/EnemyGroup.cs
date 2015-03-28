using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pirates;

namespace Britbot
{
    /// <summary>
    /// A class which represents an enemy group
    /// </summary>
    public class EnemyGroup : ITarget
    {
        /// <summary>
        /// List of pirate indexes in this group
        /// </summary>
        public List<int> EnemyPirates { get; private set; }

        /// <summary>
        /// The direction this group's heading to 
        /// </summary>
        public HeadingVector Heading { get; private set; }
    
        /// <summary>
        /// Gets the score for this group
        /// </summary>
        /// <param name="origin">The group requesting the evaluation</param>
        /// <returns>The score for this group</returns>
        public Score GetScore(Group origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the average location for this group
        /// </summary>
        /// <returns>Returns the average location for this group</returns>
        public Location GetLocation()
        {
            int totalRow = 0, totalCol = 0;
            
        }

        /// <summary>
        /// Determines if an enemy pirate belongs to this enemy group.
        /// </summary>
        /// <param name="enemyPirate">The index of the enemy pirate</param>
        /// <returns>True if the pirate belongs to the group or false, otherwise</returns>
        public bool IsInGroup(int enemyPirate)
        {
            Pirate ePirate = Bot.Game.GetEnemyPirate(enemyPirate);
            return
                this.EnemyPirates.ConvertAll(e => Bot.Game.GetEnemyPirate(e))
                    .Select(ep => Bot.Game.Distance(ep, ePirate))
                    .Concat(new int[] {})
                    .Min() <= 2;
        }
    }
}
