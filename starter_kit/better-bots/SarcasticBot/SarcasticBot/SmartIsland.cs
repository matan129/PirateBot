using System;
using System.Collections.Generic;

namespace SarcasticBot
{
    public class SmartIsland : ITarget
    {
        public static List<SmartIsland> islands; 

        public ScoreStruct GetScore(Group origin, Path path = null, bool isFast = false)
        {
            throw new NotImplementedException();
        }
    }
}