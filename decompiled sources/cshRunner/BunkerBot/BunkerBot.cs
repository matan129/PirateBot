// Decompiled with JetBrains decompiler
// Type: BunkerBot.BunkerBot
// Assembly: PiratesCsh, Version=1.0.5548.25549, Culture=neutral, PublicKeyToken=null
// MVID: 932FE985-6866-4B4F-91C1-D0B41B499FF8
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using System;
using System.Linq;
using Pirates;

namespace BunkerBot
{
    public class BunkerBot : IPirateBot
    {
        public void DoTurn(IPirateGame game)
        {
            var island = game.GetIsland(0);
            var random = new Random();
            for (var index = 0; index < game.MyPirates().Count(); ++index)
            {
                var pirate = game.MyPirates().ElementAt(index);
                if (index == 0)
                {
                    game.Debug("This pirate is going to capture the island " + pirate.Id);
                    var direction = game.GetDirections(pirate, island).First();
                    game.SetSail(pirate, direction);
                }
                else
                {
                    game.Debug("This pirate is going to protect the island " + pirate.Id);
                    var location = new Location(island.Loc.Row + random.Next(-1, 2), island.Loc.Col + random.Next(-1, 2));
                    var direction = game.GetDirections(pirate, location).First();
                    game.SetSail(pirate, direction);
                }
            }
        }
    }
}