// Decompiled with JetBrains decompiler
// Type: HunterBot.HunterBot
// Assembly: PiratesCsh, Version=1.0.5561.34964, Culture=neutral, PublicKeyToken=null
// MVID: F3CB7840-9EB5-484E-B3E0-5A16B16AF427
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using Pirates;
using System.Collections.Generic;
using System.Linq;

namespace HunterBot
{
  public class HunterBot : IPirateBot
  {
    public void DoTurn(IPirateGame game)
    {
      if (Enumerable.Count<Pirate>((IEnumerable<Pirate>) game.EnemyPirates()) < 2)
      {
        game.Debug("Less than two enemies - not doing anything");
      }
      else
      {
        Pirate pirate2_1 = Enumerable.ElementAt<Pirate>((IEnumerable<Pirate>) game.EnemyPirates(), 0);
        game.Debug("going to get enemy number " + pirate2_1.Id.ToString());
        for (int id = 0; id < Enumerable.Count<Pirate>((IEnumerable<Pirate>) game.AllMyPirates()) / 2; ++id)
        {
          Pirate myPirate = game.GetMyPirate(id);
          if (!myPirate.IsLost)
          {
            Direction direction = Enumerable.First<Direction>((IEnumerable<Direction>) game.GetDirections(myPirate, pirate2_1));
            game.SetSail(myPirate, direction);
          }
        }
        Pirate pirate2_2 = Enumerable.ElementAt<Pirate>((IEnumerable<Pirate>) game.EnemyPirates(), 1);
        game.Debug("going to get enemy number " + pirate2_2.Id.ToString());
        for (int id = Enumerable.Count<Pirate>((IEnumerable<Pirate>) game.AllMyPirates()) / 2; id < Enumerable.Count<Pirate>((IEnumerable<Pirate>) game.AllMyPirates()); ++id)
        {
          Pirate myPirate = game.GetMyPirate(id);
          if (!myPirate.IsLost)
          {
            Direction direction = Enumerable.First<Direction>((IEnumerable<Direction>) game.GetDirections(myPirate, pirate2_2));
            game.SetSail(myPirate, direction);
          }
        }
      }
    }
  }
}
