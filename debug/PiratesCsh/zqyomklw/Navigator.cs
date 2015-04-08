// Decompiled with JetBrains decompiler
// Type: Britbot.Navigator
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

using Britbot.PriorityQueue;
using Pirates;
using System;
using System.Collections.Generic;

namespace Britbot
{
  internal static class Navigator
  {
    public static Direction CalculateDirectionToStationeryTarget(Location myLoc, HeadingVector myHeading, Location target)
    {
      return Navigator.CalculatePath(myLoc, target);
    }

    public static Direction CalculateDirectionToMovingTarget(Location myLoc, HeadingVector myHeading, Location target, HeadingVector targetHeading)
    {
      double num = Navigator.SolveStupidEquation(targetHeading.Norm1(), (double) (target.get_Col() - myLoc.get_Col()), targetHeading.X, (double) (target.get_Row() - myLoc.get_Row()), targetHeading.Y);
      Location target1 = HeadingVector.AddvanceByVector(target, num * targetHeading);
      return Navigator.CalculateDirectionToStationeryTarget(myLoc, myHeading, target1);
    }

    public static double SolveStupidEquation(double a, double b, double c, double d, double e)
    {
      int[] numArray = new int[2]
      {
        -1,
        1
      };
      for (int index1 = 0; index1 <= 1; ++index1)
      {
        int num1 = numArray[index1];
        for (int index2 = 0; index2 <= 1; ++index2)
        {
          int num2 = numArray[index2];
          double num3 = -((double) num1 * b + (double) num2 * d) / (a + (double) num1 * c + (double) num2 * e);
          if (num3 > 0.0 && ((double) num1 * c * num3 <= (double) -num1 * b && (double) num2 * e * num3 <= (double) -num2 * d))
            return num3;
        }
      }
      throw new Exception("Matan K is an idiot");
    }

    public static double CalcDistFromLine(Location point, Location linePoint, HeadingVector dir)
    {
      if (dir.NormSquared() == 0.0)
        return (double) Bot.Game.Distance(point, linePoint);
      HeadingVector headingVector = HeadingVector.CalcDifference(point, linePoint);
      double num = dir.Normalize() * headingVector;
      return (headingVector - num * dir).Norm1();
    }

    public static int ComparePirateByDirection(int p1, int p2, HeadingVector hv)
    {
      double num = Navigator.CalcDistFromLine(new Location(0, 0), Bot.Game.GetMyPirate(p1).get_Loc(), hv.Orthogonal());
      return (int) (Navigator.CalcDistFromLine(new Location(0, 0), Bot.Game.GetMyPirate(p2).get_Loc(), hv.Orthogonal()) - num);
    }

    public static bool IsReachable(Location group, Location target, HeadingVector targetHeading)
    {
      if (targetHeading.Norm() == 0.0)
        return true;
      if (HeadingVector.CalcDifference(group, target) * targetHeading > 0.0)
        return false;
      HeadingVector headingVector = HeadingVector.CalcDifference(target, group);
      double num1 = headingVector.Normalize() * targetHeading.Normalize();
      double num2 = headingVector.Norm() * num1;
      Location location = HeadingVector.AddvanceByVector(target, num2 * targetHeading.Normalize());
      return Bot.Game.Distance(group, location) < Bot.Game.Distance(target, location) - 2;
    }

    public static Direction CalculatePath(Location start, Location target)
    {
      Navigator.Node.SetUpCalculation(target);
      HeapPriorityQueue<Navigator.Node> heapPriorityQueue = new HeapPriorityQueue<Navigator.Node>(Bot.Game.GetCols() + Bot.Game.GetRows());
      Navigator.Node locationNodeFromMap = Navigator.Node.GetLocationNodeFromMap(start);
      heapPriorityQueue.Enqueue(locationNodeFromMap, locationNodeFromMap.F());
      while (heapPriorityQueue.Count > 0)
      {
        Navigator.Node node1 = heapPriorityQueue.Dequeue();
        if (!node1.Equals((object) target))
        {
          node1.IsEvaluated = true;
          foreach (Navigator.Node node2 in node1.GetNeighbors())
          {
            if (!node2.IsEvaluated)
            {
              double num = node1.G + node2.Weight;
              if (!heapPriorityQueue.Contains(node2) || node2.G == -1.0 || num < node2.G)
              {
                node2.G = num;
                if (!heapPriorityQueue.Contains(node2))
                  heapPriorityQueue.Enqueue(node2, node2.F());
              }
            }
          }
        }
        else
          break;
      }
      Navigator.Node node3 = (Navigator.Node) null;
      foreach (Navigator.Node node1 in locationNodeFromMap.GetNeighbors())
      {
        if (node3 == null)
          node3 = node1;
        else if (node1.F() < node3.F())
          node3 = node1;
      }
      if (node3 == null)
        throw new Exception("Matan K is stupid as shit, please go and tell him that");
      return Bot.Game.GetDirections(locationNodeFromMap.Loc, node3.Loc)[0];
    }

    public static void UpdateMap(int groupStrength)
    {
      Navigator.Node.UpdateMap(groupStrength);
    }

    internal class Node : PriorityQueueNode
    {
      public static Navigator.Node[,] Map = new Navigator.Node[Bot.Game.GetRows(), Bot.Game.GetCols()];
      public const double Infinity = 1025583.0;
      public double G;
      public double H;
      public bool IsEvaluated;
      public bool IsTraveled;
      public Location Loc;
      public double Weight;

      public static void UpdateMap(int groupStrength)
      {
        int cols = Bot.Game.GetCols();
        int rows = Bot.Game.GetRows();
        int ringCount = Group.GetRingCount(groupStrength);
        for (int index1 = 0; index1 < cols; ++index1)
        {
          for (int index2 = 0; index2 < rows; ++index2)
          {
            Navigator.Node.Map[index2, index1].Loc = new Location(index2, index1);
            if (!Extensions.IsPassableEnough(Bot.Game, Navigator.Node.Map[index2, index1].Loc, ringCount))
              Navigator.Node.Map[index2, index1].Weight = 1025583.0;
            else
              Navigator.Node.CalcEnemyFactor(Navigator.Node.Map[index2, index1].Loc, groupStrength);
          }
        }
      }

      public static void SetUpCalculation(Location target)
      {
        for (int index1 = 0; index1 < Bot.Game.GetRows(); ++index1)
        {
          for (int index2 = 0; index2 < Bot.Game.GetCols(); ++index2)
          {
            Navigator.Node.Map[index1, index2].H = (double) Bot.Game.Distance(Navigator.Node.Map[index1, index2].Loc, target);
            Navigator.Node.Map[index1, index2].G = -1.0;
            Navigator.Node.Map[index1, index2].IsTraveled = false;
            Navigator.Node.Map[index1, index2].IsEvaluated = false;
          }
        }
      }

      private static double CalcEnemyFactor(Location loc, int GroupStrength)
      {
        double num1 = 9.0 * (double) Bot.Game.GetAttackRadius();
        double num2 = 2.0;
        double num3 = 0.0;
        int num4 = 0;
        foreach (EnemyGroup enemyGroup in Enemy.Groups)
        {
          double num5 = enemyGroup.MinimalSquaredDistanceTo(loc);
          int count = enemyGroup.EnemyPirates.Count;
          if (num5 <= num1)
          {
            num4 += count;
            double num6 = 9.75055163745889E-07 + Math.Max(0.0, num5 - (double) Bot.Game.GetAttackRadius()) / (1025583.0 * (num1 - (double) Bot.Game.GetAttackRadius()));
            if (count - 1 > GroupStrength)
              num2 += 0.5 * (double) count / num6;
            else
              num3 += 0.5 * (double) count / num6;
          }
        }
        if (num4 - 1 > GroupStrength)
          return 1.0;
        return num2 + num3;
      }

      public static Navigator.Node GetLocationNodeFromMap(Location loc)
      {
        return Navigator.Node.Map[loc.get_Row(), loc.get_Col()];
      }

      public double F()
      {
        return this.H + this.G;
      }

      public bool IsPassable()
      {
        return this.Weight < 1025583.0;
      }

      public List<Navigator.Node> GetNeighbors()
      {
        List<Navigator.Node> list = new List<Navigator.Node>();
        int[] numArray = new int[2]
        {
          -1,
          1
        };
        int col = this.Loc.get_Col();
        int row = this.Loc.get_Row();
        for (int index1 = 0; index1 < 2; ++index1)
        {
          for (int index2 = 0; index2 < 2; ++index2)
          {
            int index3 = col + numArray[index2];
            int index4 = row + numArray[index1];
            if (index3 >= 0 && index3 < Bot.Game.GetCols() && index4 >= 0 && index4 < Bot.Game.GetRows() && Navigator.Node.Map[index4, index3].IsPassable())
              list.Add(Navigator.Node.Map[index4, index3]);
          }
        }
        return list;
      }

      public override bool Equals(object obj)
      {
        if (obj.GetType() == this.GetType())
          return this.Loc == ((Navigator.Node) obj).Loc;
        return false;
      }
    }
  }
}
