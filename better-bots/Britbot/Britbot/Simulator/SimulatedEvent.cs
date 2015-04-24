#region #Usings

using System;
using Britbot.PriorityQueue;

#endregion

namespace Britbot.Simulator
{
    /// <summary>
    ///     A virtual class representing an event in the game (capture, kill, est...)
    ///     Inherits from PriorityQueueNode because the events are ordered by their time of execution
    /// </summary>
    abstract class SimulatedEvent : PriorityQueueNode
    {
        /// <summary>
        /// The turn where the event should activate
        /// </summary>
        public int Turn;

        /// <summary>
        ///     This virtual method should activate the event
        ///     returns true only if simulation is finished
        /// </summary>
        public abstract bool Activate(SimulatedGame sg);

        public SimulatedEvent(int turn)
        {
            this.Turn = turn;
        }
    }
}