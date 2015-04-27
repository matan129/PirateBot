using Pirates;

namespace Britbot.Simulator
{
    /// <summary>
    ///     This represents an arrivale depending on something
    ///     for example: an enemy group might only come to their island if we begin to capture it
    /// </summary>
    internal class PossibleArrivalEvent : SimulatedEvent
    {
        #region Fields & Properies

        /// <summary>
        ///     the Group arriving to the island
        /// </summary>
        private SimulatedGroup ArrivingGroup;

        /// <summary>
        ///     The island the groups arrives at
        /// </summary>
        private SimulatedIsland Island;

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     C'tor...
        /// </summary>
        /// <param name="island"></param>
        /// <param name="group"></param>
        public PossibleArrivalEvent(int turn, SimulatedIsland island, SimulatedGroup group) : base(turn)
        {
            this.Island = island;
            this.ArrivingGroup = group;
        }

        #endregion

        /// <summary>
        ///     this checks if the arriving group should have arrived, only then updates stuff
        /// </summary>
        /// <param name="sg"></param>
        public override bool Activate(SimulatedGame sg)
        {
            if (this.ArrivingGroup.IsBusy)
                return false;

            this.ArrivingGroup.IsBusy = true;

            if (this.ArrivingGroup == null)
                return false;

            if (!this.ArrivingGroup.IsAlive)
                return false;

            if (this.Island.CapturingGroup != null)
            {
                if (this.Island.CapturingGroup.Owner == this.ArrivingGroup.Owner)
                    return false;

                //opponenets
                if (this.Island.CapturingGroup.IsAlive)
                {
                    if (this.Island.CapturingGroup.ActualFirePower() > this.ArrivingGroup.FirePower)
                    {
                        this.ArrivingGroup.Kill(sg);
                        return false;
                    }
                    if (this.Island.CapturingGroup.ActualFirePower() == this.ArrivingGroup.FirePower)
                    {
                        this.ArrivingGroup.Kill(sg);
                        this.Island.CapturingGroup.Kill(sg);
                        return false;
                    }
                    this.Island.CapturingGroup.Kill(sg);
                    this.Island.TurnsBeingCaptured = 0;
                }
            }

            this.Island.CapturingGroup = this.ArrivingGroup;

            if (this.Island.Owner == this.ArrivingGroup.Owner)
                return false;
            if (this.Island.Owner == Consts.NO_OWNER)
                sg.AddEvent(new CaptureEvent(sg.CurrentTurn + 20 - this.Island.TurnsBeingCaptured, this.Island,
                    this.ArrivingGroup));
            else
                sg.AddEvent(new DeCaptureEvent(sg.CurrentTurn + 20 - this.Island.TurnsBeingCaptured, this.Island,
                    this.ArrivingGroup));

            return false;
        }
    }
}