// Decompiled with JetBrains decompiler
// Type: Britbot.InvalidIteratorDimensionException
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

using System;
using System.Runtime.Serialization;

namespace Britbot
{
  [Serializable]
  public class InvalidIteratorDimensionException : Exception
  {
    public InvalidIteratorDimensionException()
    {
    }

    public InvalidIteratorDimensionException(string message)
      : base(message)
    {
    }

    public InvalidIteratorDimensionException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected InvalidIteratorDimensionException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
