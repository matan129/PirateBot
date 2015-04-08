// Decompiled with JetBrains decompiler
// Type: Britbot.HeadingVector
// Assembly: zqyomklw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: F85BCEFF-D2D0-45EB-AADF-8EB4692C0C30
// Assembly location: C:\Users\Matan\AppData\Local\Temp\zqyomklw.dll

using Pirates;
using System;

namespace Britbot
{
  public class HeadingVector
  {
    public double X { get; private set; }

    public double Y { get; private set; }

    public HeadingVector(Direction d = 45)
    {
      Direction direction = d;
      if (direction <= 110)
      {
        if (direction != 101)
        {
          if (direction == 110)
          {
            this.X = 0.0;
            this.Y = -1.0;
            return;
          }
        }
        else
        {
          this.X = 1.0;
          this.Y = 0.0;
          return;
        }
      }
      else if (direction != 115)
      {
        if (direction == 119)
        {
          this.X = -1.0;
          this.Y = 0.0;
          return;
        }
      }
      else
      {
        this.X = 0.0;
        this.Y = 1.0;
        return;
      }
      this.X = 0.0;
      this.Y = 0.0;
    }

    public HeadingVector(HeadingVector toCopy)
    {
      this.X = toCopy.X;
      this.Y = toCopy.Y;
    }

    public HeadingVector(double x, double y)
    {
      this.X = x;
      this.Y = y;
    }

    public static double operator *(HeadingVector hv1, HeadingVector hv2)
    {
      return hv1.X * hv2.X + hv1.Y * hv2.Y;
    }

    public static bool operator ==(HeadingVector hv1, HeadingVector hv2)
    {
      return hv1.X == hv2.X && hv1.Y == hv2.Y;
    }

    public static bool operator !=(HeadingVector hv1, HeadingVector hv2)
    {
      return !(hv1 == hv2);
    }

    public static HeadingVector operator +(HeadingVector hv1, HeadingVector hv2)
    {
      HeadingVector headingVector = new HeadingVector(hv1);
      headingVector.X += hv2.X;
      headingVector.Y += hv2.Y;
      return headingVector;
    }

    public static HeadingVector operator -(HeadingVector hv1, HeadingVector hv2)
    {
      HeadingVector headingVector = new HeadingVector(hv1);
      headingVector.X -= hv2.X;
      headingVector.Y -= hv2.Y;
      return headingVector;
    }

    public static HeadingVector operator *(double scalar, HeadingVector hv)
    {
      return new HeadingVector(scalar * hv.X, scalar * hv.Y);
    }

    public void SetCoordinates(double x = 0.0, double y = 0.0)
    {
      this.X = x;
      this.Y = y;
    }

    public void SetCoordinates(HeadingVector hv)
    {
      this.X = hv.X;
      this.Y = hv.Y;
    }

    public static HeadingVector adjustHeading(HeadingVector hv1, HeadingVector hv2)
    {
      HeadingVector headingVector = new HeadingVector(hv1);
      if (hv1 * hv2 < 0.0)
        headingVector = hv2;
      else if (hv2.Norm() == 0.0)
      {
        headingVector = hv2;
      }
      else
      {
        headingVector.X += hv2.X;
        headingVector.Y += hv2.Y;
      }
      return headingVector;
    }

    public static HeadingVector adjustHeading(HeadingVector hv1, Direction dir)
    {
      return HeadingVector.adjustHeading(hv1, new HeadingVector(dir));
    }

    public HeadingVector adjustHeading(HeadingVector Dir)
    {
      this.SetCoordinates(HeadingVector.adjustHeading(this, Dir));
      return this;
    }

    public HeadingVector adjustHeading(Direction Dir)
    {
      this.SetCoordinates(HeadingVector.adjustHeading(this, new HeadingVector(Dir)));
      return this;
    }

    public static HeadingVector CalcDifference(Location source, Location target)
    {
      return new HeadingVector((double) (target.get_Col() - source.get_Col()), (double) (target.get_Row() - source.get_Row()));
    }

    public static Location AddvanceByVector(Location loc, HeadingVector hv)
    {
      int val1 = loc.get_Col() + (int) hv.X;
      return new Location(Math.Min(Math.Max(loc.get_Row() + (int) hv.Y, 0), Bot.Game.GetRows() - 1), Math.Min(Math.Max(val1, 0), Bot.Game.GetCols() - 1));
    }

    public HeadingVector Orthogonal()
    {
      return new HeadingVector((Direction) 45)
      {
        X = this.Y,
        Y = -this.X
      };
    }

    public double NormSquared()
    {
      return this.X * this.X + this.Y * this.Y;
    }

    public double Norm()
    {
      return Math.Sqrt(this.NormSquared());
    }

    public HeadingVector Normalize()
    {
      return 1.0 / this.Norm() * this;
    }

    public double Norm1()
    {
      return Math.Abs(this.X) + Math.Abs(this.Y);
    }

    public override string ToString()
    {
      return "(" + (object) this.X + ", " + (string) (object) this.Y + ")";
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
  }
}
