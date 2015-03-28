using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Britbot
{
    public class Score
    { 
        public Group Origin { get; private set; }
        public int Value { get; private set; }
        public ITarget target;
        /// <summary>
        /// (Holds the target score relative to the attacker)
        /// </summary>
        /// <param name="OriginGroup">The Attacker from the type Group</param>
        /// <param name="ValueofTarget">The Numerical value of the target</param>
        public Score(Group OriginGroup, int ValueofTarget)
        {
           this.Origin = OriginGroup;
           this.Value = ValueofTarget;
        }
    }
}
