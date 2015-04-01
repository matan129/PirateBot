// Decompiled with JetBrains decompiler
// Type: MyBot.TutorialBot
// Assembly: PiratesCsh, Version=1.0.5569.19785, Culture=neutral, PublicKeyToken=null
// MVID: A3BB42EC-B38F-4348-B6D7-902E3B33DA85
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
