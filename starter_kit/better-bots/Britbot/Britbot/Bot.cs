using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace Britbot
{
    public class Bot : IPirateBot
    {
        public static IPirateGame Game { get; private set; }
        public void DoTurn(IPirateGame state)
        {
            Game = state;
            throw new System.NotImplementedException();
        }
    }
}
