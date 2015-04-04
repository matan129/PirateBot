// Decompiled with JetBrains decompiler
// Type: Britbot.Commander
// Assembly: dxz2zlat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4B5F765-3404-42F9-8DF1-AF1C46E25CE3
// Assembly location: C:\Users\Matan\AppData\Local\Temp\dxz2zlat.dll

using Pirates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Britbot
{
  public static class Commander
  {
    private static bool _initFlag = false;

    public static List<Group> Groups { get; private set; }

    public static void Init()
    {
      if (Commander._initFlag)
        return;
      Commander._initFlag = true;
      try
      {
        Bot.Game.Debug("We have {0} pirates in our forces! \n", new object[1]
        {
          (object) Bot.Game.AllMyPirates().Count
        });
        Commander.Groups = new List<Group>();
        if (Bot.Game.Islands().Count == 1)
        {
          Commander.Groups.Add(new Group(0, Bot.Game.AllMyPirates().Count));
        }
        else
        {
          switch (Bot.Game.AllMyPirates().Count)
          {
            case 3:
              Commander.Groups.Add(new Group(0, 2));
              Commander.Groups.Add(new Group(2, 1));
              break;
            case 4:
              if (Bot.Game.AllEnemyPirates().Count > 4)
              {
                Commander.Groups.Add(new Group(0, 1));
                Commander.Groups.Add(new Group(1, 1));
                Commander.Groups.Add(new Group(2, 2));
                break;
              }
              Commander.Groups.Add(new Group(0, 3));
              Commander.Groups.Add(new Group(3, 1));
              break;
            case 5:
              Commander.Groups.Add(new Group(0, 2));
              Commander.Groups.Add(new Group(2, 2));
              Commander.Groups.Add(new Group(4, 1));
              break;
            case 6:
              Commander.Groups.Add(new Group(0, 3));
              Commander.Groups.Add(new Group(3, 2));
              Commander.Groups.Add(new Group(3, 1));
              break;
            case 7:
              Commander.Groups.Add(new Group(0, 2));
              Commander.Groups.Add(new Group(2, 3));
              Commander.Groups.Add(new Group(5, 2));
              break;
            case 8:
              if (Bot.Game.GetMyPirate(7).get_Loc().get_Row() == 39)
              {
                Commander.Groups.Add(new Group(0, 4));
                Commander.Groups.Add(new Group(4, 3));
                Commander.Groups.Add(new Group(7, 1));
                break;
              }
              Commander.Groups.Add(new Group(0, 3));
              Commander.Groups.Add(new Group(3, 2));
              Commander.Groups.Add(new Group(5, 2));
              Commander.Groups.Add(new Group(7, 1));
              break;
            case 9:
              Commander.Groups.Add(new Group(0, 3));
              Commander.Groups.Add(new Group(3, 3));
              Commander.Groups.Add(new Group(6, 2));
              Commander.Groups.Add(new Group(8, 1));
              Commander.Groups.Add(new Group(0, 9));
              break;
            default:
              for (int index = 0; index < Bot.Game.AllMyPirates().Count; ++index)
                Commander.Groups.Add(new Group(index, 1));
              break;
          }
        }
      }
      catch (Exception ex)
      {
        Bot.Game.Debug("==========COMMANDER EXCEPTION============");
        Bot.Game.Debug("Commander almost crashed because of exception: " + ex.Message);
        Bot.Game.Debug("==========COMMANDER EXCEPTION============");
      }
    }

    public static void CalculateAndAssignTargets()
    {
      Commander.StartCalcPriorities();
      int[] targetsDimensions = Commander.GetTargetsDimensions();
      Score[][] possibleTargetMatrix = Commander.GetPossibleTargetMatrix();
      int[] assignment = new int[targetsDimensions.Length];
      int num1 = 0;
      ExpIterator expIterator = new ExpIterator(targetsDimensions);
      Score[] scoreArray = new Score[targetsDimensions.Length];
      do
      {
        int num2 = (int) Commander.GlobalizeScore(Commander.GetSpeciphicAssignmentScores(possibleTargetMatrix, expIterator.Values));
        if (num2 > num1)
        {
          num1 = num2;
          Array.Copy((Array) expIterator.Values, (Array) assignment, expIterator.Values.Length);
        }
      }
      while (expIterator.NextIteration());
      Score[] assignmentScores = Commander.GetSpeciphicAssignmentScores(possibleTargetMatrix, assignment);
      for (int index = 0; index < targetsDimensions.Length; ++index)
        Commander.Groups[index].SetTarget(assignmentScores[index].Target);
      Bot.Game.Debug("=====================TARGETS===================");
      for (int index = 0; index < targetsDimensions.Length; ++index)
        Bot.Game.Debug(possibleTargetMatrix[index][assignment[index]].Target.GetDescription());
      Bot.Game.Debug("=====================TARGETS===================");
    }

    public static void DistributeForces(int[] config)
    {
      throw new NotImplementedException();
    }

    public static List<int> GetUltimateGameConfig()
    {
      int[] array = Enemy.Groups.ConvertAll<int>((Converter<EnemyGroup, int>) (group => group.EnemyPirates.Count)).ToArray();
      Array.Sort<int>(array, (Comparison<int>) ((a, b) => a.CompareTo(b)));
      List<int> list1 = new List<int>();
      int count = Bot.Game.AllMyPirates().Count;
      for (int index = 0; index < array.Length && count > 0 && array[index] + 1 <= count; ++index)
      {
        list1.Add(array[index] + 1);
        count -= array[index] + 1;
      }
      for (; count > 0; --count)
        list1.Add(1);
      while (list1.Count > Bot.Game.Islands().Count)
      {
        List<int> list2;
        int index;
        (list2 = list1)[index = list1.Count - 2] = list2[index] + Enumerable.Last<int>((IEnumerable<int>) list1);
        list1.RemoveAt(list1.Count - 1);
      }
      return list1;
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
          num1 += 200.0 * score.Value;
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
      return num1 * (double) scoreArr.Length / (num2 / (double) scoreArr.Length);
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

    public static Dictionary<Pirate, Direction> Play()
    {
      try
      {
        Enemy.Update();
        Commander.CalculateAndAssignTargets();
        return Commander.GetAllMoves();
      }
      catch (Exception ex)
      {
        Bot.Game.Debug("==========COMMANDER EXCEPTION============");
        Bot.Game.Debug("Commander almost crashed because of exception: " + ex.Message);
        Bot.Game.Debug("==========COMMANDER EXCEPTION============");
        return new Dictionary<Pirate, Direction>();
      }
    }

    private static Dictionary<Pirate, Direction> GetAllMoves()
    {
      List<KeyValuePair<Pirate, Direction>> list = new List<KeyValuePair<Pirate, Direction>>(Bot.Game.AllMyPirates().Count);
      foreach (Group group in Commander.Groups)
        list.AddRange(group.GetGroupMoves());
      return Enumerable.ToDictionary<KeyValuePair<Pirate, Direction>, Pirate, Direction>((IEnumerable<KeyValuePair<Pirate, Direction>>) list, (Func<KeyValuePair<Pirate, Direction>, Pirate>) (pair => pair.Key), (Func<KeyValuePair<Pirate, Direction>, Direction>) (pair => pair.Value));
    }

    private static Score[][] GetPossibleTargetMatrix()
    {
      Score[][] scoreArray = new Score[Commander.Groups.Count][];
      for (int index = 0; index < Commander.Groups.Count; ++index)
        scoreArray[index] = Commander.Groups[index].Priorities.ToArray();
      return scoreArray;
    }

    private static Score[] GetSpeciphicAssignmentScores(Score[][] possibleAssignments, int[] assignment)
    {
      Score[] scoreArray = new Score[possibleAssignments.Length];
      for (int index = 0; index < scoreArray.Length; ++index)
        scoreArray[index] = possibleAssignments[index][assignment[index]];
      return scoreArray;
    }

    private static int[] GetTargetsDimensions()
    {
      int[] numArray = new int[Commander.Groups.Count];
      for (int index = 0; index < Commander.Groups.Count; ++index)
        numArray[index] = Commander.Groups[index].Priorities.Count;
      return numArray;
    }

    private static void StartCalcPriorities()
    {
      foreach (Group group in Commander.Groups)
        group.CalcPriorities();
    }
  }
}
