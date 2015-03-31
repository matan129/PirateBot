// Decompiled with JetBrains decompiler
// Type: Pirates.Pirate
// Assembly: PiratesCsh, Version=1.0.5548.25549, Culture=neutral, PublicKeyToken=null
// MVID: 932FE985-6866-4B4F-91C1-D0B41B499FF8
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using System;

namespace Pirates
{
    public class Pirate : IEquatable<Pirate>
    {
        public Pirate(int Id, int Owner, Location Loc, Location InitialLocation)
        {
            this.Id = Id;
            this.Owner = Owner;
            this.Loc = Loc;
            this.InitialLocation = InitialLocation;
            IsLost = false;
            TurnsToRevive = 0;
            IsCloaked = false;
        }

        public int Id { get; set; }
        public int Owner { get; set; }
        public Location Loc { get; set; }
        public Location InitialLocation { get; set; }
        public bool IsLost { get; set; }
        public int TurnsToRevive { get; set; }
        public bool IsCloaked { get; set; }

        public bool Equals(Pirate other)
        {
            if (other != null)
                return Id.Equals(other.Id);
            return false;
        }

        public override int GetHashCode()
        {
            return Id*10 + Owner;
        }

        public override string ToString()
        {
            return string.Format("<Pirate ID:{0} OWNER:{1} LOC:({2}, {3})>", (object) Id, (object) Owner,
                (object) Loc.Row, (object) Loc.Col);
        }
    }
}