using System;
using Pirates;

namespace SarcasticBot
{
    public class EnemyPirate : SmartPirate
    {
        public EnemyPirate()
        {
            throw new NotImplementedException();
        }

        public bool IsCloaked
        {
            get { throw new NotImplementedException(); }
            set { }
        }

        public bool InRange(SmartPirate p)
        {
            throw new NotImplementedException();
        }

        public bool IsLost()
        {
            return Bot.Game.GetEnemyPirate(this.Index).IsLost;
        }
    }
}