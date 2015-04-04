// Decompiled with JetBrains decompiler
// Type: Britbot.CommanderGaveUpException
// Assembly: dxz2zlat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4B5F765-3404-42F9-8DF1-AF1C46E25CE3
// Assembly location: C:\Users\Matan\AppData\Local\Temp\dxz2zlat.dll

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
