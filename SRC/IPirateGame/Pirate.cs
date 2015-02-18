// Decompiled with JetBrains decompiler
// Type: Pirates.Pirate
// Assembly: IPirateGame, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 20CDB0DF-8F9C-4B1A-B2C1-C4E9F4DD3D09
// Assembly location: C:\Users\Matan\Downloads\PIRATES\starter_kit\bots\csharp\IPirateGame.dll

using System;

namespace Pirates
{
  public class Pirate : IEquatable<Pirate>
  {
    public int Id { get; set; }

    public int Owner { get; set; }

    public Location Loc { get; set; }

    public Location InitialLocation { get; set; }

    public bool IsLost { get; set; }

    public int TurnsToRevive { get; set; }

    public Pirate(int Id, int Owner, Location Loc, Location InitialLocation)
    {
      this.Id = Id;
      this.Owner = Owner;
      this.Loc = Loc;
      this.InitialLocation = InitialLocation;
      this.IsLost = false;
      this.TurnsToRevive = 0;
    }

    public override int GetHashCode()
    {
      return this.Id * 10 + this.Owner;
    }

    public override string ToString()
    {
      return string.Format("Pirate ID:{0} OWNER:{1} LOC:({2}, {3})", (object) this.Id, (object) this.Owner, (object) this.Loc.Row, (object) this.Loc.Col);
    }

    public bool Equals(Pirate other)
    {
      return other != null && this.Id.Equals(other.Id);
    }
  }
}
