// Decompiled with JetBrains decompiler
// Type: Britbot.Enemy
// Assembly: 0ztn0bu3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAE66A6C-F769-4197-8684-0F1222C47342
// Assembly location: C:\Users\Matan\AppData\Local\Temp\0ztn0bu3.dll

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
      foreach (EnemyGroup enemyGroup1 in Enemy.Groups)
      {
        List<int> list2 = new List<int>();
        do
        {
          list2.Add(enemyGroup1.EnemyPirates[0]);
          for (int index = 1; index < enemyGroup1.EnemyPirates.Count; ++index)
          {
            if (EnemyGroup.IsInGroup(list2, enemyGroup1.EnemyPirates[index]))
              list2.Add(enemyGroup1.EnemyPirates[index]);
          }
          EnemyGroup enemyGroup2 = enemyGroup1.Split(list2);
          if (enemyGroup2 != (EnemyGroup) null)
            list1.Add(enemyGroup2);
        }
        while (list2 != null);
        list1.Add(enemyGroup1);
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

    public static int[] GetConfig()
    {
      int[] numArray = new int[Enemy.Groups.Count];
      for (int index = 0; index < Enemy.Groups.Count; ++index)
        numArray[index] = Enemy.Groups[index].EnemyPirates.Count;
      return numArray;
    }
  }
}
