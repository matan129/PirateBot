// Decompiled with JetBrains decompiler
// Type: Britbot.Bot
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

using Britbot.Fallback;
using Pirates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Britbot
{
  public class Bot : IPirateBot
  {
    private static Dictionary<Pirate, Direction> _movesDictionary = new Dictionary<Pirate, Direction>();
    private static Dictionary<Pirate, Direction> _fallbackMoves = new Dictionary<Pirate, Direction>();
    private static Task[] _tasks = new Task[2];

    public static IPirateGame Game { get; private set; }

    public void DoTurn(IPirateGame state)
    {
      Bot.Game = state;
      bool flag = false;
      try
      {
        flag = Bot.ExecuteBot();
      }
      catch (Exception ex)
      {
        Bot.Game.Debug("=================BOT ERROR=====================");
        Bot.Game.Debug("Bot almost crashed because of exception: " + ex.Message);
        StackFrame frame = new StackTrace(ex, true).GetFrame(0);
        Bot.Game.Debug("The exception was thrown from method {0} at file {1} at line #{2}", new object[3]
        {
          (object) frame.GetMethod(),
          (object) frame.GetFileName(),
          (object) frame.GetFileLineNumber()
        });
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

    private static bool ExecuteBot()
    {
      Bot._fallbackMoves.Clear();
      Bot._movesDictionary.Clear();
      bool onTime = false;
      int num = (int) ((Bot.Game.GetTurn() <= 1 ? 1000.0 : (double) Bot.Game.TimeRemaining()) * 0.65);
      CancellationTokenSource commanderCancellationSource = new CancellationTokenSource(num);
      try
      {
        Bot._tasks[0] = Task.Factory.StartNew((Action) (() =>
        {
          try
          {
            Bot._movesDictionary = Commander.Play(commanderCancellationSource.Token, out onTime);
          }
          catch (Exception ex)
          {
            Bot.Game.Debug("TOP LEVEL EXCEPTION WAS CAUGHT ON THE COMMANDER TASK ON TURN " + (object) Bot.Game.GetTurn());
            Bot.Game.Debug(ex.ToString());
            throw;
          }
        }));
        Bot._tasks[1] = Task.Factory.StartNew((Action) (() =>
        {
          try
          {
            Bot._fallbackMoves = FallbackBot.GetFallbackTurns();
          }
          catch (Exception ex)
          {
            Bot.Game.Debug("TOP LEVEL EXCEPTION WAS CAUGHT ON THE FALLBACK TASK ON TURN " + (object) Bot.Game.GetTurn());
            Bot.Game.Debug(ex.ToString());
            throw;
          }
        }));
        Task.WaitAll(Bot._tasks, num);
        if (!onTime)
        {
          Bot.Game.Debug("=================TIMEOUT=======================");
          Bot.Game.Debug("Commander timed out, switching to fallback code");
          Bot.Game.Debug("=================TIMEOUT=======================");
        }
      }
      finally
      {
        if (commanderCancellationSource != null)
          commanderCancellationSource.Dispose();
      }
      return onTime;
    }
  }
}
