using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;
namespace Britbot.Simulator
{
    class SimulatedIsland
    {
        public int Id { get; set; }

        public int Value { get; set; }

        public int Owner;

        public int TurnsBeingCaptured;

        public SimulatedGroup CapturingGroup;

        /// <summary>
        /// calculates how much time it would take for the given side to capture the island
        /// </summary>
        /// <param name="capturer">The side (me or Enemy)</param>
        /// <returns>number of turns till capture</returns>
        public int TurnsTillCapture(int capturer)
        {
            //calculate total number of turns to capture (without partials)
            int totalCaptureTime;
            //check if conqueror is the owner
            if (this.Owner == capturer)
            {
                totalCaptureTime = 0;
            } //if the isalnd is nutral
            else if (this.Owner == Consts.NO_OWNER)
            {
                totalCaptureTime = this.TurnsBeingCaptured;
            } //if the island is of the other team
            else
            {
                totalCaptureTime = 2 * this.TurnsBeingCaptured;
            }

            //check if we already have some capture time
            if (this.CapturingGroup.Owner == capturer)
            {
                return totalCaptureTime - this.TurnsBeingCaptured;
            }
            return totalCaptureTime;
        }

        /// <summary>
        /// Turns till the island becomes nutral
        /// </summary>
        /// <param name="capturer"></param>
        /// <returns></returns>
        public int TurnsTillDecapture(int capturer)
        {
            //calculate total number of turns to capture (without partials)
            int totalCaptureTime;
            //check if conqueror is the owner
            if (this.Owner == capturer)
            {
                totalCaptureTime = 0;
            } //if the isalnd is nutral
            else if (this.Owner == Consts.NO_OWNER)
            {
                totalCaptureTime = 0;
            } //if the island is of the other team
            else
            {
                totalCaptureTime = this.TurnsBeingCaptured;
            }

            //check if we already have some capture time
            if (this.CapturingGroup.Owner == capturer)
            {
                return totalCaptureTime - this.TurnsBeingCaptured;
            }
            return totalCaptureTime;
        }

        /// <summary>
        /// This safely kills the local group and updates the turns count
        /// </summary>
        /// <param name="currTurn">the current turn</param>
        public void KillLocals(int currTurn)
        {
            if (this.CapturingGroup != null)
            {
                this.CapturingGroup.Kill(currTurn);
            }
            //update turn counter
            this.TurnsBeingCaptured = 0;
        }

        /// <summary>
        /// simply adds to the turn counter
        /// </summary>
        /// <param name="turnDiff"></param>
        public void Update(int turnDiff)
        {
            this.TurnsBeingCaptured += turnDiff;
        }
    }
}
