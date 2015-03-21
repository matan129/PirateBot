using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SarcasticBot
{
    public class EnemyGroup : ITarget
    {
        private List<EnemyPirate> EnemyPirates;
        private System.Collections.Generic.Dictionary<SarcasticBot.ITarget, ScoreStruct> PossibleTargets;

        public EnemyGroup()
        {
            throw new System.NotImplementedException();
        }

        public int Size
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public System.Collections.Generic.Dictionary<SarcasticBot.ITarget, ScoreStruct> CalculateTargets()
        {
            throw new System.NotImplementedException();
        }

        public int GetScore(out Group origin, out Path path, bool isFast)
        {
            throw new NotImplementedException();
        }
    }
}
