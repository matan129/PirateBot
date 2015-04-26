using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;

namespace SarcasticBot
{
    public static class Enemy
    {
        public static List<EnemyGroup> Groups {get; private set; }

        public static List<int> Configuration
        {
            get { return GetConfiguration(); }
        }

        public static List<int> GetConfiguration()
        {
            throw new NotImplementedException();
        }

        public static double Causalties()
        {
            return Bot.Game.AllEnemyPirates().Count(p => p.IsLost)/
                   Bot.Game.AllEnemyPirates().Count;
        }
    }
}