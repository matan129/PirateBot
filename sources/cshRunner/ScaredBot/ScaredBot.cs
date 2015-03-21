﻿// Decompiled with JetBrains decompiler
// Type: ScaredBot.ScaredBot
// Assembly: PiratesCsh, Version=1.0.5548.25549, Culture=neutral, PublicKeyToken=null
// MVID: 932FE985-6866-4B4F-91C1-D0B41B499FF8
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using Pirates;
using System.Collections.Generic;
using System.Linq;

namespace ScaredBot
{
  public class ScaredBot : IPirateBot
  {
    public void DoTurn(IPirateGame game)
    {
      Pirate myPirate = game.GetMyPirate(0);
      if (myPirate.IsLost)
        return;
      bool flag = false;
      foreach (Pirate pirate in game.EnemyPirates())
      {
        if (game.Distance(myPirate, pirate) <= 5)
        {
          Direction direction = Enumerable.First<Direction>((IEnumerable<Direction>) game.GetDirections(pirate, myPirate));
          game.SetSail(myPirate, direction);
          flag = true;
          game.Debug("Pirate is scared!");
          break;
        }
      }
      if (flag)
        return;
      Island island = game.GetIsland(0);
      Direction direction1 = Enumerable.First<Direction>((IEnumerable<Direction>) game.GetDirections(myPirate, island));
      game.SetSail(myPirate, direction1);
    }
  }
}
