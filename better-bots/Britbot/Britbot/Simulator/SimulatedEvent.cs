#region #Usings

using Britbot.PriorityQueue;

#endregion

namespace Britbot.Simulator
{
    /// <summary>
    ///     A virtual class representing an event in the game (capture, kill, est...)
    ///     Inherits from PriorityQueueNode because the events are ordered by their time of execution
    /// </summary>
    internal abstract class SimulatedEvent : PriorityQueueNode
    {
        #region Fields & Properies

        /// <summary>
        ///     The turn where the event should activate
        /// </summary>
        public int Turn;

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     Creates a new instance of the class
        /// </summary>
        /// <param name="turn"></param>
        protected SimulatedEvent(int turn)
        {
            this.Turn = turn;
        }

        #endregion

        /// <summary>
        ///     This virtual method should activate the event
        ///     returns true only if simulation is finished
        /// </summary>
        public abstract bool Activate(SimulatedGame sg);
    }
}