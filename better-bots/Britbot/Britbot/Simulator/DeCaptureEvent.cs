#region #Usings

using Pirates;

#endregion

namespace Britbot.Simulator
{
    /// <summary>
    ///     This event is called when an island is switching side to nutral
    /// </summary>
    internal class DeCaptureEvent : SimulatedEvent
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
        public DeCaptureEvent(int turn, SimulatedIsland island, SimulatedGroup capturer) : base(turn)
        {
            this.Island = island;
            this.Capturer = capturer;
        }

        #endregion

        /// <summary>
        ///     updates the island if event is actual
        /// </summary>
        /// <param name="sg"></param>
        public override bool Activate(SimulatedGame sg)
        {
            //check if this event is still actuall
            if (!Capturer.IsAlive)
                return false;

            if (this.Island.CapturingGroup != this.Capturer)
                return false;

            //check that the island realy of the enemy
            if (this.Island.Owner == this.Capturer.Owner)
                return false;

            //if everything checks out update island
            
            if (this.Island.Owner == Consts.ME)
                sg.MyIslandCount -= this.Island.Value;
            if (this.Island.Owner == Consts.ENEMY)
                sg.EnemyIslandCount -= this.Island.Value;
                        
            this.Island.Owner = Consts.NO_OWNER;
            this.Island.TurnsBeingCaptured = 0;

            //set out a capture event
            int captureTurn = sg.CurrentTurn + this.Island.TurnsTillCapture(this.Capturer.Owner);
            sg.AddEvent(new CaptureEvent(captureTurn, this.Island, this.Capturer));

            return false;
        }
    }
}