// Decompiled with JetBrains decompiler
// Type: Britbot.Bot
// Assembly: o5uha11d, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FD1E92EE-5B89-4A73-A517-70BE2E057BAD
// Assembly location: C:\Users\Matan\AppData\Local\Temp\o5uha11d.dll

using Pirates;
using System;

namespace Britbot
{
  public class Bot : IPirateBot
  {
    public static IPirateGame Game { get; private set; }

    public void DoTurn(IPirateGame state)
    {
      try
      {
        Bot.Game = state;
        Commander.Play();
      }
      catch (Exception ex)
      {
        Bot.Game.Debug("++++++++++++++++++++++++++++++++++++++++");
        Bot.Game.Debug("Almost crashed because of " + ex.Message);
        Bot.Game.Debug("++++++++++++++++++++++++++++++++++++++++");
      }
    }
  }
}
