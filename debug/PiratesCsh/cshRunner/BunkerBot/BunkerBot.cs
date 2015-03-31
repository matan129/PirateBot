// Decompiled with JetBrains decompiler
// Type: BunkerBot.BunkerBot
// Assembly: PiratesCsh, Version=1.0.5561.34964, Culture=neutral, PublicKeyToken=null
// MVID: F3CB7840-9EB5-484E-B3E0-5A16B16AF427
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using Pirates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BunkerBot
{
  public class BunkerBot : IPirateBot
  {
    public void DoTurn(IPirateGame game)
    {
      Island island = game.GetIsland(0);
      Random random = new Random();
      for (int index = 0; index < Enumerable.Count<Pirate>((IEnumerable<Pirate>) game.MyPirates()); ++index)
      {
        Pirate pirate = Enumerable.ElementAt<Pirate>((IEnumerable<Pirate>) game.MyPirates(), index);
        if (index == 0)
        {
          game.Debug("This pirate is going to capture the island " + pirate.Id.ToString());
          Direction direction = Enumerable.First<Direction>((IEnumerable<Direction>) game.GetDirections(pirate, island));
          game.SetSail(pirate, direction);
        }
        else
        {
          game.Debug("This pirate is going to protect the island " + pirate.Id.ToString());
          Location location = new Location(island.Loc.Row + random.Next(-1, 2), island.Loc.Col + random.Next(-1, 2));
          Direction direction = Enumerable.First<Direction>((IEnumerable<Direction>) game.GetDirections(pirate, location));
          game.SetSail(pirate, direction);
        }
      }
    }
  }
}
