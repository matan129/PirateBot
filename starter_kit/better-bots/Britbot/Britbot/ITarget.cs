using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Britbot
{
    public interface ITarget
    {
        Score GetScore(Group origin);

        Pirates.Location GetLocation();
    }
}
