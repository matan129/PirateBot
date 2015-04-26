using Pirates;

namespace Britbot.Simulator
{
    /// <summary>
    ///     this event is called when a group (surely) arrives to an island
    /// </summary>
    internal class GroupArrivalEvent : SimulatedEvent
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
        public GroupArrivalEvent(int turn, SimulatedIsland island, SimulatedGroup group) : base(turn)
        {
            this.Island = island;
            this.ArrivingGroup = group;
        }

        #endregion

        /// <summary>
        ///     this updates the game as if ArrivingGroup arrived (considering forces and stuff)
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
                    else if (this.Island.CapturingGroup.ActualFirePower() == this.ArrivingGroup.FirePower)
                    {
                        this.ArrivingGroup.Kill(sg);
                        this.Island.CapturingGroup.Kill(sg);
                        return false;
                    }
                    else
                    {
                        this.Island.CapturingGroup.Kill(sg);
                        this.Island.TurnsBeingCaptured = 0;
                    }
                }
            }

            this.Island.CapturingGroup = this.ArrivingGroup;

            if (this.Island.Owner == this.ArrivingGroup.Owner)
                return false;
            else if (this.Island.Owner == Consts.NO_OWNER)
                sg.AddEvent(new CaptureEvent(sg.CurrentTurn + 20- this.Island.TurnsBeingCaptured, this.Island, this.ArrivingGroup));
            else
                sg.AddEvent(new DeCaptureEvent(sg.CurrentTurn + 20 - this.Island.TurnsBeingCaptured, this.Island, this.ArrivingGroup));

            return false;
        }
    }
}