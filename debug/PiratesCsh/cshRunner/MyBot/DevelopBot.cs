// Decompiled with JetBrains decompiler
// Type: MyBot.DevelopBot
// Assembly: PiratesCsh, Version=1.0.5569.19785, Culture=neutral, PublicKeyToken=null
// MVID: A3BB42EC-B38F-4348-B6D7-902E3B33DA85
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using Pirates;
using System;
using System.Collections.Generic;
using System.Linq;

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
      this.rnd = new Random();
    }

    public void DoTurn(IPirateGame game)
    {
      Location location = new Location(-1, -1);
      game.IsPassable(location);
      this.game = game;
      if (game.NotMyIslands().Count < 1 || game.EnemyPirates().Count <= 0)
        return;
      foreach (Pirate pirate in game.MyPirates())
      {
        Pirate closest = this.get_closest(pirate, game.EnemyPirates());
        List<Direction> directions = game.GetDirections(pirate.Loc, closest.Loc);
        if (game.IsPassable(game.Destination(pirate, Enumerable.First<Direction>((IEnumerable<Direction>) directions))))
          game.SetSail(pirate, Enumerable.First<Direction>((IEnumerable<Direction>) directions));
      }
    }

    private Pirate get_closest(Pirate a, List<Pirate> pirates)
    {
      Pirate pirate1 = Enumerable.First<Pirate>((IEnumerable<Pirate>) pirates);
      int num1 = 9999;
      foreach (Pirate pirate2 in this.game.EnemyPirates())
      {
        int num2 = this.game.Distance(a.Loc, pirate2.Loc);
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
