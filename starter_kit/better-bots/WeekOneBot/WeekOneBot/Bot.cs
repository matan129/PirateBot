using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace WeekOneBot
{
    public class Bot : IPirateBot
    {
        public IPirateGame Game;
        private Commander commander;

        private bool inited;
        
        public void DoTurn(IPirateGame state)
        {
            this.Game = state;
            this.commander = Commander.Instance;
            this.commander.Game = state;
            if (!inited)
            {
                this.commander.SetConfiguration(Ai.GetBestConfig());
                this.commander.Distribute();
                this.inited = true;
            }

            this.commander.Play();
        }
    }
}