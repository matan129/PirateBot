// Decompiled with JetBrains decompiler
// Type: MyBot.TutorialBot
// Assembly: PiratesCsh, Version=1.0.5561.34964, Culture=neutral, PublicKeyToken=null
// MVID: F3CB7840-9EB5-484E-B3E0-5A16B16AF427
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using Pirates;
using System.Collections.Generic;
using System.Linq;

namespace MyBot
{
  public class TutorialBot : IPirateBot
  {
    public void DoTurn(IPirateGame game)
    {
      if (Enumerable.Count<Island>((IEnumerable<Island>) game.NotMyIslands()) == 0)
        return;
      Island island = Enumerable.First<Island>((IEnumerable<Island>) game.NotMyIslands());
      game.Debug("going to island " + island.Id.ToString());
      foreach (Pirate pirate in game.MyPirates())
      {
        Direction direction = Enumerable.First<Direction>((IEnumerable<Direction>) game.GetDirections(pirate, island));
        game.SetSail(pirate, direction);
      }
    }
  }
}
