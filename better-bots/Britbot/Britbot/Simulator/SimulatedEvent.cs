using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Britbot.PriorityQueue;

namespace Britbot.Simulator 
{
    /// <summary>
    /// A virtual class representing an event in the game (capture, kill, est...)
    /// Inherits from PriorityQueueNode because the events are ordered by their time of execution
    /// </summary>
    class SimulatedEvent : PriorityQueueNode
    {
        /// <summary>
        /// This virtual method should activate the event
        /// </summary>
        public virtual void MakeShitHappen(SimulatedGame sg)
        {
            throw new NotImplementedException();
        }

    }
}
