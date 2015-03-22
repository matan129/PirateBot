// Decompiled with JetBrains decompiler
// Type: MyBot.DevelopBot
// Assembly: PiratesCsh, Version=1.0.5548.25549, Culture=neutral, PublicKeyToken=null
// MVID: 932FE985-6866-4B4F-91C1-D0B41B499FF8
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using System;
using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace MyBot
{
    public class DevelopBot : IPirateBot
    {
        public static int SHIPCOUNT = 6;
        public static int ISLANDCOUNT = 6;
        private IPirateGame game;
        private Random rnd;

        public DevelopBot()
        {
            rnd = new Random();
        }

        public void DoTurn(IPirateGame game)
        {
            var location = new Location(-1, -1);
            game.IsPassable(location);
            this.game = game;
            if (game.NotMyIslands().Count < 1 || game.EnemyPirates().Count <= 0)
                return;
            foreach (var pirate in game.MyPirates())
            {
                var closest = get_closest(pirate, game.EnemyPirates());
                var directions = game.GetDirections(pirate.Loc, closest.Loc);
                if (game.IsPassable(game.Destination(pirate, directions.First())))
                    game.SetSail(pirate, directions.First());
            }
        }

        private Pirate get_closest(Pirate a, List<Pirate> pirates)
        {
            var pirate1 = pirates.First();
            var num1 = 9999;
            foreach (var pirate2 in game.EnemyPirates())
            {
                var num2 = game.Distance(a.Loc, pirate2.Loc);
                if (num2 < num1)
                {
                    pirate1 = pirate2;
                    num1 = num2;
                }
            }
            return pirate1;
        }
    }
}