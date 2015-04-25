#region Usings

using Pirates;

#endregion

namespace Britbot.Simulator
{
    class SimulatedIsland
    {
        #region Fields & Properies

        public SimulatedGroup CapturingGroup;
        public int Owner;
        public int TurnsBeingCaptured;
        public int Id { get; set; }
        public int Value { get; set; }

        //original data
        public int OriginalOwner;
        public int OriginalTrunsBeingCaptured;
        public SimulatedGroup OriginalCapturingGroup;

        #endregion

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="turnsBeingCaptured"></param>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="capturingGroup"></param>
        public SimulatedIsland(int owner, int turnsBeingCaptured,int id,int value, SimulatedGroup capturingGroup = null)
        {
            this.Owner = owner;
            this.TurnsBeingCaptured = turnsBeingCaptured;
            this.Id = id;
            this.Value = value;
            this.CapturingGroup = capturingGroup;

            //set original

            this.OriginalCapturingGroup = capturingGroup;
            this.OriginalOwner = owner;
            this.OriginalTrunsBeingCaptured = turnsBeingCaptured;
        }

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
        public void KillLocals(SimulatedGame sg)
        {
            if (this.CapturingGroup != null)
            {
                this.CapturingGroup.Kill(sg);

                //update turn counter
                this.TurnsBeingCaptured = 0;
            }
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
