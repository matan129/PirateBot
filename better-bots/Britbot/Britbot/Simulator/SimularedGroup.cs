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
        public int Id;

        /// <summary>
        /// The owner of this group (Me or Enemy)
        /// </summary>
        public int Owner;

        /// <summary>
        /// The actual firepower of the group
        /// </summary>
        public double FirePower;

        /// <summary>
        /// True if the enemy group is alive
        /// </summary>
        public bool IsAlive;

        /// <summary>
        /// The turn when the group revives
        /// </summary>
        public int ReviveTurn;


        /// <summary>
        /// just a constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="owner"></param>
        /// <param name="firePower"></param>
        public SimularedGroup(int id, int owner, double firePower )
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

        public bool IsCapturing(SimulatedGame sg)
        {
            //going over all the islands to check if we are capturing them
            for(int i = 0;i < sg.Islands.Length;i++)
            {
                if (this == sg.Islands[i].CapturingGroup)
                    return true;
            }
            return true;
        }

        public int ActualFirePower(SimulatedGame sg)
        {
            if (this.IsCapturing())
                return this.FirePower - 1;
            return this.FirePower;
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


        public static override bool operator ==(SimularedGroup sg1, SimularedGroup sg2)
        {
            return sg1.Id == sg2.Id;
        }
    }
}
