// Decompiled with JetBrains decompiler
// Type: Pirates.InnerConsts
// Assembly: PiratesCsh, Version=1.0.5548.25549, Culture=neutral, PublicKeyToken=null
// MVID: 932FE985-6866-4B4F-91C1-D0B41B499FF8
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using System.Collections.Generic;

namespace Pirates
{
  public static class InnerConsts
  {
    public static readonly Dictionary<Direction, Location> AIM = new Dictionary<Direction, Location>()
    {
      {
        Direction.NORTH,
        new Location(-1, 0)
      },
      {
        Direction.EAST,
        new Location(0, 1)
      },
      {
        Direction.SOUTH,
        new Location(1, 0)
      },
      {
        Direction.WEST,
        new Location(0, -1)
      },
      {
        Direction.NOTHING,
        new Location(0, 0)
      },
      {
        Direction.REVEAL,
        new Location(0, 0)
      },
      {
        Direction.CLOAK,
        new Location(0, 0)
      }
    };
    public const int PIRATES = 0;
    public const int LOST = -1;
    public const int ISLAND = -1;
    public const int WATER = -2;
    public const int ZONE = -4;
    public const char LAND_IN_PARSE = 'l';
    public const char WATER_IN_PARSE = 'w';
    public const char ISLAND_IN_PARSE = '$';
    public const string PLAYER_PIRATE = "abcdefghij";
    public const string ISLAND_PIRATE = "ABCDEFGHIJ";
    public const string PLAYER_ISLAND = "0123456789";
    public const string MAP_OBJECT = "?%*.!";
    public const string MAP_RENDER = "abcdefghijABCDEFGHIJ0123456789?%*.!";
  }
}
