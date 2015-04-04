// Decompiled with JetBrains decompiler
// Type: Britbot.Fallback.FallbackBot
// Assembly: dxz2zlat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4B5F765-3404-42F9-8DF1-AF1C46E25CE3
// Assembly location: C:\Users\Matan\AppData\Local\Temp\dxz2zlat.dll

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
