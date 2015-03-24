using System;
using System.Collections.Generic;

namespace SarcasticBot
{
    public class EnemyGroup : ITarget
    {
        private List<EnemyPirate> EnemyPirates;
        private Dictionary<ITarget, ScoreStruct> PossibleTargets;

        public EnemyGroup()
        {
            throw new NotImplementedException();
        }

        public int Size
        {
            get { throw new NotImplementedException(); }
            set { }
        }

        public Dictionary<ITarget, ScoreStruct> CalculateTargets()
        {
            throw new NotImplementedException();
        }

        public int GetScore(out Group origin, out Path path, bool isFast)
        {
            throw new NotImplementedException();
        }
    }
}