namespace Britbot.Simulator
{
    /// <summary>
    ///     this Event is called when an island is being captured
    /// </summary>
    internal class CaptureEvent : SimulatedEvent
    {
        #region Fields & Properies

        /// <summary>
        ///     The group Capturing
        /// </summary>
        private SimulatedGroup Capturer;

        /// <summary>
        ///     The island being captured
        /// </summary>
        private SimulatedIsland Island;

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     simple C'tor
        /// </summary>
        /// <param name="island"></param>
        /// <param name="capturer"></param>
        public CaptureEvent(SimulatedIsland island, SimulatedGroup capturer)
        {
            this.Island = island;
            this.Capturer = capturer;
        }

        #endregion

        /// <summary>
        ///     updates the island if event is actual
        /// </summary>
        /// <param name="sg"></param>
        public override void Activate(SimulatedGame sg)
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