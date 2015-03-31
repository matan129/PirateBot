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

        /// <summary>
        /// Join two groups together and add the to the list of groups
        /// </summary>
        /// <param name="groupA">The first group to join</param>
        /// <param name="groupB">Thr second group to join</param>
        public static Group Join(Group groupA, Group groupB)
        {
            //Add all the pirate in group B to group A, and ignore duplicates (although there shouldn't be)
            Group joined = new Group(groupB.Pirates.Where(p => groupA.Pirates.Contains(p) != true));
            joined.Pirates.AddRange(groupA.Pirates);
            
            //Remove the old groups from the list
            //The remove method uses the Equal object method so I implemented it in the Group class
            Commander.Groups.Remove(groupA);
            Commander.Groups.Remove(groupB);

            return joined;
        }
    }
}
