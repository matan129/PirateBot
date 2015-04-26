#region #Usings

using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     Represent a target we can, well, target.
    /// </summary>
    public interface ITarget
    {
        /// <summary>
        ///     Return a score for this target
        /// </summary>
        /// <param name="origin">The group requesting the score</param>
        /// <returns>A Score instance for the target</returns>
        Score GetScore(Group origin);

        /// <summary>
        ///     Gets the location of the target
        /// </summary>
        /// <returns>Returns the location of the target</returns>
        Location GetLocation();

        /// <summary>
        ///     Given a group, this should give the group the right direction to move
        ///     in order to reach the target
        /// </summary>
        /// <param name="origin">The group requesting directions</param>
        /// <returns></returns>
        Direction GetDirection(Group origin);

        /// <summary>
        ///     Tests if two targets are the same
        /// </summary>
        /// <param name="operandB">The other target to test</param>
        /// <returns>true if identical, false either</returns>
        bool Equals(ITarget operandB);

        /// <summary>
        ///     Get the type of the target
        /// </summary>
        /// <returns></returns>
        TargetType GetTargetType();

        /// <summary>
        ///     Gets a string description for this Target for log purposes
        /// </summary>
        /// <returns></returns>
        string GetDescription();

        /// <summary>
        ///     this should be called when a group is assign this targets, it will be used
        ///     so the target could tell if it causes the assignment process to get stuck
        ///     and remove itself
        /// </summary>
        void TargetAssignmentEvent();

        /// <summary>
        ///     cana"l
        /// </summary>
        void TargetDessignmentEvent();
    }
}