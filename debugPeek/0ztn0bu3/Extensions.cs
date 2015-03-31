// Decompiled with JetBrains decompiler
// Type: Britbot.Extensions
// Assembly: 0ztn0bu3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAE66A6C-F769-4197-8684-0F1222C47342
// Assembly location: C:\Users\Matan\AppData\Local\Temp\0ztn0bu3.dll

using Pirates;
using System;
using System.Collections.Generic;

namespace Britbot
{
  public static class Extensions
  {
    public static SmartIsland GetSmartIsland(this IPirateGame game, int Id)
    {
      return SmartIsland.IslandList.Find((Predicate<SmartIsland>) (isle => isle.Id == Id));
    }

    public static List<SmartIsland> SmartIslands(this IPirateGame game)
    {
      return SmartIsland.IslandList;
    }

    public static List<Direction> GetDirections(this IPirateGame game, Pirate a, Pirate b)
    {
      return Bot.Game.GetDirections(a.get_Loc(), b.get_Loc());
    }
  }
}
