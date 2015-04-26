// Decompiled with JetBrains decompiler
// Type: Pirates.Location
// Assembly: PiratesCsh, Version=1.0.5581.42591, Culture=neutral, PublicKeyToken=null
// MVID: F9F1F072-EFD6-461C-A5E1-7E4E5CE853F7
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
