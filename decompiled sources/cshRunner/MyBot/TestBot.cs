// Decompiled with JetBrains decompiler
// Type: MyBot.TestBot
// Assembly: PiratesCsh, Version=1.0.5548.25549, Culture=neutral, PublicKeyToken=null
// MVID: 932FE985-6866-4B4F-91C1-D0B41B499FF8
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using System;
using System.Linq;
using Pirates;

namespace MyBot
{
    public class TestBot : IPirateBot
    {
        public static int SHIPCOUNT = 6;
        public static int ISLANDCOUNT = 5;
        private readonly Random rnd;
        private int pirate1counter;

        public TestBot()
        {
            rnd = new Random();
            pirate1counter = 0;
        }

        public void DoTurn(IPirateGame game)
        {
            test_API(game);
        }

        private void test_API(IPirateGame game)
        {
            game.MyPirates().Concat(game.MyLostPirates()).OrderBy(t => t.Id).SequenceEqual(game.AllMyPirates());
            game.EnemyPirates().Concat(game.EnemyLostPirates()).OrderBy(t => t.Id).SequenceEqual(game.AllMyPirates());
            var i = rnd.Next(0, game.AllMyPirates().Count());
            foreach (var pirate in game.AllMyPirates().Where(p => p.Id != i))
                ;
            i = rnd.Next(0, game.AllMyPirates().Count());
            foreach (var pirate in game.AllEnemyPirates().Where(p => p.Id != i))
                ;
            foreach (var pirate in game.AllMyPirates().Union(game.AllEnemyPirates()))
            {
                var num = pirate.IsLost ? 1 : 0;
            }
            game.MyIslands().Concat(game.NotMyIslands()).OrderBy(t => t.Id).SequenceEqual(game.Islands());
            game.EnemyIslands().Concat(game.NeutralIslands()).OrderBy(t => t.Id).SequenceEqual(game.NotMyIslands());
            game.MyIslands()
                .Concat(game.EnemyIslands())
                .Concat(game.NeutralIslands())
                .OrderBy(t => t.Id)
                .SequenceEqual(game.Islands());
            i = rnd.Next(0, game.Islands().Count());
            foreach (var island in game.Islands().Where(p => p.Id != i))
                ;
            var directions = game.GetDirections(game.GetMyPirate(0), game.GetIsland(0));
            game.GetTurn();
            game.SetSail(game.GetMyPirate(0), directions.ElementAt(0));
            if (game.GetIsland(0).Owner != 0)
            {
                if (game.GetPirateOn(game.GetIsland(0)) == game.GetMyPirate(0))
                    ++pirate1counter;
                else
                    pirate1counter = 0;
            }
            var myPirate1 = game.GetMyPirate(4);
            var myPirate2 = game.GetMyPirate(3);
            var direction = game.GetDirections(myPirate2, myPirate1).First();
            game.SetSail(myPirate2, direction);
        }
    }
}