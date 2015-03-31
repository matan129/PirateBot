// Decompiled with JetBrains decompiler
// Type: MyBot.MyBot
// Assembly: PiratesCsh, Version=1.0.5548.25549, Culture=neutral, PublicKeyToken=null
// MVID: 932FE985-6866-4B4F-91C1-D0B41B499FF8
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using System.Linq;
using Pirates;

namespace MyBot
{
    public class MyBot : IPirateBot
    {
        private int firstPirateId = -1;

        public void DoTurn(IPirateGame game)
        {
            PirateGame p = (MyBot.PirateGame) game;
            /*unsafe
            {
                PirateGame* p = &game;
            }*/
        }
    }
}