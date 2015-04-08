// Decompiled with JetBrains decompiler
// Type: Britbot.Enemy
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

using Pirates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Britbot
{
  public static class Enemy
  {
    public static List<EnemyGroup> Groups { get; private set; }

    static Enemy()
    {
      Enemy.Groups = new List<EnemyGroup>();
    }

    public static List<EnemyGroup> AnalyzeEnemyGroups(CancellationToken cancellationToken)
    {
      EnemyGroup[] enemyGroupArray = Enemy.AnalyzeFull(cancellationToken).ToArray();
      if (Enemy.Groups.Count == 0)
        return Enumerable.ToList<EnemyGroup>((IEnumerable<EnemyGroup>) enemyGroupArray);
      List<EnemyGroup> list = new List<EnemyGroup>(enemyGroupArray.Length);
      bool[] flagArray = new bool[enemyGroupArray.Length];
      for (int index = 0; index < enemyGroupArray.Length; ++index)
      {
        cancellationToken.ThrowIfCancellationRequested();
        EnemyGroup enemyGroup1 = enemyGroupArray[index];
        foreach (EnemyGroup enemyGroup2 in Enemy.Groups)
        {
          cancellationToken.ThrowIfCancellationRequested();
          if (object.Equals((object) enemyGroup2, (object) enemyGroup1))
          {
            list.Add(enemyGroup2);
            flagArray[index] = true;
            break;
          }
        }
      }
      string str = "";
      for (int index = 0; index < enemyGroupArray.Length; ++index)
      {
        cancellationToken.ThrowIfCancellationRequested();
        if (!flagArray[index])
        {
          list.Add(enemyGroupArray[index]);
          str = str + (object) enemyGroupArray[index] + ",";
        }
      }
      return list;
    }

    private static List<EnemyGroup> AnalyzeFull(CancellationToken cancellationToken)
    {
      List<EnemyGroup> list1 = new List<EnemyGroup>();
      using (IEnumerator<Pirate> enumerator = Enumerable.Where<Pirate>((IEnumerable<Pirate>) Bot.Game.AllEnemyPirates(), (Func<Pirate, bool>) (p => !p.get_IsLost())).GetEnumerator())
      {
        while (((IEnumerator) enumerator).MoveNext())
        {
          Pirate pete = enumerator.Current;
          cancellationToken.ThrowIfCancellationRequested();
          EnemyGroup enemyGroup1 = new EnemyGroup();
          enemyGroup1.EnemyPirates.Add(pete.get_Id());
          List<EnemyGroup> list2 = Enumerable.ToList<EnemyGroup>(Enumerable.Where<EnemyGroup>((IEnumerable<EnemyGroup>) list1, (Func<EnemyGroup, bool>) (g => g.IsInGroup(pete.get_Id()))));
          if (list2.Count > 0)
          {
            list1.RemoveAll((Predicate<EnemyGroup>) (g => g.IsInGroup(pete.get_Id())));
            foreach (EnemyGroup enemyGroup2 in list2)
            {
              cancellationToken.ThrowIfCancellationRequested();
              enemyGroup1.EnemyPirates.AddRange((IEnumerable<int>) enemyGroup2.EnemyPirates);
            }
          }
          enemyGroup1.PrevLoc = enemyGroup1.GetLocation();
          list1.Add(enemyGroup1);
        }
      }
      return list1;
    }

    public static void Update(CancellationToken cancellationToken)
    {
      List<EnemyGroup> list = Enemy.AnalyzeEnemyGroups(cancellationToken);
      Enemy.Groups = Enumerable.ToList<EnemyGroup>(Enumerable.Intersect<EnemyGroup>((IEnumerable<EnemyGroup>) Enemy.Groups, (IEnumerable<EnemyGroup>) list));
      Enemy.Groups = Enumerable.ToList<EnemyGroup>(Enumerable.Union<EnemyGroup>((IEnumerable<EnemyGroup>) Enemy.Groups, (IEnumerable<EnemyGroup>) list));
      Parallel.ForEach<EnemyGroup>((IEnumerable<EnemyGroup>) Enemy.Groups, (Action<EnemyGroup>) (eGroup => eGroup.UpdateHeading()));
    }
  }
}
