using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
    }
}