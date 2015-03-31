// Decompiled with JetBrains decompiler
// Type: Pirates.Location
// Assembly: PiratesCsh, Version=1.0.5561.34964, Culture=neutral, PublicKeyToken=null
// MVID: F3CB7840-9EB5-484E-B3E0-5A16B16AF427
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
