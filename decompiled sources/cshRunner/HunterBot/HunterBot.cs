// Decompiled with JetBrains decompiler
// Type: HunterBot.HunterBot
// Assembly: PiratesCsh, Version=1.0.5548.25549, Culture=neutral, PublicKeyToken=null
// MVID: 932FE985-6866-4B4F-91C1-D0B41B499FF8
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using System.Linq;
using Pirates;

namespace HunterBot
{
    public class HunterBot : IPirateBot
    {
        public void DoTurn(IPirateGame game)
        {
            if (game.EnemyPirates().Count() < 2)
            {
                game.Debug("Less than two enemies - not doing anything");
            }
            else
            {
                var pirate2_1 = game.EnemyPirates().ElementAt(0);
                game.Debug("going to get enemy number " + pirate2_1.Id);
                for (var id = 0; id < game.AllMyPirates().Count()/2; ++id)
                {
                    var myPirate = game.GetMyPirate(id);
                    if (!myPirate.IsLost)
                    {
                        var direction = game.GetDirections(myPirate, pirate2_1).First();
                        game.SetSail(myPirate, direction);
                    }
                }
                var pirate2_2 = game.EnemyPirates().ElementAt(1);
                game.Debug("going to get enemy number " + pirate2_2.Id);
                for (var id = game.AllMyPirates().Count()/2; id < game.AllMyPirates().Count(); ++id)
                {
                    var myPirate = game.GetMyPirate(id);
                    if (!myPirate.IsLost)
                    {
                        var direction = game.GetDirections(myPirate, pirate2_2).First();
                        game.SetSail(myPirate, direction);
                    }
                }
            }
        }
    }
}