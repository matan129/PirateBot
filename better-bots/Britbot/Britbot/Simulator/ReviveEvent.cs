using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Britbot.Simulator
{
    class ReviveEvent : SimulatedEvent
    {
        /// <summary>
        /// The group to be revived
        /// </summary>
        public SimulatedGroup GroupToRevive;

        public ReviveEvent(int turn, SimulatedGroup groupToRevive) : base(turn)
        {
            this.GroupToRevive = groupToRevive;
        }

        //revive group
        public override bool Activate(SimulatedGame sg)
        {
            this.GroupToRevive.IsAlive = true;

            return false;
        }
    }
}
