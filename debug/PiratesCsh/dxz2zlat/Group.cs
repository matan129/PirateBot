// Decompiled with JetBrains decompiler
// Type: Britbot.Group
// Assembly: dxz2zlat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4B5F765-3404-42F9-8DF1-AF1C46E25CE3
// Assembly location: C:\Users\Matan\AppData\Local\Temp\dxz2zlat.dll

using Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Britbot
{
  public class Group
  {
    public readonly int Id;

    public HeadingVector Heading { get; private set; }

    public List<int> Pirates { get; private set; }

    public ITarget Target { get; private set; }

    public GroupRole Role { get; private set; }

    public List<Score> Priorities { get; private set; }

    public Thread CalcThread { get; private set; }

    public static int GroupCounter { get; private set; }

    public Dictionary<int, Location> FormOrders { get; private set; }

    public Group(int index, int amount)
    {
      this.Pirates = new List<int>();
      this.Heading = new HeadingVector(0.0, 0.0);
      this.Priorities = new List<Score>();
      this.Role = GroupRole.Destroyer;
      this.Id = Group.GroupCounter++;
      Bot.Game.Debug("===================GROUP {0}===================", new object[1]
      {
        (object) this.Id
      });
      for (; amount > 0; --amount)
        this.Pirates.Add(index + amount - 1);
      this.FormDictionary((Location[]) null);
    }

    public void SetTarget(ITarget target)
    {
      if (this.Target == null)
      {
        this.Target = target;
        this.Heading.SetCoordinates(0.0, 0.0);
      }
      else
      {
        if (object.Equals((object) this.Target, (object) target))
          return;
        this.Target = target;
        this.Heading.SetCoordinates(0.0, 0.0);
      }
    }

    public Location GetLocation()
    {
      int num1 = 0;
      int num2 = 0;
      if (this.Pirates == null)
        this.Pirates = new List<int>();
      foreach (int num3 in this.Pirates)
      {
        num1 += Bot.Game.GetMyPirate(num3).get_Loc().get_Col();
        num2 += Bot.Game.GetMyPirate(num3).get_Loc().get_Row();
      }
      try
      {
        return new Location(num2 / this.Pirates.Count, num1 / this.Pirates.Count);
      }
      catch (Exception ex)
      {
        return new Location(0, 0);
      }
    }

    public IEnumerable<KeyValuePair<Pirate, Direction>> GetGroupMoves()
    {
      // ISSUE: object of a compiler-generated type is created
      return (IEnumerable<KeyValuePair<Pirate, Direction>>) new Group.\u003CGetGroupMoves\u003Ed__5(-2)
      {
        \u003C\u003E4__this = this
      };
    }

    private double CasualtiesPercent()
    {
      return (double) (100 * (Enumerable.Count<int>((IEnumerable<int>) this.Pirates, (Func<int, bool>) (p => Bot.Game.GetMyPirate(p).get_IsLost())) / this.Pirates.Count));
    }

    private bool IsFormed()
    {
      if (this.CasualtiesPercent() > 20.0)
        return false;
      int num1 = 0;
      int num2 = 0;
      bool flag1 = false;
      bool flag2 = false;
      using (Dictionary<int, Location>.Enumerator enumerator = this.FormOrders.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          KeyValuePair<int, Location> current = enumerator.Current;
          Pirate myPirate = Bot.Game.GetMyPirate(current.Key);
          if (myPirate != null && !myPirate.get_IsLost())
          {
            if (!flag1)
            {
              num1 = current.Value.get_Col() - myPirate.get_Loc().get_Col();
              num2 = current.Value.get_Row() - myPirate.get_Loc().get_Row();
              flag1 = true;
            }
            if (myPirate.get_Loc().get_Col() + num1 != current.Value.get_Col() || myPirate.get_Loc().get_Row() + num2 != current.Value.get_Row())
            {
              flag2 = true;
              break;
            }
          }
        }
      }
      if (!flag2)
      {
        Bot.Game.Debug("Group {0} is formed", new object[1]
        {
          (object) this.Id
        });
        return true;
      }
      Pirate pirateOn = Bot.Game.GetPirateOn(this.FindCenter(false));
      Location[] structure = (Location[]) null;
      if (pirateOn != null)
      {
        try
        {
          structure = this.GetStructure(pirateOn.get_Loc());
        }
        catch (InvalidLocationException ex)
        {
          Bot.Game.Debug("Group {0} is not formed yet", new object[1]
          {
            (object) this.Id
          });
          return false;
        }
        if (Enumerable.All<Location>((IEnumerable<Location>) structure, (Func<Location, bool>) (loc => Bot.Game.GetPirateOn(loc) != null)))
        {
          Bot.Game.Debug("Group {0} is formed", new object[1]
          {
            (object) this.Id
          });
          return true;
        }
      }
      Bot.Game.Debug("Group {0} is not formed yet", new object[1]
      {
        (object) this.Id
      });
      this.FormDictionary(structure);
      return false;
    }

    private void FormDictionary(Location[] structure = null)
    {
      Location center = this.FindCenter(false);
      if (structure == null)
      {
        Bot.Game.Debug("Forming group structure");
        while (true)
        {
          try
          {
            structure = this.GetStructure(center);
            break;
          }
          catch (InvalidLocationException ex)
          {
            center = this.AdvancePivot(center);
          }
        }
      }
      bool[] flagArray = new bool[structure.Length];
      Dictionary<Pirate, Location> dictionary = new Dictionary<Pirate, Location>();
      List<Pirate> list = this.Pirates.ConvertAll<Pirate>((Converter<int, Pirate>) (p => Bot.Game.GetMyPirate(p)));
      list.Sort((Comparison<Pirate>) ((b, a) => Bot.Game.Distance(a.get_Loc(), center).CompareTo(Bot.Game.Distance(b.get_Loc(), center))));
      using (List<Pirate>.Enumerator enumerator = list.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          Pirate current = enumerator.Current;
          Location location = (Location) null;
          int num = Bot.Game.GetCols() + Bot.Game.GetRows();
          int index1 = 0;
          for (int index2 = 0; index2 < structure.Length; ++index2)
          {
            if (!flagArray[index2] && Bot.Game.Distance(current.get_Loc(), structure[index2]) < num)
            {
              num = Bot.Game.Distance(current.get_Loc(), structure[index2]);
              location = structure[index2];
              index1 = index2;
            }
          }
          flagArray[index1] = true;
          dictionary.Add(current, location);
        }
      }
      this.FormOrders = Enumerable.ToDictionary<KeyValuePair<Pirate, Location>, int, Location>((IEnumerable<KeyValuePair<Pirate, Location>>) Enumerable.OrderBy<KeyValuePair<Pirate, Location>, int>((IEnumerable<KeyValuePair<Pirate, Location>>) dictionary, (Func<KeyValuePair<Pirate, Location>, int>) (pair => Bot.Game.Distance(pair.Key.get_Loc(), pair.Value))), (Func<KeyValuePair<Pirate, Location>, int>) (pair => pair.Key.get_Id()), (Func<KeyValuePair<Pirate, Location>, Location>) (pair => pair.Value));
      Bot.Game.Debug("====FORMING TO====");
      using (Dictionary<int, Location>.Enumerator enumerator = this.FormOrders.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          KeyValuePair<int, Location> current = enumerator.Current;
          Bot.Game.Debug((string) (object) Bot.Game.GetMyPirate(current.Key) + (object) "," + (string) (object) current.Value);
        }
      }
      Bot.Game.Debug("==================");
    }

    private Location[] GetStructure(Location pivot)
    {
      int num = (int) Math.Ceiling((Decimal) (this.Pirates.Count - 1) / new Decimal(4));
      List<Location> list = new List<Location>();
      for (int ringOrdinal = 0; ringOrdinal <= num; ++ringOrdinal)
        list.AddRange((IEnumerable<Location>) Group.GetRing(pivot, ringOrdinal));
      return Enumerable.ToArray<Location>(Enumerable.Take<Location>((IEnumerable<Location>) list, this.Pirates.Count));
    }

    private Location AdvancePivot(Location pivot)
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

    private static List<Location> GetRing(Location pivot, int ringOrdinal)
    {
      if (ringOrdinal < 0)
        throw new InvalidRingException("Ring ordinal must be non-negative");
      List<Location> list = new List<Location>(ringOrdinal * 4);
      int row = pivot.get_Row();
      int col = pivot.get_Col();
      for (int index = row - ringOrdinal; index <= row + ringOrdinal; ++index)
      {
        Location location1 = new Location(index, (int) (((double) (2 * col) + Math.Sqrt(4.0 * Math.Pow((double) col, 2.0) + 4.0 * (Math.Pow((double) (ringOrdinal - Math.Abs(row - index)), 2.0) - Math.Pow((double) col, 2.0)))) / 2.0));
        if (!Bot.Game.IsPassable(location1))
          throw new InvalidLocationException("Location is not passable!");
        list.Add(location1);
        Location location2 = new Location(index, (int) (((double) (2 * col) - Math.Sqrt(4.0 * Math.Pow((double) col, 2.0) + 4.0 * (Math.Pow((double) (ringOrdinal - Math.Abs(row - index)), 2.0) - Math.Pow((double) col, 2.0)))) / 2.0));
        if (!Bot.Game.IsPassable(location2))
          throw new InvalidLocationException("Location is not passable!");
        if (location1.get_Col() != location2.get_Col() || location1.get_Row() != location2.get_Row())
          list.Add(location2);
      }
      return list;
    }

    private Location FindCenter(bool enforcePirate)
    {
      List<Pirate> list = this.Pirates.ConvertAll<Pirate>((Converter<int, Pirate>) (p => Bot.Game.GetMyPirate(p)));
      Location location1 = new Location(0, 0);
      using (List<Pirate>.Enumerator enumerator = list.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          Pirate current = enumerator.Current;
          Location location2 = location1;
          int num1 = location2.get_Row() + current.get_Loc().get_Row();
          location2.set_Row(num1);
          Location location3 = location1;
          int num2 = location3.get_Col() + current.get_Loc().get_Col();
          location3.set_Col(num2);
        }
      }
      Location location4 = location1;
      int num3 = location4.get_Col() / list.Count;
      location4.set_Col(num3);
      Location location5 = location1;
      int num4 = location5.get_Row() / list.Count;
      location5.set_Row(num4);
      if (enforcePirate)
      {
        int num1 = Bot.Game.GetCols() + Bot.Game.GetCols();
        Pirate pirate = (Pirate) null;
        using (List<Pirate>.Enumerator enumerator = list.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            Pirate current = enumerator.Current;
            int num2 = Bot.Game.Distance(location1, current.get_Loc());
            if (num2 < num1)
            {
              num1 = num2;
              pirate = current;
            }
          }
        }
        location1 = pirate.get_Loc();
      }
      return location1;
    }

    public int LiveCount()
    {
      return Enumerable.Count<Pirate>((IEnumerable<Pirate>) this.Pirates.ConvertAll<Pirate>((Converter<int, Pirate>) (p => Bot.Game.GetMyPirate(p))), (Func<Pirate, bool>) (p => !p.get_IsLost()));
    }

    public void CalcPriorities()
    {
      List<ITarget> list1 = new List<ITarget>();
      List<Score> list2 = new List<Score>();
      list1.AddRange((IEnumerable<ITarget>) SmartIsland.IslandList);
      foreach (ITarget target in list1)
      {
        Score score = target.GetScore(this);
        if (score != null)
          list2.Add(score);
      }
      this.Priorities = list2;
      if (this.Priorities.Count != 0)
        return;
      this.Priorities.Add(new NoTarget().GetScore(this));
    }

    public void AddPirate(int index)
    {
      foreach (Group group in Commander.Groups)
        group.Pirates.Remove(index);
      this.Pirates.Add(index);
    }
  }
}
