// Decompiled with JetBrains decompiler
// Type: Pirates.Location
// Assembly: IPirateGame, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 20CDB0DF-8F9C-4B1A-B2C1-C4E9F4DD3D09
// Assembly location: C:\Users\Matan\Downloads\PIRATES\starter_kit\bots\csharp\IPirateGame.dll

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
      return other != null && this.Row == other.Row && this.Col == other.Col;
    }
  }
}
