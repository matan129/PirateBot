// Decompiled with JetBrains decompiler
// Type: Britbot.Enemy
// Assembly: o5uha11d, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FD1E92EE-5B89-4A73-A517-70BE2E057BAD
// Assembly location: C:\Users\Matan\AppData\Local\Temp\o5uha11d.dll

using Pirates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Britbot
{
  public static class Enemy
  {
    public static List<EnemyGroup> Groups { get; private set; }

    static Enemy()
    {
      Enemy.Groups = new List<EnemyGroup>();
    }

    public static List<EnemyGroup> AnalyzeEnemyGroups()
    {
      List<EnemyGroup> list1 = new List<EnemyGroup>();
      using (IEnumerator<Pirate> enumerator = Enumerable.Where<Pirate>((IEnumerable<Pirate>) Bot.Game.AllEnemyPirates(), (Func<Pirate, bool>) (p => !p.get_IsLost())).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          Pirate pete = enumerator.Current;
          EnemyGroup enemyGroup1 = new EnemyGroup();
          enemyGroup1.EnemyPirates.Add(pete.get_Id());
          List<EnemyGroup> list2 = Enumerable.ToList<EnemyGroup>(Enumerable.Where<EnemyGroup>((IEnumerable<EnemyGroup>) list1, (Func<EnemyGroup, bool>) (g => g.IsInGroup(pete.get_Id()))));
          if (list2.Count > 0)
          {
            list1.RemoveAll((Predicate<EnemyGroup>) (g => g.IsInGroup(pete.get_Id())));
            foreach (EnemyGroup enemyGroup2 in list2)
              enemyGroup1.EnemyPirates.AddRange((IEnumerable<int>) enemyGroup2.EnemyPirates);
          }
          enemyGroup1.PrevLoc = enemyGroup1.GetLocation();
          list1.Add(enemyGroup1);
        }
      }
      Bot.Game.Debug("Enemy Configuration: " + string.Join<EnemyGroup>(",", (IEnumerable<EnemyGroup>) list1));
      return list1;
    }

    public static void Update()
    {
      List<EnemyGroup> list = Enemy.AnalyzeEnemyGroups();
      Enemy.Groups = Enumerable.ToList<EnemyGroup>(Enumerable.Intersect<EnemyGroup>((IEnumerable<EnemyGroup>) Enemy.Groups, (IEnumerable<EnemyGroup>) list));
      Enemy.Groups = Enumerable.ToList<EnemyGroup>(Enumerable.Union<EnemyGroup>((IEnumerable<EnemyGroup>) Enemy.Groups, (IEnumerable<EnemyGroup>) list));
      foreach (EnemyGroup enemyGroup in Enemy.Groups)
        enemyGroup.UpdateHeading();
    }

    public static int[] GetConfig()
    {
      int[] numArray = new int[Enemy.Groups.Count];
      for (int index = 0; index < Enemy.Groups.Count; ++index)
        numArray[index] = Enemy.Groups[index].EnemyPirates.Count;
      return numArray;
    }
  }
}
