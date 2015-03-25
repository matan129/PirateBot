using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarcasticBot
{
    static class Utility
    {
        /// <summary>
        /// Gets all the ITarget objects in the game
        /// </summary>
        /// <returns>A List of ITargets</returns>
        public static List<ITarget> GetAllTargets()
        {
            List<ITarget> targets = new List<ITarget>();
            targets.AddRange(SmartGame.SmartIslands);
            targets.AddRange(Enemy.Groups);
            targets.AddRange(Commander.Groups);
            return targets;
        }
    }
}
