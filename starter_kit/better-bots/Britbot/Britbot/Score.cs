namespace Britbot
{
    /// <summary>
    /// A class represent information about a target's score in relation to a specific group
    /// </summary>
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
        public int Value { get; private set; }

        /// <summary>
        /// Time until completion
        /// </summary>
        public int Eta;

        /// <summary>
        /// Holds the target score relative to the attacker
        /// </summary>
        /// <param name="target">The target this score relates to</param>
        /// <param name="type">The type of the target (island or enemy group)</param>
        /// <param name="value">The Numerical value of the target</param>
        /// <param name="eta">Estimated time to arrive at target</param>
        public Score(ITarget target, TargetType type, int value, int eta)
        {
            this.Target = target;
            this.Type = type;
            this.Value = value;
            this.Eta = eta;
        }
    }

    /// <summary>
    /// An enum represent the type of the target of a group
    /// </summary>
    public enum TargetType
    {
        Island,
        EnemyGroup
    }
}