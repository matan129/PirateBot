// Decompiled with JetBrains decompiler
// Type: MyBot.TutorialBot
// Assembly: PiratesCsh, Version=1.0.5548.25549, Culture=neutral, PublicKeyToken=null
// MVID: 932FE985-6866-4B4F-91C1-D0B41B499FF8
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using System.Linq;
using Pirates;

namespace MyBot
{
    public class TutorialBot : IPirateBot
    {
        public void DoTurn(IPirateGame game)
        {
            if (game.NotMyIslands().Count() == 0)
                return;
            var island = game.NotMyIslands().First();
            game.Debug("going to island " + island.Id);
            foreach (var pirate in game.MyPirates())
            {
                var direction = game.GetDirections(pirate, island).First();
                game.SetSail(pirate, direction);
            }
        }
    }
}