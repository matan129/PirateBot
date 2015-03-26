using System;
using System.Collections.Generic;
using System.Linq;

namespace SarcasticBot
{
    public class EnemyGroup : ITarget
    {
        public List<EnemyPirate> EnemyPirates;
        private Dictionary<ITarget, ScoreStruct> PossibleTargets;

        /// <summary>
        /// Creates a new EnemyGroup
        /// </summary>
        public EnemyGroup()
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Try to guess the targets of this enemy group.
        /// </summary>
        /// <returns>A dictionary with the possible targets</returns>
        public Dictionary<ITarget, ScoreStruct> CalculateTargets()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the score for the instance of the EnemyGroup class relative to the asking Group.
        /// </summary>
        /// <param name="origin">The group requesting the score</param>
        /// <param name="path">A path to approximate to</param>
        /// <param name="isFast">Use fast approximation methods or not</param>
        /// <returns>A ScoreStruct for this EnemyGroup</returns>
        public ScoreStruct GetScore(Group origin, Path path = null, bool isFast = false)
        {
            throw new NotImplementedException();
        }

        public bool IsInGroup(EnemyPirate test)
        {
            
        }
    }
}