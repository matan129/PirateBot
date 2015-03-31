// Decompiled with JetBrains decompiler
// Type: Britbot.Bot
// Assembly: 0ztn0bu3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAE66A6C-F769-4197-8684-0F1222C47342
// Assembly location: C:\Users\Matan\AppData\Local\Temp\0ztn0bu3.dll

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
