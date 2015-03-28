using System;
using System.Collections.Generic;
using System.Threading;

namespace Britbot
{
    /// <summary>
    /// Represents a structure of few of our pirates that have a common goal
    /// </summary>
    public class Group
    {
        #region Members
        //Direction of the group to make navigation more precise
        private HeadingVector heading;

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
        public ITarget Target { get; set; }

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
            throw new NotImplementedException();
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