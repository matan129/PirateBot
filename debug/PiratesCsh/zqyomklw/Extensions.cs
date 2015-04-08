// Decompiled with JetBrains decompiler
// Type: Britbot.Extensions
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

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

    public static Direction Oppsite(this Direction dir)
    {
      Direction direction = dir;
      if (direction <= 110)
      {
        if (direction == 101)
          return (Direction) 119;
        if (direction == 110)
          return (Direction) 115;
      }
      else
      {
        if (direction == 115)
          return (Direction) 110;
        if (direction == 119)
          return (Direction) 101;
      }
      return (Direction) 45;
    }

    public static double EuclidianDistanceSquared(this IPirateGame game, Location loc1, Location loc2)
    {
      return Math.Pow((double) (loc1.get_Col() - loc2.get_Col()), 2.0) + Math.Pow((double) (loc1.get_Row() - loc2.get_Row()), 2.0);
    }

    public static bool IsReallyInRange(this IPirateGame game, Location loc1, Location loc2)
    {
      return Extensions.EuclidianDistanceSquared(game, loc1, loc2) < (double) game.GetAttackRadius();
    }

    public static List<SmartIsland> SmartIslands(this IPirateGame game)
    {
      return SmartIsland.IslandList;
    }

    public static bool IsPassableEnough(this IPirateGame game, Location loc, int passRadious)
    {
      for (int index1 = -passRadious; index1 <= passRadious; ++index1)
      {
        for (int index2 = -Math.Abs(passRadious - index1); index2 <= Math.Abs(passRadious - index1); ++index2)
        {
          if (!Bot.Game.IsPassable(new Location(loc.get_Row() + index2, loc.get_Col() + index1)))
            return false;
        }
      }
      return true;
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

    public static Location AdvancePivot(this Location pivot)
    {
      int cols = Bot.Game.GetCols();
      int rows = Bot.Game.GetRows();
      int num1 = 0;
      int num2 = 0;
      int num3 = cols - pivot.get_Col();
      int num4 = rows - pivot.get_Row();
      if (num3 > pivot.get_Col())
        ++num1;
      else if (num3 < pivot.get_Col())
        --num1;
      if (num4 > pivot.get_Row())
        ++num2;
      else if (num4 < pivot.get_Row())
        --num2;
      pivot = new Location(pivot.get_Row() + num2, pivot.get_Col() + num1);
      return pivot;
    }
  }
}
