using System;
using System.Collections.Generic;

namespace Britbot
{
    /// <summary>
    /// Just stuff that makes the hard decisions
    /// </summary>
    public static class Commander
    {
        public static List<Group> Groups { get; private set; }

        /// <summary>
        /// Do something!
        /// </summary>
        public static void Play()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Distribute our pirates into groups and re-arrange them at the start of the game
        /// </summary>
        /// <param name="config">The new configuration. i.e. {2,2,2} for three groups of two pirates</param>
        public static void DistributeForces(int[] config)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Assigns targets to each group based on pure magic
        /// </summary>
        public static void AssignTargets()
        {
            throw new NotImplementedException();
        }
    }
}
