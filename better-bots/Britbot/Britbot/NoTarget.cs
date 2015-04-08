#region Usings

using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     This class represents a null target wich does nothing
    ///     this will help us prevent cases of dimentions error due to
    ///     null priority lists
    /// </summary>
    internal class NoTarget : ITarget
    {
        #region Interface Implementations

        /// <summary>
        ///     fulfills the interface, just returns zeroish score
        /// </summary>
        /// <param name="origin">the group for which the score is calculated</param>
        /// <returns></returns>
        public Score GetScore(Group origin)
        {
            return new Score(this, TargetType.NoTarget, 0, 0, 1);
        }

        /// <summary>
        ///     returns the origin because what else
        /// </summary>
        /// <returns>returns the origin</returns>
        public Location GetLocation()
        {
            return new Location(0, 0);
        }

        /// <summary>
        ///     just returns no direction
        /// </summary>
        /// <param name="origin">The group requesting directions</param>
        /// <returns>nothing</returns>
        public Direction GetDirection(Group origin)
        {
            return Direction.NOTHING;
        }

        /// <summary>
        ///     Tests if two targets are the same
        ///     returns true if the other target is a no target
        ///     false otherwise
        /// </summary>
        /// <param name="operandB">The other target to test</param>
        /// <returns>true if operandB is NoTarget, false either</returns>
        public bool Equals(ITarget operandB)
        {
            if (operandB is NoTarget)
                return true;
            return false;
        }

        public TargetType GetTargetType()
        {
            return TargetType.NoTarget;
        }

        /// <summary>
        ///     Gets a string description forthis Target for log purposes
        /// </summary>
        /// <returns></returns>
        public string GetDescription()
        {
            return "NoTarget. Nothing interesting here";
        }

        #endregion
    }
}