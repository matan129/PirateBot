namespace Britbot
{
    public class Score
    {
        /// <summary>
        /// The target being scored
        /// </summary>
        public ITarget Target;
        /// <summary>
        /// target's type
        /// </summary>
        public TargetType Type;
        /// <summary>
        /// value of the island if island, number of ships if ship
        /// </summary>
        public int value;
        /// <summary>
        /// time till completion
        /// </summary>
        public int ETA;

        /// <summary>
        /// (Holds the target score relative to the attacker)
        /// </summary>
        /// <param name="originGroup">The Attacker from the type Group</param>
        /// <param name="valueofTarget">The Numerical value of the target</param>
        public Score(ITarget Target, TargetType Type, int value, int ETA)
        {
            this.Target = Target;
            this.Type = Type;
            this.value = value;
            this.ETA = ETA;
        }

    }

    public enum TargetType
    {
        Island;
        EnemyGroup;
    }
}