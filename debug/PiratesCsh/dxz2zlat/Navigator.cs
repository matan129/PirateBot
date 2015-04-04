// Decompiled with JetBrains decompiler
// Type: Britbot.Navigator
// Assembly: dxz2zlat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F4B5F765-3404-42F9-8DF1-AF1C46E25CE3
// Assembly location: C:\Users\Matan\AppData\Local\Temp\dxz2zlat.dll

using Pirates;
using System;
using System.Collections.Generic;

namespace Britbot
{
  internal class Navigator
  {
    public static Direction CalculateDirectionToStationeryTarget(Location myLoc, HeadingVector myHeading, Location target)
    {
      HeadingVector headingVector = HeadingVector.CalcDifference(myLoc, target);
      Direction direction = (Direction) 45;
      double num1 = -1.0;
      using (List<Direction>.Enumerator enumerator = Bot.Game.GetDirections(myLoc, target).GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          Direction current = enumerator.Current;
          double num2 = HeadingVector.adjustHeading(myHeading, current).Normalize() * headingVector.Normalize();
          if (num2 > num1)
          {
            direction = current;
            num1 = num2;
          }
        }
      }
      return direction;
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
      HeadingVector headingVector = HeadingVector.CalcDifference(point, linePoint);
      double num = dir.Normalize() * headingVector;
      return (headingVector - num * dir).Norm1();
    }

    public int ComparePirateByDirection(int p1, int p2, HeadingVector hv)
    {
      double num = Navigator.CalcDistFromLine(new Location(0, 0), Bot.Game.GetMyPirate(p1).get_Loc(), hv.Orthogonal());
      return (int) (Navigator.CalcDistFromLine(new Location(0, 0), Bot.Game.GetMyPirate(p2).get_Loc(), hv.Orthogonal()) - num);
    }

    public static bool IsReachable(Location group, Location target, HeadingVector targetHeading)
    {
      HeadingVector headingVector = HeadingVector.CalcDifference(target, group);
      double num1 = headingVector.Normalize() * targetHeading.Normalize();
      double num2 = headingVector.Norm() * num1;
      Location location = HeadingVector.AddvanceByVector(target, num2 * targetHeading.Normalize());
      return Bot.Game.Distance(group, location) < Bot.Game.Distance(target, location) - 2;
    }
  }
}
