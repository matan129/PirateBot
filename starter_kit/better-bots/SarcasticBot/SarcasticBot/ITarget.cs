using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SarcasticBot
{
    public interface ITarget
    {
        ScoreStruct GetScore(Group origin, out Path path, bool isFast);
    }
}
