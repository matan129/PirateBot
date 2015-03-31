// Decompiled with JetBrains decompiler
// Type: Britbot.Commander
// Assembly: 0ztn0bu3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAE66A6C-F769-4197-8684-0F1222C47342
// Assembly location: C:\Users\Matan\AppData\Local\Temp\0ztn0bu3.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Britbot
{
  public static class Commander
  {
    public static List<Group> Groups { get; private set; }

    static Commander()
    {
      Bot.Game.Debug("We have {0} pirates in our forces! \n", new object[1]
      {
        (object) Bot.Game.AllMyPirates().Count
      });
      Commander.Groups = new List<Group>();
      switch (Bot.Game.AllMyPirates().Count)
      {
        case 3:
          Commander.Groups.Add(new Group(0, 2));
          Commander.Groups.Add(new Group(2, 1));
          break;
        case 4:
          Commander.Groups.Add(new Group(0, 3));
          Commander.Groups.Add(new Group(3, 1));
          break;
        case 5:
          Commander.Groups.Add(new Group(0, 3));
          Commander.Groups.Add(new Group(3, 2));
          break;
        case 6:
          if (Bot.Game.EnemyIslands().Count > 0)
          {
            Commander.Groups.Add(new Group(0, 1));
            Commander.Groups.Add(new Group(1, 1));
            Commander.Groups.Add(new Group(2, 1));
            Commander.Groups.Add(new Group(3, 1));
            Commander.Groups.Add(new Group(4, 1));
            Commander.Groups.Add(new Group(5, 1));
            break;
          }
          Commander.Groups.Add(new Group(0, 1));
          Commander.Groups.Add(new Group(1, 3));
          Commander.Groups.Add(new Group(4, 2));
          break;
      }
    }

    public static void Play()
    {
      Enemy.Update();
      Commander.AssignTargets();
      Commander.MoveForces();
    }

    public static void DistributeForces(int[] config)
    {
      throw new NotImplementedException();
    }

    public static bool IsEmployed(int pirate)
    {
      foreach (Group group in Commander.Groups)
      {
        if (group.Pirates.Contains(pirate))
          return true;
      }
      return false;
    }

    public static void AssignTargets()
    {
      Commander.StartCalcPriorities();
      int[] targetsDimensions = Commander.GetTargetsDimensions();
      Score[][] possibleTargetMatrix = Commander.GetPossibleTargetMatrix();
      int[] numArray = new int[targetsDimensions.Length];
      int num1 = 0;
      ExpIterator expIterator = new ExpIterator(targetsDimensions);
      Score[] scoreArr = new Score[targetsDimensions.Length];
      do
      {
        for (int index = 0; index < targetsDimensions.Length; ++index)
          scoreArr[index] = possibleTargetMatrix[index][expIterator.Values[index]];
        int num2 = (int) Commander.GlobalizeScore(scoreArr);
        if (num2 > num1)
        {
          num1 = num2;
          for (int index = 0; index < numArray.Length; ++index)
            numArray[index] = expIterator.Values[index];
        }
      }
      while (expIterator.NextIteration());
      for (int index = 0; index < targetsDimensions.Length; ++index)
        Commander.Groups[index].SetTarget(possibleTargetMatrix[index][numArray[index]].Target);
      Bot.Game.Debug("----------TARGETS--------------");
      for (int index = 0; index < targetsDimensions.Length; ++index)
        Bot.Game.Debug(possibleTargetMatrix[index][numArray[index]].Target.ToS());
      Bot.Game.Debug("----------TARGETS--------------");
    }

    public static double GlobalizeScore(Score[] scoreArr)
    {
      double num1 = 0.0;
      double num2 = 0.0;
      foreach (Score score in scoreArr)
      {
        if (score.Type == TargetType.Island)
          num1 += 100.0 * score.Value;
        else if (score.Type == TargetType.EnemyGroup)
          num1 += 2000000.0 * score.Value;
        num2 += score.Eta;
      }
      for (int index1 = 0; index1 < Enumerable.Count<Score>((IEnumerable<Score>) scoreArr) - 1; ++index1)
      {
        for (int index2 = index1 + 1; index2 < scoreArr.Length; ++index2)
        {
          if (scoreArr[index1].Target.Equals(scoreArr[index2].Target))
            num1 -= 100000000.0;
        }
      }
      return num1 * (double) scoreArr.Length / num2;
    }

    private static void StartCalcPriorities()
    {
      foreach (Group group in Commander.Groups)
        group.CalcPriorities();
    }

    private static Score[][] GetPossibleTargetMatrix()
    {
      Score[][] scoreArray = new Score[Commander.Groups.Count][];
      for (int index = 0; index < Commander.Groups.Count; ++index)
        scoreArray[index] = Commander.Groups[index].Priorities.ToArray();
      return scoreArray;
    }

    private static int[] GetTargetsDimensions()
    {
      int[] numArray = new int[Commander.Groups.Count];
      for (int index = 0; index < Commander.Groups.Count; ++index)
        numArray[index] = Commander.Groups[index].Priorities.Count;
      return numArray;
    }

    private static void MoveForces()
    {
      foreach (Group group in Commander.Groups)
        group.Move();
    }
  }
}
