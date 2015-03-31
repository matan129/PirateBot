using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Britbot
{
    class InvalidIteratorDimension : Exception
    {
        public InvalidIteratorDimension() { }
        public InvalidIteratorDimension(string message)
            : base(message) { }
    }
}
