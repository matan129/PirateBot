using Pirates;

namespace Britbot
{
    /// <summary>
    /// Represent a target we can, well, target.
    /// </summary>
    public interface ITarget
    {
        /// <summary>
        /// Return a score for this target
        /// </summary>
        /// <param name="origin">The group requesting the score</param>
        /// <returns>A Score instance for the target</returns>
        Score GetScore(Group origin);

        /// <summary>
        /// Gets the location of the target
        /// </summary>
        /// <returns>Returns the location of the target</returns>
        Location GetLocation();

        /// <summary>
        /// Given a group, this should give the group the right direction to move
        /// in order to reach the target
        /// </summary>
        /// <param name="origin">The group requesting directions</param>
        /// <returns></returns>
        Direction GetDirection(Group origin);

        /// <summary>
        /// Tests if two targets are the same
        /// </summary>
        /// <param name="operandB">The other target to test</param>
        /// <returns>true if identical, false either</returns>
        bool Equals(ITarget operandB);

        /// <summary>
        /// Gets a string description forthis Target for log purposes 
        /// </summary>
        /// <returns></returns>
        string GetDescription();
    }
}