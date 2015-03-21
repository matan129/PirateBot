using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SarcasticBot
{
    public class Group : ITarget
    {
        private System.Collections.Generic.List<SmartPirate> Pirates;
        private ITarget Target;
        private System.Collections.Generic.Dictionary<SarcasticBot.ITarget, ScoreStruct> Priorities;
        private Path Trajectory;
    
        public Group(int size, int index)
        {
            throw new System.NotImplementedException();
        }

        private void Move()
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.Dictionary<SarcasticBot.ITarget, int> CalculatePriorities()
        {
            throw new System.NotImplementedException();
        }

        public ScoreStruct GetScore(out Group origin, out Path path, bool isFast)
        {
            throw new NotImplementedException();
        }

        public Group Split(int size)
        {
            throw new System.NotImplementedException();
        }

        public Group Split(params SmartPirates[] sPirates)
        {
            throw new System.NotImplementedException();
        }
    }
}
