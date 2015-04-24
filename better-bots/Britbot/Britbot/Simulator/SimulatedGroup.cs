namespace Britbot.Simulator
{
    /// <summary>
    ///     this represents a group of pirates (either friendly or enemy)
    /// </summary>
    internal class SimulatedGroup
    {
        #region Fields & Properies

        /// <summary>
        ///     The actual firepower of the group
        /// </summary>
        public double FirePower;

        /// <summary>
        ///     The id of the group
        /// </summary>
        public int Id;

        /// <summary>
        ///     True if the enemy group is alive
        /// </summary>
        public bool IsAlive;

        /// <summary>
        ///     The owner of this group (Me or Enemy)
        /// </summary>
        public int Owner;

        /// <summary>
        ///     The turn when the group revives
        /// </summary>
        public int ReviveTurn;

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     just a constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="owner"></param>
        /// <param name="firePower"></param>
        public SimulatedGroup(int id, int owner, double firePower)
        {
            //just set stuff
            this.Id = id;
            this.Owner = owner;
            this.FirePower = firePower;

            this.IsAlive = true;
            this.ReviveTurn = -1;
        }

        /// <summary>
        ///     copy C'tor
        /// </summary>
        /// <param name="sg"></param>
        public SimulatedGroup(SimulatedGroup sg)
        {
            this.Id = sg.Id;
            this.Owner = sg.Owner;
            this.FirePower = sg.FirePower;
            this.IsAlive = sg.IsAlive;
            this.ReviveTurn = sg.ReviveTurn;
        }

        #endregion

        private bool IsCapturing(SimulatedGame sg)
        {
            //going over all the islands to check if we are capturing them
            for (int i = 0; i < sg.Islands.Length; i++)
            {
                if (this == sg.Islands[i].CapturingGroup)
                    return true;
            }
            return true;
        }

        public int ActualFirePower(SimulatedGame simGame)
        {
            if (this.IsCapturing(simGame))
                return (int) this.FirePower - 1;
            return (int) this.FirePower;
        }

        /// <summary>
        ///     kills the group
        /// </summary>
        /// <param name="currTurn"></param>
        public void Kill(int currTurn)
        {
            this.IsAlive = false;
            this.ReviveTurn = currTurn + Bot.Game.GetSpawnTurns();

            //TODO: make a revive event
        }

        public static bool operator ==(SimulatedGroup sg1, SimulatedGroup sg2)
        {
            return sg1 != null && sg2 != null && sg1.Id == sg2.Id;
        }

        public static bool operator !=(SimulatedGroup sg1, SimulatedGroup sg2)
        {
            return !(sg1 == sg2);
        }

        protected bool Equals(SimulatedGroup other)
        {
            return this.Id == other.Id && this.Owner == other.Owner && this.FirePower.Equals(other.FirePower) &&
                   this.IsAlive == other.IsAlive && this.ReviveTurn == other.ReviveTurn;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj))
                return false;
            if (object.ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SimulatedGroup)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.Id;
                hashCode = (hashCode * 397) ^ this.Owner;
                hashCode = (hashCode * 397) ^ this.FirePower.GetHashCode();
                hashCode = (hashCode * 397) ^ this.IsAlive.GetHashCode();
                hashCode = (hashCode * 397) ^ this.ReviveTurn;
                return hashCode;
            }
        }
    }
}