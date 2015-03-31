using System;

namespace Britbot
{
    internal class InvalidIteratorDimension : Exception
    {
        public InvalidIteratorDimension()
        {
        }

        public InvalidIteratorDimension(string message)
            : base(message)
        {
        }
    }
}