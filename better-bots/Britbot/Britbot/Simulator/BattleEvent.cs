using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Britbot.Simulator
{
    /// <summary>
    /// this represents a battle between two ships
    /// </summary>
    class BattleEvent : SimulatedEvent
    {
        //first group
        SimulatedGroup Group1;

        //second group
        SimulatedGroup Group2;

        public BattleEvent(SimulatedGroup g1,SimulatedGroup g2)
        {
            this.Group1 = g1;
            this.Group2 = g2;
        }

        public override void MakeShitHappen(SimulatedGame sg)
        {
            //check fire power
            if(this.Group1.ActualFirePower(sg) < this.Group2.ActualFirePower(sg))
            {
                this.Group1.Kill(sg.CurrentTurn);
            }
            else if(this.Group1.ActualFirePower(sg) == this.Group2.ActualFirePower(sg))
            {
                this.Group1.Kill(sg.CurrentTurn);
                this.Group2.Kill(sg.CurrentTurn);
            }
            else
            {
                this.Group2.Kill(sg.CurrentTurn);
            }
        }
    }
}
