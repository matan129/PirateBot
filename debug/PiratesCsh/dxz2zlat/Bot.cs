// Decompiled with JetBrains decompiler
// Type: Britbot.Bot
// Assembly: dxz2zlat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4B5F765-3404-42F9-8DF1-AF1C46E25CE3
// Assembly location: C:\Users\Matan\AppData\Local\Temp\dxz2zlat.dll

using Britbot.Fallback;
using Pirates;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Britbot
{
  public class Bot : IPirateBot
  {
    private static Dictionary<Pirate, Direction> _movesDictionary = new Dictionary<Pirate, Direction>();
    private static Dictionary<Pirate, Direction> _fallbackMoves = new Dictionary<Pirate, Direction>();

    public static IPirateGame Game { get; private set; }

    private static bool ExecuteBot()
    {
      Bot._fallbackMoves = new Dictionary<Pirate, Direction>();
      Bot._movesDictionary = new Dictionary<Pirate, Direction>();
      Thread thread1 = new Thread((ThreadStart) (() => Bot._movesDictionary = Commander.Play()));
      Thread thread2 = new Thread((ThreadStart) (() => Bot._fallbackMoves = FallbackBot.GetFallbackTurns()));
      thread1.Start();
      thread2.Start();
      bool flag = thread1.Join((int) ((double) Bot.Game.TimeRemaining() * 0.5));
      if (!flag)
      {
        thread1.Abort();
        Bot.Game.Debug("=================TIMEOUT=======================");
        Bot.Game.Debug("Commander timed out, switching to fallback code");
        Bot.Game.Debug("=================TIMEOUT=======================");
      }
      return flag;
    }

    public void DoTurn(IPirateGame state)
    {
      Bot.Game = state;
      SmartIsland.Init();
      Commander.Init();
      bool flag = false;
      try
      {
        flag = Bot.ExecuteBot();
      }
      catch (Exception ex)
      {
        Bot.Game.Debug("=================BOT ERROR=====================");
        Bot.Game.Debug("Bot almost crashed because of exception: " + ex.Message);
        Bot.Game.Debug("=================BOT ERROR=====================");
      }
      finally
      {
        if (flag)
          Mover.MoveAll(Bot._movesDictionary);
        else
          Mover.MoveAll(Bot._fallbackMoves);
      }
    }
  }
}
