﻿// Decompiled with JetBrains decompiler
// Type: Pirates.Island
// Assembly: PiratesCsh, Version=1.0.5561.34964, Culture=neutral, PublicKeyToken=null
// MVID: F3CB7840-9EB5-484E-B3E0-5A16B16AF427
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using System;

namespace Pirates
{
  public class Island : IEquatable<Island>
  {
    public int Id { get; set; }

    public Location Loc { get; set; }

    public int Owner { get; set; }

    public int CaptureTurns { get; set; }

    public int TeamCapturing { get; set; }

    public int TurnsBeingCaptured { get; set; }

    public int Value { get; set; }

    public Island(int Id, Location Loc, int Owner, int TeamCapturing, int TurnsBeingCaptured, int CaptureTurns, int Value)
    {
      this.Id = Id;
      this.Loc = Loc;
      this.Owner = Owner;
      this.CaptureTurns = CaptureTurns;
      this.TeamCapturing = TeamCapturing;
      this.TurnsBeingCaptured = TurnsBeingCaptured;
      this.Value = Value;
    }

    public override int GetHashCode()
    {
      return this.Id;
    }

    public override string ToString()
    {
      return string.Format("<Island ID:{0} OWNER:{1} LOC:({2}, {3})>", (object) this.Id, (object) this.Owner, (object) this.Loc.Row, (object) this.Loc.Col);
    }

    public bool Equals(Island other)
    {
      if (other != null)
        return this.Id.Equals(other.Id);
      return false;
    }
  }
}
