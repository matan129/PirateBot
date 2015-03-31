// Decompiled with JetBrains decompiler
// Type: Britbot.Group
// Assembly: 0ztn0bu3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAE66A6C-F769-4197-8684-0F1222C47342
// Assembly location: C:\Users\Matan\AppData\Local\Temp\0ztn0bu3.dll

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

    public Group(int index, int amount)
    {
      this.Pirates = new List<int>();
      this.Heading = new HeadingVector(0, 0);
      this.Priorities = new List<Score>();
      this.Role = GroupRole.Destroyer;
      this.Id = Group.GroupCounter++;
      for (; amount > 0; --amount)
      {
        Bot.Game.Debug("Adding pirate at index {0} to this groups pirates", new object[1]
        {
          (object) (index + amount - 1)
        });
        this.Pirates.Add(index + amount - 1);
      }
      Bot.Game.Debug("\n");
    }

    public void SetTarget(ITarget target)
    {
      if (this.Target == null)
      {
        this.Target = target;
        this.Heading.SetCoordinates(0, 0);
      }
      else
      {
        if (object.Equals((object) this.Target, (object) target))
          return;
        this.Target = target;
        this.Heading.SetCoordinates(0, 0);
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

    public void Move()
    {
      this.Target.GetDirection(this);
      using (List<Pirate>.Enumerator enumerator = this.Pirates.ConvertAll<Pirate>((Converter<int, Pirate>) (p => Bot.Game.GetMyPirate(p))).GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          Pirate current = enumerator.Current;
          Bot.Game.SetSail(current, Enumerable.First<Direction>((IEnumerable<Direction>) Bot.Game.GetDirections(current, this.Target.GetLocation())));
        }
      }
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
      Bot.Game.Debug("Priorities Count: " + (object) this.Priorities.Count);
    }
  }
}
