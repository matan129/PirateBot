// Decompiled with JetBrains decompiler
// Type: Britbot.Commander
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

using Pirates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Britbot
{
  public static class Commander
  {
    public static int MaxIterator = 10000;

    public static List<Group> Groups { get; set; }

    static Commander()
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
            if (Bot.Game.EnemyIslands().Count > 0)
            {
              Commander.Groups.Add(new Group(0, 5));
              Commander.Groups.Add(new Group(5, 1));
              break;
            }
            Commander.Groups.Add(new Group(2, 4));
            Commander.Groups.Add(new Group(0, 1));
            Commander.Groups.Add(new Group(1, 1));
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

    public static int CalcMaxPrioritiesNum()
    {
      return (int) Math.Pow((double) Commander.MaxIterator, 1.0 / (double) Commander.Groups.Count);
    }

    public static void CalculateAndAssignTargets(CancellationToken cancellationToken)
    {
      Commander.StartCalcPriorities(cancellationToken);
      int[] targetsDimensions = Commander.GetTargetsDimensions();
      Score[][] possibleTargetMatrix = Commander.GetPossibleTargetMatrix();
      int[] assignment = new int[targetsDimensions.Length];
      double num1 = -9999999999999.0;
      ExpIterator expIterator = new ExpIterator(targetsDimensions);
      Score[] scoreArray = new Score[targetsDimensions.Length];
      do
      {
        cancellationToken.ThrowIfCancellationRequested();
        double num2 = Commander.GlobalizeScore(Commander.GetSpecificAssignmentScores(possibleTargetMatrix, expIterator.Values), cancellationToken);
        if (num2 > num1)
        {
          num1 = num2;
          Array.Copy((Array) expIterator.Values, (Array) assignment, expIterator.Values.Length);
        }
      }
      while (expIterator.NextIteration());
      Score[] assignmentScores = Commander.GetSpecificAssignmentScores(possibleTargetMatrix, assignment);
      for (int index = 0; index < targetsDimensions.Length; ++index)
        Commander.Groups[index].SetTarget(assignmentScores[index].Target);
      Bot.Game.Debug("Max score: " + (object) num1);
      Bot.Game.Debug("maxx ass: " + string.Join<int>(", ", (IEnumerable<int>) assignment));
      Bot.Game.Debug("dimention: " + string.Join<int>(", ", (IEnumerable<int>) expIterator.Dimensions));
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

    public static double GlobalizeScore(Score[] scoreArr, CancellationToken cancellationToken)
    {
      double num1 = 0.0;
      double num2 = 0.0;
      foreach (Score score in scoreArr)
      {
        cancellationToken.ThrowIfCancellationRequested();
        num1 = num1 + Math.Pow(2.0, (double) Bot.Game.MyIslands().Count + score.Value) + 0.2 * Math.Pow(3.0, score.EnemyShips);
        num2 += score.Eta;
      }
      for (int index1 = 0; index1 < scoreArr.Length - 1; ++index1)
      {
        cancellationToken.ThrowIfCancellationRequested();
        for (int index2 = index1 + 1; index2 < scoreArr.Length; ++index2)
        {
          cancellationToken.ThrowIfCancellationRequested();
          if (scoreArr[index1].Target.Equals(scoreArr[index2].Target))
            num1 -= 100.0;
        }
      }
      return num1 - num2 / (double) scoreArr.Length;
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

    public static Dictionary<Pirate, Direction> Play(CancellationToken cancellationToken, out bool onTime)
    {
      try
      {
        Enemy.Update(cancellationToken);
        Bot.Game.Debug("Alowed Targets: " + (object) Commander.CalcMaxPrioritiesNum());
        Commander.CalculateAndAssignTargets(cancellationToken);
        Bot.Game.Debug("-------------priorities--------------");
        foreach (Group group in Commander.Groups)
        {
          Bot.Game.Debug("Group " + (object) group.Id);
          foreach (object obj in group.Priorities)
            Bot.Game.Debug(obj.ToString());
        }
        foreach (Group group in Commander.Groups)
          group.Debug();
        Dictionary<Pirate, Direction> allMoves = Commander.GetAllMoves(cancellationToken);
        onTime = true;
        return allMoves;
      }
      catch (AggregateException ex)
      {
        Bot.Game.Debug("****** COMMANDER EXITING DUE TO AggregateException ******");
        foreach (object obj in ex.InnerExceptions)
          Bot.Game.Debug(obj.ToString());
        onTime = false;
        throw;
      }
      catch (OperationCanceledException ex)
      {
        Bot.Game.Debug("****** COMMANDER EXITING DUE TO TASK CANCELLATION ******");
        onTime = false;
        throw;
      }
      catch (Exception ex)
      {
        Bot.Game.Debug("==========COMMANDER EXCEPTION============");
        Bot.Game.Debug("Commander almost crashed because of exception: " + ex.Message);
        StackFrame frame = new StackTrace(ex, true).GetFrame(0);
        Bot.Game.Debug("The exception was thrown from method {0} at file {1} at line #{2}", new object[3]
        {
          (object) frame.GetMethod(),
          (object) frame.GetFileName(),
          (object) frame.GetFileLineNumber()
        });
        Bot.Game.Debug("==========COMMANDER EXCEPTION============");
        onTime = false;
        throw;
      }
    }

    private static Dictionary<Pirate, Direction> GetAllMoves(CancellationToken cancellationToken)
    {
      List<KeyValuePair<Pirate, Direction>> list = new List<KeyValuePair<Pirate, Direction>>(Bot.Game.AllMyPirates().Count);
      foreach (Group group in Commander.Groups)
        list.AddRange(group.GetGroupMoves(cancellationToken));
      return Enumerable.ToDictionary<KeyValuePair<Pirate, Direction>, Pirate, Direction>((IEnumerable<KeyValuePair<Pirate, Direction>>) list, (Func<KeyValuePair<Pirate, Direction>, Pirate>) (pair => pair.Key), (Func<KeyValuePair<Pirate, Direction>, Direction>) (pair => pair.Value));
    }

    private static Score[][] GetPossibleTargetMatrix()
    {
      Score[][] scoreArray = new Score[Commander.Groups.Count][];
      for (int index = 0; index < Commander.Groups.Count; ++index)
        scoreArray[index] = Commander.Groups[index].Priorities.ToArray();
      return scoreArray;
    }

    private static Score[] GetSpecificAssignmentScores(Score[][] possibleAssignments, int[] assignment)
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

    private static void StartCalcPriorities(CancellationToken cancellationToken)
    {
      Parallel.ForEach<Group>((IEnumerable<Group>) Commander.Groups, (Action<Group>) (g => g.CalcPriorities(cancellationToken)));
      Bot.Game.Debug("Priorities Calculated");
    }
  }
}
