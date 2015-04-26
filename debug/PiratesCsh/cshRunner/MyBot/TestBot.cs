// Decompiled with JetBrains decompiler
// Type: MyBot.TestBot
// Assembly: PiratesCsh, Version=1.0.5581.42591, Culture=neutral, PublicKeyToken=null
// MVID: F9F1F072-EFD6-461C-A5E1-7E4E5CE853F7
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using Pirates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyBot
{
  public class TestBot : IPirateBot
  {
    public static int SHIPCOUNT = 6;
    public static int ISLANDCOUNT = 5;
    private Random rnd;
    private int pirate1counter;

    public TestBot()
    {
      this.rnd = new Random();
      this.pirate1counter = 0;
    }

    public void DoTurn(IPirateGame game)
    {
      this.test_API(game);
    }

    private void test_API(IPirateGame game)
    {
      Enumerable.SequenceEqual<Pirate>((IEnumerable<Pirate>) Enumerable.OrderBy<Pirate, int>(Enumerable.Concat<Pirate>((IEnumerable<Pirate>) game.MyPirates(), (IEnumerable<Pirate>) game.MyLostPirates()), (Func<Pirate, int>) (t => t.Id)), (IEnumerable<Pirate>) game.AllMyPirates());
      Enumerable.SequenceEqual<Pirate>((IEnumerable<Pirate>) Enumerable.OrderBy<Pirate, int>(Enumerable.Concat<Pirate>((IEnumerable<Pirate>) game.EnemyPirates(), (IEnumerable<Pirate>) game.EnemyLostPirates()), (Func<Pirate, int>) (t => t.Id)), (IEnumerable<Pirate>) game.AllMyPirates());
      int i = this.rnd.Next(0, Enumerable.Count<Pirate>((IEnumerable<Pirate>) game.AllMyPirates()));
      foreach (Pirate pirate in Enumerable.Where<Pirate>((IEnumerable<Pirate>) game.AllMyPirates(), (Func<Pirate, bool>) (p => p.Id != i)))
        ;
      i = this.rnd.Next(0, Enumerable.Count<Pirate>((IEnumerable<Pirate>) game.AllMyPirates()));
      foreach (Pirate pirate in Enumerable.Where<Pirate>((IEnumerable<Pirate>) game.AllEnemyPirates(), (Func<Pirate, bool>) (p => p.Id != i)))
        ;
      foreach (Pirate pirate in Enumerable.Union<Pirate>((IEnumerable<Pirate>) game.AllMyPirates(), (IEnumerable<Pirate>) game.AllEnemyPirates()))
      {
        int num = pirate.IsLost ? 1 : 0;
      }
      Enumerable.SequenceEqual<Island>((IEnumerable<Island>) Enumerable.OrderBy<Island, int>(Enumerable.Concat<Island>((IEnumerable<Island>) game.MyIslands(), (IEnumerable<Island>) game.NotMyIslands()), (Func<Island, int>) (t => t.Id)), (IEnumerable<Island>) game.Islands());
      Enumerable.SequenceEqual<Island>((IEnumerable<Island>) Enumerable.OrderBy<Island, int>(Enumerable.Concat<Island>((IEnumerable<Island>) game.EnemyIslands(), (IEnumerable<Island>) game.NeutralIslands()), (Func<Island, int>) (t => t.Id)), (IEnumerable<Island>) game.NotMyIslands());
      Enumerable.SequenceEqual<Island>((IEnumerable<Island>) Enumerable.OrderBy<Island, int>(Enumerable.Concat<Island>(Enumerable.Concat<Island>((IEnumerable<Island>) game.MyIslands(), (IEnumerable<Island>) game.EnemyIslands()), (IEnumerable<Island>) game.NeutralIslands()), (Func<Island, int>) (t => t.Id)), (IEnumerable<Island>) game.Islands());
      i = this.rnd.Next(0, Enumerable.Count<Island>((IEnumerable<Island>) game.Islands()));
      foreach (Island island in Enumerable.Where<Island>((IEnumerable<Island>) game.Islands(), (Func<Island, bool>) (p => p.Id != i)))
        ;
      List<Direction> directions = game.GetDirections(game.GetMyPirate(0), game.GetIsland(0));
      game.GetTurn();
      game.SetSail(game.GetMyPirate(0), Enumerable.ElementAt<Direction>((IEnumerable<Direction>) directions, 0));
      if (game.GetIsland(0).Owner != 0)
      {
        if (game.GetPirateOn(game.GetIsland(0)) == game.GetMyPirate(0))
          ++this.pirate1counter;
        else
          this.pirate1counter = 0;
      }
      Pirate myPirate1 = game.GetMyPirate(4);
      Pirate myPirate2 = game.GetMyPirate(3);
      Direction direction = Enumerable.First<Direction>((IEnumerable<Direction>) game.GetDirections(myPirate2, myPirate1));
      game.SetSail(myPirate2, direction);
    }
  }
}
