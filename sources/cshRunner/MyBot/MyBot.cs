// Decompiled with JetBrains decompiler
// Type: MyBot.MyBot
// Assembly: PiratesCsh, Version=1.0.5548.25549, Culture=neutral, PublicKeyToken=null
// MVID: 932FE985-6866-4B4F-91C1-D0B41B499FF8
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using Pirates;
using System.Collections.Generic;
using System.Linq;

namespace MyBot
{
  public class MyBot : IPirateBot
  {
    private int firstPirateId = -1;

    public void DoTurn(IPirateGame game)
    {
      if (this.firstPirateId == -1)
        this.firstPirateId = Enumerable.First<Pirate>((IEnumerable<Pirate>) game.MyPirates()).Id;
      game.GetMyPirate(this.firstPirateId);
    }
  }
}
