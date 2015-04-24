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
            //check if the arriving group is alive
            if (this.ArrivingGroup.IsAlive)
            {
                //check if the arriving group really should have arrived

                //it would come only if there are enemy forces in the island and they are fewer then them
                //also if they already are on the island they wont come                
                if ((this.Island.CapturingGroup == null) ||
                    (this.Island.CapturingGroup.Owner == this.ArrivingGroup.Owner) ||
                    (this.Island.CapturingGroup.ActualFirePower(sg) >= this.ArrivingGroup.FirePower))
                    
                {
                    return false;
                }


                //the arriving group has enought force to take over the island and it might do it
                //first, kill locals
                this.Island.KillLocals(sg);

                //then take over
                this.Island.CapturingGroup = this.ArrivingGroup;

                int captureTurn = sg.CurrentTurn + this.Island.TurnsTillDecapture(this.ArrivingGroup.Owner);

                //then set a capture and decapture event
                sg.AddEvent(new DeCaptureEvent(captureTurn, this.Island, this.ArrivingGroup));
            }

            return false;
        }
    }
}