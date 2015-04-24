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
    internal abstract class SimulatedEvent : PriorityQueueNode
    {
        /// <summary>
        ///     This virtual method should activate the event
        /// </summary>
        public abstract void Activate(SimulatedGame sg);
    }
}