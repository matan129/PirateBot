using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Britbot.Simulator
{
    /// <summary>
    /// this Event is called when an island is being captured
    /// </summary>
    class CaptureEvent : SimulatedEvent
    {
        /// <summary>
        /// The island being captured
        /// </summary>
        SimulatedIsland Island;

        /// <summary>
        /// The group Capturing
        /// </summary>
        SimulatedGroup Capturer;


        /// <summary>
        /// simple C'tor
        /// </summary>
        /// <param name="island"></param>
        /// <param name="capturer"></param>
        public CaptureEvent(SimulatedIsland island, SimulatedGroup capturer)
        {
            this.Island = island;
            this.Capturer = capturer;
        }


        /// <summary>
        /// updates the island if event is actual
        /// </summary>
        /// <param name="sg"></param>
        public override void MakeShitHappen(SimulatedGame sg)
        {
            //check if this event is still actuall
            if (!Capturer.IsAlive)
                return;

            if (this.Island.CapturingGroup != this.Capturer)
                return;

            //if everything checks out update island
            this.Island.Owner = this.Capturer.Owner;
        }
    }
}
