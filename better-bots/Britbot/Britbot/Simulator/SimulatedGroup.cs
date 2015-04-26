using System;
using System.Collections.Generic;
using Pirates;

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

        public bool IsCapturing;

        //originals
        public bool OriginalIsAlive;
        public int OriginalReviveTurn;
        public bool OriginalIsCapturing;
        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     just a constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="owner"></param>
        /// <param name="firePower"></param>
        public SimulatedGroup(int id, int owner, double firePower,bool isCapturing)
        {
            //just set stuff
            this.Id = id;
            this.Owner = owner;
            this.FirePower = firePower;
            this.IsCapturing = isCapturing;

            this.IsAlive = true;
            this.ReviveTurn = -1;

            //set originals
            this.OriginalIsAlive = IsAlive;
            this.OriginalIsCapturing = isCapturing;
            this.OriginalReviveTurn = -1;
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

        public bool IsBusy = false;
        
        public int ActualFirePower()
        {
            if (this.IsCapturing)
                return (int) this.FirePower - 1;
            return (int) this.FirePower;
        }

        /// <summary>
        ///     kills the group
        /// </summary>
        /// <param name="currTurn"></param>
        public void Kill(SimulatedGame sg)
        {
            this.IsAlive = false;
            this.ReviveTurn = sg.CurrentTurn + Bot.Game.GetSpawnTurns();

            if (this.Owner == Consts.ME)
                sg.MyDeadPirates += (int) this.FirePower;
            else
                sg.EnemyDeadPirates += (int) this.FirePower;

            sg.AddEvent(new ReviveEvent(sg.CurrentTurn + Bot.Game.GetSpawnTurns(), this));
        }

        /// <summary>
        ///     Checks if two groups are identical
        /// </summary>
        /// <param name="sg1">first group</param>
        /// <param name="sg2">second group</param>
        /// <returns>true if the two groups are identical and false if not</returns>
        public static bool operator ==(SimulatedGroup sg1, SimulatedGroup sg2)
        {
            if (ReferenceEquals(sg1, null) && ReferenceEquals(sg2, null))
                return true; //both null

            return !ReferenceEquals(sg1,null) && !ReferenceEquals(sg2,null) && sg1.Id == sg2.Id;
        }

        /// <summary>
        ///     Determines if two group are not the same
        /// </summary>
        /// <param name="sg1">first group</param>
        /// <param name="sg2">scond group</param>
        /// <returns>true if the two groups are not identical and false if they are</returns>
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