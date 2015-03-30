using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace DaemonNine
{
    public class Ai
    {
        public static bool IsAngry { get; private set; }
        public static int CBots { get; private set; }

        static Ai()
        {
            int mpc = Bot.Game.AllMyPirates().Count;
            int opc = Bot.Game.AllEnemyPirates().Count;
            IsAngry = mpc > opc || Bot.Game.MyIslands().Count == 0;
            CBots = Math.Max(Bot.Game.NotMyIslands().Count/2, mpc);
        }
    }
}
