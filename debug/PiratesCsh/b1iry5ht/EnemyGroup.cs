// Decompiled with JetBrains decompiler
// Type: Britbot.EnemyGroup
// Assembly: b1iry5ht, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6E84BA16-E0F0-4D91-BADE-8A0B9BF81F38
// Assembly location: C:\Users\Matan\AppData\Local\Temp\b1iry5ht.dll

using Pirates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Britbot
{
  public class EnemyGroup : ITarget
  {
    public static int IdCount;
    public readonly int Id;

    public Location PrevLoc { get; set; }

    public List<int> EnemyPirates { get; private set; }

    public HeadingVector Heading { get; private set; }

    public EnemyGroup()
    {
      this.Id = EnemyGroup.IdCount++;
      this.EnemyPirates = new List<int>();
      this.PrevLoc = new Location(0, 0);
      this.Heading = new HeadingVector(this.PrevLoc);
    }

    public EnemyGroup(Location prevLoc, List<int> enemyPirates, HeadingVector heading)
    {
      this.Id = EnemyGroup.IdCount++;
      this.PrevLoc = prevLoc;
      this.EnemyPirates = enemyPirates;
      this.Heading = heading;
    }

    public bool Equals(ITarget operandB)
    {
      EnemyGroup enemyGroup = operandB as EnemyGroup;
      if (enemyGroup != null)
        return object.Equals((object) this, (object) enemyGroup);
      return false;
    }

    protected bool Equals(EnemyGroup other)
    {
      if (this.EnemyPirates.Count != other.EnemyPirates.Count)
        return false;
      for (int index = 0; index < this.EnemyPirates.Count; ++index)
      {
        if (this.EnemyPirates[index] != other.EnemyPirates[index])
          return false;
      }
      return true;
    }

    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals((object) null, obj))
        return false;
      if (object.ReferenceEquals((object) this, obj))
        return true;
      if (obj.GetType() != this.GetType())
        return false;
      return this.Equals((EnemyGroup) obj);
    }

    public Score GetScore(Group origin)
    {
      if (this.Heading.Norm1() < 2 || !HeadingVector.IsReachable(origin.GetLocation(), this.GetLocation(), this.Heading))
        return (Score) null;
      double num = HeadingVector.CalcDistFromLine(origin.GetLocation(), this.GetLocation(), this.Heading);
      Bot.Game.Debug("EnemyGroup's HeadingVector CalcFromLine returned: " + (object) num);
      double eta = num - (double) Bot.Game.GetAttackRadius();
      if (origin.LiveCount() <= this.LiveCount())
        return (Score) null;
      Bot.Game.Debug("EnemyGroup was moved to ExpIterator processing:" + (object) this.Id + " " + (string) (object) this.LiveCount() + " " + (string) (object) origin.LiveCount());
      return new Score((ITarget) this, TargetType.EnemyGroup, (double) this.EnemyPirates.Count, eta);
    }

    public Location GetLocation()
    {
      List<Location> list = new List<Location>();
      if (this.EnemyPirates == null)
        return new Location(0, 0);
      foreach (int num in this.EnemyPirates)
      {
        Pirate enemyPirate = Bot.Game.GetEnemyPirate(num);
        list.Add(enemyPirate.get_Loc());
      }
      int num1 = Enumerable.Sum<Location>((IEnumerable<Location>) list, (Func<Location, int>) (loc => loc.get_Col()));
      int num2 = Enumerable.Sum<Location>((IEnumerable<Location>) list, (Func<Location, int>) (loc => loc.get_Row()));
      return new Location(num1 / list.Count, num2 / list.Count);
    }

    public Direction GetDirection(Group group)
    {
      return HeadingVector.CalculateDirectionToMovingTarget(group.GetLocation(), group.Heading, this.GetLocation(), this.Heading);
    }

    public string GetDescription()
    {
      string str = "Enemy Group, Pirates: ";
      foreach (int num in this.EnemyPirates)
        str = str + (object) " " + (string) (object) num;
      return str;
    }

    public int LiveCount()
    {
      return Enumerable.Count<Pirate>((IEnumerable<Pirate>) this.EnemyPirates.ConvertAll<Pirate>((Converter<int, Pirate>) (p => Bot.Game.GetMyPirate(p))), (Func<Pirate, bool>) (p => !p.get_IsLost()));
    }

    private int InRangeGroupDistance(EnemyGroup eg, Group group)
    {
      Pirate pirate1 = (Pirate) null;
      Pirate pirate2 = (Pirate) null;
      int num1 = Bot.Game.GetCols() + Bot.Game.GetRows();
      for (int index1 = 0; index1 < eg.EnemyPirates.Count; ++index1)
      {
        Pirate enemyPirate = Bot.Game.GetEnemyPirate(index1);
        for (int index2 = 0; index2 < group.Pirates.Count; ++index2)
        {
          Pirate myPirate = Bot.Game.GetMyPirate(group.Pirates[index2]);
          int num2 = Bot.Game.Distance(enemyPirate, myPirate);
          if (num2 < num1)
          {
            num1 = num2;
            pirate1 = enemyPirate;
            pirate2 = myPirate;
          }
        }
      }
      return Bot.Game.Distance(pirate1.get_Loc(), pirate2.get_Loc()) - Bot.Game.GetAttackRadius() * 2;
    }

    public bool IsInGroup(int enemyPirate)
    {
      Pirate ePirate = Bot.Game.GetEnemyPirate(enemyPirate);
      return Enumerable.Min(Enumerable.Concat<int>(Enumerable.Select<Pirate, int>((IEnumerable<Pirate>) this.EnemyPirates.ConvertAll<Pirate>((Converter<int, Pirate>) (e => Bot.Game.GetEnemyPirate(e))), (Func<Pirate, int>) (ep => Bot.Game.Distance(ep, ePirate))), (IEnumerable<int>) new int[0])) <= 2;
    }

    public static bool IsInGroup(List<int> group, int enemyPirate)
    {
      Pirate ePirate = Bot.Game.GetEnemyPirate(enemyPirate);
      return Enumerable.Min(Enumerable.Concat<int>(Enumerable.Select<Pirate, int>((IEnumerable<Pirate>) group.ConvertAll<Pirate>((Converter<int, Pirate>) (e => Bot.Game.GetEnemyPirate(e))), (Func<Pirate, int>) (ep => Bot.Game.Distance(ep, ePirate))), (IEnumerable<int>) new int[0])) <= 2;
    }

    public int MinimalDistanceTo(Location location)
    {
      return Enumerable.Min(Enumerable.Concat<int>(Enumerable.Select<Pirate, int>((IEnumerable<Pirate>) this.EnemyPirates.ConvertAll<Pirate>((Converter<int, Pirate>) (p => Bot.Game.GetEnemyPirate(p))), (Func<Pirate, int>) (pirate => Bot.Game.Distance(pirate.get_Loc(), location))), (IEnumerable<int>) new int[0]));
    }

    public SmartIsland GuessTarget()
    {
      List<SmartIsland> islandList = SmartIsland.IslandList;
      islandList.Sort((Comparison<SmartIsland>) ((a, b) => Bot.Game.Distance(b.Loc, this.GetLocation()).CompareTo(Bot.Game.Distance(a.Loc, this.GetLocation()))));
      foreach (SmartIsland smartIsland in islandList)
      {
        if (HeadingVector.CalcDistFromLine(smartIsland.GetLocation(), this.GetLocation(), this.Heading) < 2.0)
          return smartIsland;
      }
      return (SmartIsland) null;
    }

    public void UpdateHeading()
    {
      Direction direction = Bot.Game.GetDirections(this.PrevLoc, this.GetLocation())[0];
      this.PrevLoc = this.GetLocation();
      this.Heading += direction;
    }

    public EnemyGroup Split(List<int> removedPirates)
    {
      if (removedPirates.Count == this.EnemyPirates.Count)
        return (EnemyGroup) null;
      EnemyGroup enemyGroup = new EnemyGroup(this.PrevLoc, removedPirates, this.Heading);
      this.EnemyPirates.RemoveAll(new Predicate<int>(removedPirates.Contains));
      return enemyGroup;
    }

    public void Join(EnemyGroup enemyGroup)
    {
      if (this.Id == enemyGroup.Id)
        return;
      this.EnemyPirates.AddRange((IEnumerable<int>) enemyGroup.EnemyPirates);
      this.Heading += enemyGroup.Heading;
      this.PrevLoc = new Location((this.PrevLoc.get_Row() + enemyGroup.PrevLoc.get_Row()) / 2, (this.PrevLoc.get_Col() + enemyGroup.PrevLoc.get_Col()) / 2);
    }

    public override string ToString()
    {
      return this.EnemyPirates.Count.ToString();
    }
  }
}
