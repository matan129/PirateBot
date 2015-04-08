// Decompiled with JetBrains decompiler
// Type: Britbot.Mover
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

using Pirates;
using System.Collections.Generic;

namespace Britbot
{
  internal static class Mover
  {
    public static void MoveAll(Dictionary<Pirate, Direction> moves)
    {
      try
      {
        using (Dictionary<Pirate, Direction>.Enumerator enumerator = moves.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            KeyValuePair<Pirate, Direction> current = enumerator.Current;
            if (!current.Key.get_IsLost())
              Bot.Game.SetSail(current.Key, current.Value);
          }
        }
      }
      catch
      {
        using (List<Pirate>.Enumerator enumerator = Bot.Game.AllMyPirates().GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            Pirate current = enumerator.Current;
            if (!current.get_IsLost())
              Bot.Game.SetSail(current, (Direction) 45);
          }
        }
      }
    }
  }
}
