namespace Britbot.Simulator
{
    /// <summary>
    ///     this represents a battle between two ships
    /// </summary>
    internal class BattleEvent : SimulatedEvent
    {
        #region Fields & Properies

        //first group
        private SimulatedGroup _groupA;
        //second group
        private SimulatedGroup _groupB;

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     Creates a new instance of BattleEvent
        /// </summary>
        /// <param name="turn"></param>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        public BattleEvent(int turn, SimulatedGroup g1, SimulatedGroup g2) 
            : base(turn)
        {
            this._groupA = g1;
            this._groupB = g2;
        }

        #endregion

        /// <summary>
        ///     Activates the event?
        /// </summary>
        /// <param name="sg"></param>
        /// <returns></returns>
        public override bool Activate(SimulatedGame sg)
        {
            //check if the groups are oposing
            if (this._groupA.Owner != this._groupB.Owner)
            {
                //check fire power
                if (this._groupA.ActualFirePower() < this._groupB.ActualFirePower())
                {
                    this._groupA.Kill(sg);
                }
                else if (this._groupA.ActualFirePower() == this._groupB.ActualFirePower())
                {
                    this._groupA.Kill(sg);
                    this._groupB.Kill(sg);
                }
                else
                {
                    this._groupB.Kill(sg);
                }
            }

            return false;
        }
    }
}