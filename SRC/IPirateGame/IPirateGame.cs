// Decompiled with JetBrains decompiler
// Type: Pirates.IPirateGame
// Assembly: IPirateGame, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 20CDB0DF-8F9C-4B1A-B2C1-C4E9F4DD3D09
// Assembly location: C:\Users\Matan\Downloads\PIRATES\starter_kit\bots\csharp\IPirateGame.dll

using System.Collections.Generic;

namespace Pirates
{
  public interface IPirateGame
  {
    List<Pirate> AllMyPirates();

    List<Pirate> MyPirates();

    List<Pirate> MyLostPirates();

    List<Pirate> AllEnemyPirates();

    List<Pirate> EnemyPirates();

    List<Pirate> EnemyLostPirates();

    Pirate GetMyPirate(int id);

    Pirate GetEnemyPirate(int id);

    Pirate GetPirateOn(Island island);

    Pirate GetPirateOn(Location location);

    List<Island> Islands();

    List<Island> MyIslands();

    List<Island> NotMyIslands();

    List<Island> EnemyIslands();

    List<Island> NeutralIslands();

    Island GetIsland(int id);

    List<Direction> GetDirections(Location loc1, Location loc2);

    List<Direction> GetDirections(Pirate pirate, Island island);

    List<Direction> GetDirections(Pirate pirate1, Pirate pirate2);

    List<Direction> GetDirections(Pirate pirate, Location location);

    void SetSail(Pirate pirate, Direction direction);

    int Distance(Location loc1, Location loc2);

    int Distance(Pirate pirate1, Pirate pirate2);

    int Distance(Pirate pirate, Island island);

    int Distance(Island island, Pirate pirate);

    int Distance(Island island1, Island island2);

    Location Destination(Pirate a, Direction d);

    Location Destination(Location loc, Direction d);

    void Debug(string message);

    void Debug(string message, List<int> turnsToDisplay);

    void Debug(string message, int turnToDisplay);

    int GetTurn();

    bool InRange(Pirate a1, Location loc2);

    bool InRange(Location l1, Pirate a2);

    bool InRange(Location loc1, Location loc2);

    bool InRange(Pirate a1, Pirate a2);

    bool isCapturing(Pirate pirate);

    bool IsPassable(Location location);

    bool IsOccupied(Location loc);
  }
}
