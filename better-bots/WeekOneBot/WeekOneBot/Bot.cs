using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;


namespace WeekOneBot
{
    public class Bot : IPirateBot
    {
        public static IPirateGame Game;
        private Commander commander;

        private bool inited;

        public void DoTurn(IPirateGame state)
        {
            Game = state;
            this.commander = Commander.Instance;

            if (!inited)
            {
                this.commander.Distribute(Ai.GetBestConfig());
                this.inited = true;
            }

            this.commander.Play();
        }
    }
}