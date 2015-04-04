// Decompiled with JetBrains decompiler
// Type: Britbot.Extensions
// Assembly: dxz2zlat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4B5F765-3404-42F9-8DF1-AF1C46E25CE3
// Assembly location: C:\Users\Matan\AppData\Local\Temp\dxz2zlat.dll

using Pirates;
using System;
using System.Collections.Generic;

namespace Britbot
{
  public static class Extensions
  {
    public static SmartIsland GetSmartIsland(this IPirateGame game, int id)
    {
      return SmartIsland.IslandList.Find((Predicate<SmartIsland>) (isle => isle.Id == id));
    }

    public static int Distance(this IPirateGame game, Location loc, SmartIsland isle)
    {
      return Bot.Game.Distance(loc, isle.Loc);
    }

    public static List<SmartIsland> SmartIslands(this IPirateGame game)
    {
      return SmartIsland.IslandList;
    }

    public static List<Direction> GetDirections(this IPirateGame game, Pirate a, Pirate b)
    {
      return Bot.Game.GetDirections(a.get_Loc(), b.get_Loc());
    }

    public static IEnumerable<Pirate> InRangeFriends(this Pirate pirate)
    {
      // ISSUE: object of a compiler-generated type is created
      return (IEnumerable<Pirate>) new Extensions.\u003CInRangeFriends\u003Ed__3(-2)
      {
        \u003C\u003E3__pirate = pirate
      };
    }

    public static bool IsActuallyPassable(this Location loc)
    {
      return !Bot.Game.IsOccupied(loc) && Bot.Game.IsPassable(loc);
    }

    public static Location Add(this Location loc1, Location loc2)
    {
      return new Location(loc1.get_Row() + loc2.get_Row(), loc2.get_Col() + loc2.get_Col());
    }

    public static Location Subtract(this Location loc1, Location loc2)
    {
      return new Location(loc1.get_Row() - loc2.get_Row(), loc2.get_Col() - loc2.get_Col());
    }
  }
}
