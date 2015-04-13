#region #Usings

using System;

#endregion

namespace Britbot
{
    /// <summary>
    ///     A class represent information about a target's score in relation to a specific group
    /// </summary>
    public class Score : IComparable, IComparable<Score>
    {
        #region Fields & Properies

        /// <summary>
        ///     Amount of enemy ships nearBy
        /// </summary>
        public double EnemyShips;

        /// <summary>
        ///     Time until completion
        /// </summary>
        public double Eta;

        /// <summary>
        ///     The target being scored
        /// </summary>
        public ITarget Target;

        /// <summary>
        ///     target's type
        /// </summary>
        public TargetType Type;

        /// <summary>
        ///     value of the island if island
        /// </summary>
        public double Value { get; private set; }

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     Holds the target score relative to the attacker
        /// </summary>
        /// <param name="target">The target this score relates to</param>
        /// <param name="type">The type of the target (island or enemy group)</param>
        /// <param name="value">The Numerical value of the island</param>
        /// <param name="EnemyShips">amount of enemy ships nearby</param>
        /// <param name="eta">Estimated time to arrive at target</param>
        public Score(ITarget target, TargetType type, double value, double EnemyShips, double eta)
        {
            this.Target = target;
            this.Type = type;
            this.Value = value;
            this.Eta = eta;
            this.EnemyShips = EnemyShips;
        }

        #endregion

        #region Interface Implementations

        /// <summary>
        ///     Used to compare two score elements
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            Score score = obj as Score;
            if (score != null)
            {
                return this.CompareTo(score);
            }

            throw new ArgumentException("Object must a a Score in order to compare it with another score object");
        }

        /// <summary>
        ///     Used to compare two score elements
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Score other)
        {
            return (this.Value * 10 - this.Eta).CompareTo((other.Value * 10 - other.Eta));
        }

        #endregion

        public override string ToString()
        {
            return "Target: " + this.Target + " value: " + Value + " Enemy: " + EnemyShips + " ETA: " + Eta;
        }
    }

    /// <summary>
    ///     An enum represent the type of the target of a group
    /// </summary>
    public enum TargetType
    {
        Island,
        NoTarget,
        EnemyGroup
    }
}