﻿// Decompiled with JetBrains decompiler
// Type: Britbot.HeadingVector
// Assembly: b1iry5ht, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6E84BA16-E0F0-4D91-BADE-8A0B9BF81F38
// Assembly location: C:\Users\Matan\AppData\Local\Temp\b1iry5ht.dll

using Pirates;
using System;
using System.Collections.Generic;

namespace Britbot
{
  public class HeadingVector
  {
    public int X { get; private set; }

    public int Y { get; private set; }

    private HeadingVector(Direction d = 45)
    {
      Direction direction = d;
      if (direction <= 110)
      {
        if (direction != 101)
        {
          if (direction == 110)
          {
            this.X = 0;
            this.Y = -1;
            return;
          }
        }
        else
        {
          this.X = 1;
          this.Y = 0;
          return;
        }
      }
      else if (direction != 115)
      {
        if (direction == 119)
        {
          this.X = 1;
          this.Y = 0;
          return;
        }
      }
      else
      {
        this.X = 0;
        this.Y = 1;
        return;
      }
      this.X = 0;
      this.Y = 0;
    }

    public HeadingVector(int x, int y)
    {
      this.X = x;
      this.Y = y;
    }

    public HeadingVector(Location loc)
    {
      this.X = loc.get_Col();
      this.Y = loc.get_Row();
    }

    public static int operator *(HeadingVector hv1, HeadingVector hv2)
    {
      return hv1.X * hv2.X + hv1.Y * hv2.Y;
    }

    public static HeadingVector operator +(HeadingVector hv1, Direction d)
    {
      HeadingVector headingVector1 = hv1;
      HeadingVector headingVector2 = new HeadingVector(d);
      if (hv1 * headingVector2 < 0)
      {
        headingVector1 = headingVector2;
      }
      else
      {
        headingVector1.X += headingVector2.X;
        headingVector1.Y += headingVector2.Y;
      }
      return headingVector1;
    }

    public static HeadingVector operator +(HeadingVector hv1, HeadingVector hv2)
    {
      HeadingVector headingVector = hv1;
      headingVector.X += hv2.X;
      headingVector.Y += hv2.Y;
      return headingVector;
    }

    public static HeadingVector operator -(HeadingVector hv1, HeadingVector hv2)
    {
      HeadingVector headingVector = hv1;
      headingVector.X -= hv2.X;
      headingVector.Y -= hv2.Y;
      return headingVector;
    }

    public static bool operator ==(HeadingVector hv1, HeadingVector hv2)
    {
      return hv1.X == hv2.X && hv1.Y == hv2.Y;
    }

    public static bool operator !=(HeadingVector hv1, HeadingVector hv2)
    {
      return !(hv1 == hv2);
    }

    public void SetCoordinates(int x = 0, int y = 0)
    {
      this.X = x;
      this.Y = y;
    }

    public static HeadingVector CalcDifference(Location source, Location target)
    {
      HeadingVector headingVector = new HeadingVector(target);
      headingVector.X -= source.get_Col();
      headingVector.Y -= source.get_Row();
      return headingVector;
    }

    public HeadingVector Orthogonal()
    {
      return new HeadingVector((Direction) 45)
      {
        X = this.Y,
        Y = -this.X
      };
    }

    public int NormSquared()
    {
      return this.X * this.X + this.Y * this.Y;
    }

    public double Norm()
    {
      return Math.Sqrt((double) this.NormSquared());
    }

    public int Norm1()
    {
      return Math.Abs(this.X) + Math.Abs(this.Y);
    }

    public override string ToString()
    {
      return "(" + (object) this.X + ", " + (string) (object) this.Y + ")";
    }

    public IEnumerable<Location> EnumerateLocations(Location originPivot)
    {
      // ISSUE: object of a compiler-generated type is created
      return (IEnumerable<Location>) new HeadingVector.\u003CEnumerateLocations\u003Ed__4(-2)
      {
        \u003C\u003E4__this = this,
        \u003C\u003E3__originPivot = originPivot
      };
    }

    public static Direction CalculateDirectionToStationeryTarget(Location myLoc, HeadingVector myHeading, Location target)
    {
      HeadingVector headingVector1 = HeadingVector.CalcDifference(myLoc, target);
      Direction direction = (Direction) 45;
      double num1 = -1.0;
      using (List<Direction>.Enumerator enumerator = Bot.Game.GetDirections(myLoc, target).GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          Direction current = enumerator.Current;
          HeadingVector headingVector2 = myHeading + current;
          double num2 = 1.0 - (double) (headingVector2 * headingVector1) / (headingVector2.Norm() * headingVector1.Norm());
          if (num2 < num1)
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
      double num = HeadingVector.SolveStupidEquation((double) targetHeading.Norm1(), (double) (target.get_Col() - myLoc.get_Col()), (double) targetHeading.X, (double) (target.get_Row() - myLoc.get_Row()), (double) targetHeading.Y);
      Location target1 = new Location(target.get_Row() + (int) (num * (double) targetHeading.Y), target.get_Col() + (int) (num * (double) targetHeading.X));
      return HeadingVector.CalculateDirectionToStationeryTarget(myLoc, myHeading, target1);
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
      double num = (double) (dir * headingVector) / dir.Norm();
      return Math.Abs((double) headingVector.X - num * (double) dir.X) + Math.Abs((double) headingVector.Y - num * (double) dir.Y);
    }

    public int ComparePirateByDirection(int p1, int p2)
    {
      double num = HeadingVector.CalcDistFromLine(new Location(0, 0), Bot.Game.GetMyPirate(p1).get_Loc(), this.Orthogonal());
      return (int) (HeadingVector.CalcDistFromLine(new Location(0, 0), Bot.Game.GetMyPirate(p2).get_Loc(), this.Orthogonal()) - num);
    }

    public static bool IsReachable(Location group, Location target, HeadingVector targetHeading)
    {
      HeadingVector headingVector = HeadingVector.CalcDifference(target, group);
      double num = (double) (headingVector * targetHeading) / (headingVector.Norm() * targetHeading.Norm());
      Location location = new Location(target.get_Row() + (int) ((double) targetHeading.Y * headingVector.Norm() * num / targetHeading.Norm()), target.get_Col() + (int) ((double) targetHeading.X * headingVector.Norm() * num / targetHeading.Norm()));
      return Bot.Game.Distance(group, location) < Bot.Game.Distance(target, location) - 2;
    }

    protected bool Equals(HeadingVector other)
    {
      return this.X == other.X && this.Y == other.Y;
    }

    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals((object) null, obj))
        return false;
      if (object.ReferenceEquals((object) this, obj))
        return true;
      if (obj.GetType() != this.GetType())
        return false;
      return this.Equals((HeadingVector) obj);
    }

    public override int GetHashCode()
    {
      return this.X * 397 ^ this.Y;
    }
  }
}
