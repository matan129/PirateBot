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
        public GroupArrivalEvent(SimulatedIsland island, SimulatedGroup group)
        {
            this.Island = island;
            this.ArrivingGroup = group;
        }

        #endregion

        /// <summary>
        ///     this updates the game as if ArrivingGroup arrived (considering forces and stuff)
        /// </summary>
        /// <param name="sg"></param>
        public override void Activate(SimulatedGame sg)
        {
            //check if the arriving group is alive
            if (this.ArrivingGroup.IsAlive)
            {
                //check if the island has any forces
                if (this.Island.CapturingGroup != null)
                {
                    //check if the local gorup is the enemy and they are alive
                    if ((this.Island.CapturingGroup.Owner != this.ArrivingGroup.Owner) &&
                        this.Island.CapturingGroup.IsAlive)
                    {
                        //confront with local forces
                        if (this.Island.CapturingGroup.ActualFirePower(sg) > this.ArrivingGroup.FirePower)
                        {
                            //if the local force is stronger then the arriving group dies
                            this.ArrivingGroup.Kill(sg.CurrentTurn);
                            return;
                        }
                        if (this.Island.CapturingGroup.ActualFirePower(sg) == this.ArrivingGroup.FirePower)
                        {
                            //if the forces are equal, kill them both
                            this.ArrivingGroup.Kill(sg.CurrentTurn);
                            this.Island.CapturingGroup.Kill(sg.CurrentTurn);
                            return;
                        }
                    }
                }

                //the arriving group has enought force to take over the island
                //first, kill locals, if there are any
                this.Island.KillLocals(sg.CurrentTurn);
                //then take over
                this.Island.CapturingGroup = this.ArrivingGroup;

                int captureTurn = sg.CurrentTurn + this.Island.TurnsTillDecapture(this.ArrivingGroup.Owner);

                //then set a decapture event
                sg.AddEvent(new DeCaptureEvent(this.Island, this.ArrivingGroup), captureTurn);
            }
        }
    }
}