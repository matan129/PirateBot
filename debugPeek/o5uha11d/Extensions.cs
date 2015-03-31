// Decompiled with JetBrains decompiler
// Type: Britbot.Extensions
// Assembly: o5uha11d, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FD1E92EE-5B89-4A73-A517-70BE2E057BAD
// Assembly location: C:\Users\Matan\AppData\Local\Temp\o5uha11d.dll

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
