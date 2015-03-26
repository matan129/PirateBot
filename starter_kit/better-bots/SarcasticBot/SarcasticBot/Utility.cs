using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;

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
            targets.AddRange(SmartIsland.islands);
            targets.AddRange(Enemy.Groups);
            targets.AddRange(Commander.Groups);
            return targets;
        }

        /// <summary>
        /// Adds a location a direction
        /// </summary>
        /// <param name="loc">The location</param>
        /// <param name="d">The Direction</param>
        /// <returns>A new location based on the direction</returns>
        public static Location AddLoc(Location loc, Direction d)
        {
            switch (d)
            {
                    case Direction.EAST:
                    loc.Col++;
                    break;
                    case Direction.WEST:
                    loc.Col--;
                    break;
                    case Direction.NORTH:
                    loc.Row++;
                    break;
                    case Direction.SOUTH:
                    loc.Row--;
                    break;
            }

            return loc;
        }
    }
}
