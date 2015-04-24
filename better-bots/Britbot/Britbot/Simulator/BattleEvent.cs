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

        public BattleEvent(SimulatedGroup g1, SimulatedGroup g2)
        {
            this.GroupA = g1;
            this.GroupB = g2;
        }

        #endregion

        public override void Activate(SimulatedGame sg)
        {
            //check fire power
            if (this.GroupA.ActualFirePower(sg) < this.GroupB.ActualFirePower(sg))
            {
                this.GroupA.Kill(sg.CurrentTurn);
            }
            else if (this.GroupA.ActualFirePower(sg) == this.GroupB.ActualFirePower(sg))
            {
                this.GroupA.Kill(sg.CurrentTurn);
                this.GroupB.Kill(sg.CurrentTurn);
            }
            else
            {
                this.GroupB.Kill(sg.CurrentTurn);
            }
        }
    }
}