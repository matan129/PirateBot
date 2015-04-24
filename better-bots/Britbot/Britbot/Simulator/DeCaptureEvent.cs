using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;
namespace Britbot.Simulator
{
    /// <summary>
    /// This event is called when an island is switching side to nutral
    /// </summary>
    class DeCaptureEvent : SimulatedEvent
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
        public DeCaptureEvent(SimulatedIsland island, SimulatedGroup capturer)
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

            //check that the island realy of the enemy
            if (this.Island.Owner == this.Capturer.Owner)
                return;

            //if everything checks out update island
            this.Island.Owner = Consts.NO_OWNER;
            this.Island.TurnsBeingCaptured = 0;

            //set out a capture event
            int captureTurn = sg.CurrentTurn + this.Island.TurnsTillCapture(this.Capturer.Owner);
            sg.AddEvent(new CaptureEvent(this.Island, this.Capturer), captureTurn);
        }
    }
}
