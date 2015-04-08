// Decompiled with JetBrains decompiler
// Type: Britbot.CommanderGaveUpException
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

using System;
using System.Runtime.Serialization;

namespace Britbot
{
  [Serializable]
  public class CommanderGaveUpException : Exception
  {
    public CommanderGaveUpException()
    {
    }

    public CommanderGaveUpException(string message)
      : base(message)
    {
    }

    public CommanderGaveUpException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected CommanderGaveUpException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
