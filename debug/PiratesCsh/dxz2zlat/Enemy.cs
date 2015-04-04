// Decompiled with JetBrains decompiler
// Type: Britbot.Enemy
// Assembly: dxz2zlat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4B5F765-3404-42F9-8DF1-AF1C46E25CE3
// Assembly location: C:\Users\Matan\AppData\Local\Temp\dxz2zlat.dll

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
      EnemyGroup[] enemyGroupArray = Enemy.AnalyzeFull().ToArray();
      if (Enemy.Groups.Count == 0)
        return Enumerable.ToList<EnemyGroup>((IEnumerable<EnemyGroup>) enemyGroupArray);
      List<EnemyGroup> list = new List<EnemyGroup>(enemyGroupArray.Length);
      bool[] flagArray = new bool[enemyGroupArray.Length];
      for (int index = 0; index < enemyGroupArray.Length; ++index)
      {
        EnemyGroup enemyGroup1 = enemyGroupArray[index];
        foreach (EnemyGroup enemyGroup2 in Enemy.Groups)
        {
          if (object.Equals((object) enemyGroup2, (object) enemyGroup1))
          {
            list.Add(enemyGroup2);
            flagArray[index] = true;
            break;
          }
        }
      }
      Bot.Game.Debug("Enemy veteran groups: " + string.Join<EnemyGroup>(",", (IEnumerable<EnemyGroup>) list));
      string str = "";
      for (int index = 0; index < enemyGroupArray.Length; ++index)
      {
        if (!flagArray[index])
        {
          list.Add(enemyGroupArray[index]);
          str = str + (object) enemyGroupArray[index] + ",";
        }
      }
      Bot.Game.Debug("Enemy new groups: " + str.TrimEnd(','));
      Bot.Game.Debug("Total enemy config: " + string.Join<EnemyGroup>(",", (IEnumerable<EnemyGroup>) list));
      return list;
    }

    private static List<EnemyGroup> AnalyzeFull()
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
  }
}
