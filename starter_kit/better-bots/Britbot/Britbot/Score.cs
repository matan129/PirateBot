namespace Britbot
{
    public class Score
    { 
        public Group Origin { get; private set; }
        public int Value { get; private set; }

        /// <summary>
        /// (Holds the target score relative to the attacker)
        /// </summary>
        /// <param name="originGroup">The Attacker from the type Group</param>
        /// <param name="valueofTarget">The Numerical value of the target</param>
        public Score(Group originGroup, int valueofTarget)
        {
           this.Origin = originGroup;
           this.Value = valueofTarget;
        }
    }
}
