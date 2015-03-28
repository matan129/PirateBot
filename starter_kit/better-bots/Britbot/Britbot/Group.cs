using System;
using System.Collections.Generic;
using System.Threading;
using Pirates;

namespace Britbot
{
    /// <summary>
    /// Represents a structure of few of our pirates that have a common goal
    /// </summary>
    public class Group
    {
        #region Members
        /// <summary>
        /// Direction of the group to make navigation more precise
        /// </summary>
        public readonly HeadingVector Heading;

        /// <summary>
        /// The Group ID number
        /// useful for debugging
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// List of the indexes of the pirates in this group
        /// </summary>
        public List<int> Pirates { get; private set; }

        /// <summary>
        /// The target of the Group
        /// </summary>
        private ITarget Target;

        /// <summary>
        /// The group's role (i.e. destroyer or attacker)
        /// </summary>
        public GroupRole Role { get; private set; }

        /// <summary>
        /// List of priorities for this group
        /// </summary>
        public List<Score> Priorities { get; private set; }

        /// <summary>
        /// A thread for complex calculations that can be ran in parallel to other stuff
        /// </summary>
        public Thread CalcThread { get; private set; }
        #endregion

        /// <summary>
        /// Sets the target of the group, while doing so also resets the heading vector
        /// if there is need (meaning if we didn't choose the same target again).
        /// </summary>
        /// <param name="target">the new target</param>
        public void SetTarget(ITarget target)
        {
            //if it isn't the same target as before update and reset heading
            if (Target != target)
            {
                Target = target;
                Heading.SetCoordinates(0,0);
            }
        }

        /// <summary>
        /// Returns the average location for this group
        /// </summary>
        /// <returns>Returns the average location for this group</returns>
        public Location GetLocation()
        {
            //assigning X and Y to hold the sum
            int X = 0;
            int Y = 0;

            foreach (int pirate in Pirates)
            {
                X += Bot.Game.GetEnemyPirate(pirate).Loc.Col;
                Y += Bot.Game.GetEnemyPirate(pirate).Loc.Row;
            }

            return new Location(Y / Pirates.Count, X / Pirates.Count);
        }

        /// <summary>
        /// Creates a new group
        /// </summary>
        /// <param name="id">The ID of the group</param>
        public Group(int id)
        {
            //Maybe use static counter to determine the ID by the constructor?
            this.Id = id;

            throw new NotImplementedException();
        }


        /// <summary>
        /// Move the pirates!
        /// </summary>
        public void Move()
        {
            //first move the first
            Bot.Game.SetSail(Bot.Game.MyPirates()[Pirates[0]],Target.GetDirection(this));

            //if there are others, move them after him
            if (Pirates.Count > 1)
            {
                for (int i = 1; i < Pirates.Count; i++)
                {
                    //sail every other ship after the first
                    //----REMARK: THIS LINE IS TERRIBLE, WE NEED TO THINK OF A WAY TO MAKE IT SIMPLER
                    Bot.Game.SetSail(Bot.Game.MyPirates()[Pirates[i]],
                                     Bot.Game.GetDirections(Bot.Game.MyPirates()[Pirates[i]],Bot.Game.MyPirates()[Pirates[0]])[0]);
                }
            }
        }

        /// <summary>
        /// Calculate target priorities for this group
        /// </summary>
        public void CalcPriorities()
        {
            throw new NotImplementedException();
        }
    }
}