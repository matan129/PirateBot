﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace Britbot
{
    static class Utility
    {
        public static Location AddLocation(Location loc1, Location loc2)
        {
            return new Location(loc1.Row + loc2.Row, loc2.Col + loc2.Col);
        }

        public static Location SubtractLocation(Location loc1, Location loc2)
        {
            return new Location(loc1.Row - loc2.Row, loc2.Col - loc2.Col);
        }
    }
}
