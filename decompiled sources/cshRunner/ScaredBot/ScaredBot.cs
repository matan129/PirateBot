// Decompiled with JetBrains decompiler
// Type: ScaredBot.ScaredBot
// Assembly: PiratesCsh, Version=1.0.5548.25549, Culture=neutral, PublicKeyToken=null
// MVID: 932FE985-6866-4B4F-91C1-D0B41B499FF8
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using System.Linq;
using Pirates;

namespace ScaredBot
{
    public class ScaredBot : IPirateBot
    {
        public void DoTurn(IPirateGame game)
        {
            var myPirate = game.GetMyPirate(0);
            if (myPirate.IsLost)
                return;
            var flag = false;
            foreach (var pirate in game.EnemyPirates())
            {
                if (game.Distance(myPirate, pirate) <= 5)
                {
                    var direction = game.GetDirections(pirate, myPirate).First();
                    game.SetSail(myPirate, direction);
                    flag = true;
                    game.Debug("Pirate is scared!");
                    break;
                }
            }
            if (flag)
                return;
            var island = game.GetIsland(0);
            var direction1 = game.GetDirections(myPirate, island).First();
            game.SetSail(myPirate, direction1);
        }
    }
}