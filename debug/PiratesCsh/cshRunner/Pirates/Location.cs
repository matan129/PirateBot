// Decompiled with JetBrains decompiler
// Type: Pirates.Location
// Assembly: PiratesCsh, Version=1.0.5569.19785, Culture=neutral, PublicKeyToken=null
// MVID: A3BB42EC-B38F-4348-B6D7-902E3B33DA85
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using System;

namespace Pirates
{
  public class Location : IEquatable<Location>
  {
    public int Row { get; set; }

    public int Col { get; set; }

    public Location(int Row, int Col)
    {
      this.Row = Row;
      this.Col = Col;
    }

    public Location(Location OtherLocation)
    {
      this.Row = OtherLocation.Row;
      this.Col = OtherLocation.Col;
    }

    public override int GetHashCode()
    {
      return this.Col * 100 + this.Row;
    }

    public override string ToString()
    {
      return string.Format("({0}, {1})", (object) this.Row, (object) this.Col);
    }

    public bool Equals(Location other)
    {
      if (other != null && this.Row == other.Row)
        return this.Col == other.Col;
      return false;
    }
  }
}
