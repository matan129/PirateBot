// Decompiled with JetBrains decompiler
// Type: Britbot.SmartIsland
// Assembly: 0ztn0bu3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAE66A6C-F769-4197-8684-0F1222C47342
// Assembly location: C:\Users\Matan\AppData\Local\Temp\0ztn0bu3.dll

using Pirates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Britbot
{
  public class SmartIsland : ITarget
  {
    public readonly int Id;

    public static List<SmartIsland> IslandList { get; private set; }

    public int CaptureTurns
    {
      get
      {
        return Bot.Game.GetIsland(this.Id).get_CaptureTurns();
      }
    }

    public int Value
    {
      get
      {
        return Bot.Game.GetIsland(this.Id).get_Value();
      }
    }

    public Location Loc
    {
      get
      {
        return Bot.Game.GetIsland(this.Id).get_Loc();
      }
    }

    public int TeamCapturing
    {
      get
      {
        return Bot.Game.GetIsland(this.Id).get_TeamCapturing();
      }
    }

    public int TurnsBeingCaptured
    {
      get
      {
        return Bot.Game.GetIsland(this.Id).get_TurnsBeingCaptured();
      }
    }

    public int Owner
    {
      get
      {
        return Bot.Game.GetIsland(this.Id).get_Owner();
      }
    }

    static SmartIsland()
    {
      SmartIsland.IslandList = new List<SmartIsland>();
      using (List<Island>.Enumerator enumerator = Bot.Game.Islands().GetEnumerator())
      {
        while (enumerator.MoveNext())
          SmartIsland.IslandList.Add(new SmartIsland(enumerator.Current.get_Id()));
      }
    }

    public SmartIsland(int encapsulate)
    {
      this.Id = encapsulate;
    }

    public static bool operator ==(SmartIsland a, SmartIsland b)
    {
      return object.Equals((object) a, (object) b);
    }

    public static bool operator !=(SmartIsland a, SmartIsland b)
    {
      return !object.Equals((object) a, (object) b);
    }

    public bool Equals(ITarget operandB)
    {
      if (operandB is SmartIsland)
        return this.Equals((SmartIsland) operandB);
      return false;
    }

    protected bool Equals(SmartIsland other)
    {
      return this.Id == other.Id;
    }

    public override bool Equals(object obj)
    {
      if (obj is SmartIsland)
        return this.Equals((SmartIsland) obj);
      return false;
    }

    public override int GetHashCode()
    {
      return ((base.GetHashCode() * 397 ^ this.CaptureTurns) * 397 ^ this.Value) * 397 ^ (this.Loc != null ? ((object) this.Loc).GetHashCode() : 0);
    }

    public Score GetScore(Group origin)
    {
      int num = Enumerable.Min(Enumerable.Concat<int>(Enumerable.Select<Pirate, int>((IEnumerable<Pirate>) origin.Pirates.ConvertAll<Pirate>((Converter<int, Pirate>) (p => Bot.Game.GetMyPirate(p))), (Func<Pirate, int>) (e => Bot.Game.Distance(e.get_Loc(), this.Loc))), (IEnumerable<int>) new int[0]));
      int captureTurns = this.CaptureTurns;
      if (this.Owner != 0)
        return new Score((ITarget) this, TargetType.Island, (double) (origin.Pirates.Count * (this.Value - 1) + 1), (double) (num + captureTurns));
      return (Score) null;
    }

    public Location GetLocation()
    {
      return this.Loc;
    }

    public Direction GetDirection(Group group)
    {
      return HeadingVector.CalculateDirectionToStaitionaryTarget(group.GetLocation(), group.Heading, this.GetLocation());
    }

    public int NearbyEnemyCount()
    {
      int num1 = 0;
      int num2 = 0;
      foreach (SmartIsland smartIsland in SmartIsland.IslandList)
      {
        int num3 = Bot.Game.Distance(smartIsland.Loc, this.Loc);
        if (num3 < num2)
          num2 = num3;
      }
      int num4 = num2 / 2;
      foreach (EnemyGroup enemyGroup in Enemy.Groups)
      {
        if (enemyGroup.MinimalDistanceTo(this.Loc) <= num4 && enemyGroup.GuessTarget() == this)
          num1 += enemyGroup.EnemyPirates.Count;
      }
      return num1;
    }

    public string ToS()
    {
      return string.Concat(new object[4]
      {
        (object) "Island, id: ",
        (object) this.Id,
        (object) " location: ",
        (object) ((object) this.Loc).ToString()
      });
    }
  }
}
