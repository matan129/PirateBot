using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Britbot.Simulator
{
    /// <summary>
    /// this terminates the calculation by returning true
    /// </summary>
    class SimulationEndEvent : SimulatedEvent
    {
        public SimulationEndEvent(int turn) : base(turn) { }

        public override bool Activate(SimulatedGame sg)
        {
            return true;
        }
    }
}
