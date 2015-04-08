// Decompiled with JetBrains decompiler
// Type: Britbot.SmartIsland
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

using Pirates;
using System.Collections.Generic;

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

    private SmartIsland(int encapsulate)
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

    public Score GetScore(Group origin)
    {
      int dangerRadius = 4 * Bot.Game.GetAttackRadius();
      int attackRadius = Bot.Game.GetAttackRadius();
      if (this.NearbyEnemyCount(dangerRadius) > origin.LiveCount())
        return (Score) null;
      int num = Bot.Game.Distance(this.Loc, origin.FindCenter(true));
      int captureTurns = this.CaptureTurns;
      if (this.Owner != 0 || this.TeamCapturing == 1)
        return new Score((ITarget) this, TargetType.Island, (double) this.Value, (double) this.NearbyEnemyCount(attackRadius), (double) (num + captureTurns));
      return (Score) null;
    }

    public Location GetLocation()
    {
      return this.Loc;
    }

    public Direction GetDirection(Group group)
    {
      return Navigator.CalculateDirectionToStationeryTarget(group.FindCenter(true), group.Heading, this.GetLocation());
    }

    public TargetType GetTargetType()
    {
      return TargetType.Island;
    }

    public string GetDescription()
    {
      return string.Concat(new object[4]
      {
        (object) "Island, id: ",
        (object) this.Id,
        (object) " location: ",
        (object) this.Loc
      });
    }

    public bool Equals(ITarget operandB)
    {
      SmartIsland other = operandB as SmartIsland;
      if (other != (SmartIsland) null)
        return this.Equals(other);
      return false;
    }

    public override int GetHashCode()
    {
      return this.Id;
    }

    public static bool IsNearNonOurIsland(Location loc, int Range)
    {
      foreach (SmartIsland isle in SmartIsland.IslandList)
      {
        if (isle.Owner != 0 && Extensions.Distance(Bot.Game, loc, isle) < Range)
          return true;
      }
      return false;
    }

    public int NearbyEnemyCount(int dangerRadius = 15)
    {
      int num = 0;
      foreach (EnemyGroup enemyGroup in Enemy.Groups)
      {
        if (enemyGroup.MinimalSquaredDistanceTo(this.Loc) <= (double) dangerRadius)
          num += enemyGroup.EnemyPirates.Count;
      }
      return num;
    }

    protected bool Equals(SmartIsland other)
    {
      return this.Id == other.Id;
    }

    public override bool Equals(object obj)
    {
      SmartIsland other = obj as SmartIsland;
      if (other != (SmartIsland) null)
        return this.Equals(other);
      return false;
    }
  }
}
