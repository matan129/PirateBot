// Decompiled with JetBrains decompiler
// Type: Britbot.EnemyGroup
// Assembly: 0ztn0bu3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAE66A6C-F769-4197-8684-0F1222C47342
// Assembly location: C:\Users\Matan\AppData\Local\Temp\0ztn0bu3.dll

using Pirates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Britbot
{
  public class EnemyGroup : ITarget
  {
    public static int idCount;
    public readonly int Id;

    public Location PrevLoc { get; set; }

    public List<int> EnemyPirates { get; private set; }

    public HeadingVector Heading { get; private set; }

    public EnemyGroup()
    {
      this.Id = EnemyGroup.idCount++;
      this.EnemyPirates = new List<int>();
      this.PrevLoc = new Location(0, 0);
      this.Heading = new HeadingVector(this.PrevLoc);
    }

    public EnemyGroup(Location prevLoc, List<int> enemyPirates, HeadingVector heading)
    {
      this.Id = EnemyGroup.idCount++;
      this.PrevLoc = prevLoc;
      this.EnemyPirates = enemyPirates;
      this.Heading = heading;
    }

    public static bool operator ==(EnemyGroup a, EnemyGroup b)
    {
      return a.Equals(b);
    }

    public static bool operator !=(EnemyGroup a, EnemyGroup b)
    {
      return !(a == b);
    }

    public bool Equals(ITarget operandB)
    {
      if (operandB is EnemyGroup)
        return this.Id == ((EnemyGroup) operandB).Id;
      return false;
    }

    protected bool Equals(EnemyGroup other)
    {
      return object.ReferenceEquals((object) this, (object) other);
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

    public int LiveCount()
    {
      return Enumerable.Count<Pirate>((IEnumerable<Pirate>) this.EnemyPirates.ConvertAll<Pirate>((Converter<int, Pirate>) (p => Bot.Game.GetMyPirate(p))), (Func<Pirate, bool>) (p => !p.get_IsLost()));
    }

    private int InRangeGroupDistance(EnemyGroup eg, Group group)
    {
      Pirate pirate1 = (Pirate) null;
      Pirate pirate2 = (Pirate) null;
      int num1 = Bot.Game.GetCols() + Bot.Game.GetRows();
      using (List<Pirate>.Enumerator enumerator1 = eg.EnemyPirates.ConvertAll<Pirate>((Converter<int, Pirate>) (ep => Bot.Game.GetEnemyPirate(ep))).GetEnumerator())
      {
        while (enumerator1.MoveNext())
        {
          Pirate current1 = enumerator1.Current;
          using (List<Pirate>.Enumerator enumerator2 = group.Pirates.ConvertAll<Pirate>((Converter<int, Pirate>) (pir => Bot.Game.GetMyPirate(pir))).GetEnumerator())
          {
            while (enumerator2.MoveNext())
            {
              Pirate current2 = enumerator2.Current;
              int num2 = Bot.Game.Distance(current1, current2);
              if (num2 < num1)
              {
                num1 = num2;
                pirate1 = current1;
                pirate2 = current2;
              }
            }
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
      this.EnemyPirates.RemoveAll((Predicate<int>) (pirate => removedPirates.Contains(pirate)));
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

    public string ToS()
    {
      string str = "Enemy Group, Pirates: ";
      foreach (int num in this.EnemyPirates)
        str = str + (object) " " + (string) (object) num;
      return str;
    }
  }
}
