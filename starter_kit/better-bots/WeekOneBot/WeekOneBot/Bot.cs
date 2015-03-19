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
				IsConnectedToInternet();
                this.commander.Distribute(Ai.GetBestConfig());
                this.inited = true;
            }

            this.commander.Play();
        }
		
		public bool IsConnectedToInternet()
        {
		    Game.Debug("Checking connection...");
            //if connected we can save logs between games and adapt the strategy
            try
			{
				using (var client = new WebClient())
				using (var stream = client.OpenRead("http://www.google.com"))
				{
					Game.Debug("Connected to Google!");
					return true;
				}
			}
			catch
			{
				Game.Debug("Could not connect");
				return false;
			}
		}		
    }
    
}