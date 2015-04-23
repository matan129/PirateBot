using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Britbot.Simulator
{
    /// <summary>
    /// this represents a group of pirates (either friendly or enemy)
    /// </summary>
    class SimularedGroup
    {
        /// <summary>
        /// The id of the group
        /// </summary>
        private int Id;

        /// <summary>
        /// The owner of this group (Me or Enemy)
        /// </summary>
        int Owner;

        /// <summary>
        /// The actual firepower of the group
        /// </summary>
        int FirePower;

        /// <summary>
        /// True if the enemy group is alive
        /// </summary>
        bool IsAlive;

        /// <summary>
        /// The turn when the group revives
        /// </summary>
        int ReviveTurn;


        /// <summary>
        /// just a constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="owner"></param>
        /// <param name="firePower"></param>
        public SimularedGroup(int id, int owner, int firePower )
        {
            //just set stuff
            this.Id = id;
            this.Owner = owner;
            this.FirePower = firePower;

            this.IsAlive = true;
            this.ReviveTurn = -1;

        }

        /// <summary>
        /// copy C'tor
        /// </summary>
        /// <param name="sg"></param>
        public SimularedGroup(SimularedGroup sg)
        {
            this.Id = sg.Id;
            this.Owner = sg.Owner;
            this.FirePower = sg.FirePower;
            this.IsAlive = sg.IsAlive;
            this.ReviveTurn = sg.ReviveTurn;
        }

        /// <summary>
        /// kills the group
        /// </summary>
        /// <param name="currTurn"></param>
        public void Kill(int currTurn)
        {
            this.IsAlive = false;
            this.ReviveTurn = currTurn + Bot.Game.GetSpawnTurns();

            //TODO: make a revive event
        }

    }
}
