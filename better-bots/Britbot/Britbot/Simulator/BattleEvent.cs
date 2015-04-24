namespace Britbot.Simulator
{
    /// <summary>
    ///     this represents a battle between two ships
    /// </summary>
    internal class BattleEvent : SimulatedEvent
    {
        #region Fields & Properies

        //first group
        private SimulatedGroup GroupA;
        //second group
        private SimulatedGroup GroupB;

        #endregion

        #region Constructors & Initializers

        public BattleEvent(int turn, SimulatedGroup g1, SimulatedGroup g2) : base(turn)
        {
            this.GroupA = g1;
            this.GroupB = g2;
        }

        #endregion

        public override bool Activate(SimulatedGame sg)
        {            
            //check if the groups are oposing
            if (this.GroupA.Owner != this.GroupB.Owner)
            {
                //check fire power
                if (this.GroupA.ActualFirePower(sg) < this.GroupB.ActualFirePower(sg))
                {
                    this.GroupA.Kill(sg);
                }
                else if (this.GroupA.ActualFirePower(sg) == this.GroupB.ActualFirePower(sg))
                {
                    this.GroupA.Kill(sg);
                    this.GroupB.Kill(sg);
                }
                else
                {
                    this.GroupB.Kill(sg);
                }
            }

            return false;
        }
    }
}