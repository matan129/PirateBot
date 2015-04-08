// Decompiled with JetBrains decompiler
// Type: Britbot.Fallback.FallbackBot
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

using Britbot;
using Pirates;
using System.Collections.Generic;

namespace Britbot.Fallback
{
  public class FallbackBot
  {
    public static Dictionary<Pirate, Direction> GetFallbackTurns()
    {
      Dictionary<Pirate, Direction> dictionary = new Dictionary<Pirate, Direction>();
      using (List<Pirate>.Enumerator enumerator = Bot.Game.AllMyPirates().GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          Pirate current = enumerator.Current;
          if (Bot.Game.GetTurn() % 2 == 0)
            dictionary.Add(current, (Direction) 110);
          else
            dictionary.Add(current, (Direction) 115);
        }
      }
      Bot.Game.Debug("===============FALLBACK READY=================");
      return dictionary;
    }
  }
}
