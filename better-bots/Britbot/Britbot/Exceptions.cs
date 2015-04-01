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


    internal class InvalidRingException : Exception
    {
        public InvalidRingException()
        {
            
        }

        public InvalidRingException(string message)
            : base(message)
        {
            
        }
    }
}