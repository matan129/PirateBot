#region Usings

using System;
using System.Runtime.Serialization;

#endregion

namespace Britbot
{
    [Serializable]
    public class InvalidIteratorDimensionException : Exception
    {
        #region Constructors & Initializers

        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidIteratorDimensionException()
        {
        }

        public InvalidIteratorDimensionException(string message) : base(message)
        {
        }

        public InvalidIteratorDimensionException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidIteratorDimensionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        #endregion
    }


    [Serializable]
    public class InvalidRingException : Exception
    {
        #region Constructors & Initializers

        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidRingException()
        {
        }

        public InvalidRingException(string message) : base(message)
        {
        }

        public InvalidRingException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidRingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        #endregion
    }

    [Serializable]
    public class InvalidLocationException : Exception
    {
        #region Constructors & Initializers

        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidLocationException()
        {
        }

        public InvalidLocationException(string message) : base(message)
        {
        }

        public InvalidLocationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidLocationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        #endregion
    }

    [Serializable]
    public class CommanderGaveUpException : Exception
    {
        #region Constructors & Initializers

        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public CommanderGaveUpException()
        {
        }

        public CommanderGaveUpException(string message) : base(message)
        {
        }

        public CommanderGaveUpException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CommanderGaveUpException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        #endregion
    }
}