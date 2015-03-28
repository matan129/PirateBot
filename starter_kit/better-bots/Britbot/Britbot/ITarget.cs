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
    }
}