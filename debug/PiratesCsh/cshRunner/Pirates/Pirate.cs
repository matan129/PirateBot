// Decompiled with JetBrains decompiler
// Type: Pirates.Pirate
// Assembly: PiratesCsh, Version=1.0.5569.19785, Culture=neutral, PublicKeyToken=null
// MVID: A3BB42EC-B38F-4348-B6D7-902E3B33DA85
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

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

    public bool IsCloaked { get; set; }

    public Pirate(int Id, int Owner, Location Loc, Location InitialLocation)
    {
      this.Id = Id;
      this.Owner = Owner;
      this.Loc = Loc;
      this.InitialLocation = InitialLocation;
      this.IsLost = false;
      this.TurnsToRevive = 0;
      this.IsCloaked = false;
    }

    public override int GetHashCode()
    {
      return this.Id * 10 + this.Owner;
    }

    public override string ToString()
    {
      return string.Format("<Pirate ID:{0} OWNER:{1} LOC:({2}, {3})>", (object) this.Id, (object) this.Owner, (object) this.Loc.Row, (object) this.Loc.Col);
    }

    public bool Equals(Pirate other)
    {
      if (other != null)
        return this.Id.Equals(other.Id);
      return false;
    }
  }
}
