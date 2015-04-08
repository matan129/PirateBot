// Decompiled with JetBrains decompiler
// Type: Britbot.InvalidRingException
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

using System;
using System.Runtime.Serialization;

namespace Britbot
{
  [Serializable]
  public class InvalidRingException : Exception
  {
    public InvalidRingException()
    {
    }

    public InvalidRingException(string message)
      : base(message)
    {
    }

    public InvalidRingException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected InvalidRingException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
