// Decompiled with JetBrains decompiler
// Type: Pirates.Island
// Assembly: IPirateGame, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 20CDB0DF-8F9C-4B1A-B2C1-C4E9F4DD3D09
// Assembly location: C:\Users\Matan\Downloads\PIRATES\starter_kit\bots\csharp\IPirateGame.dll

using System;

namespace Pirates
{
  public class Island : IEquatable<Island>
  {
    public int Id { get; set; }

    public Location Loc { get; set; }

    public int Owner { get; set; }

    public int CaptureDuration { get; set; }

    public int TeamCapturing { get; set; }

    public Island(int Id, Location Loc, int Owner, int TeamCapturing, int CaptureDuration)
    {
      this.Id = Id;
      this.Loc = Loc;
      this.Owner = Owner;
      this.CaptureDuration = CaptureDuration;
      this.TeamCapturing = TeamCapturing;
    }

    public override int GetHashCode()
    {
      return this.Id;
    }

    public override string ToString()
    {
      return string.Format("Island ID:{0} OWNER:{1} LOC:({2}, {3})", (object) this.Id, (object) this.Owner, (object) this.Loc.Row, (object) this.Loc.Col);
    }

    public bool Equals(Island other)
    {
      return other != null && this.Id.Equals(other.Id);
    }
  }
}
