using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using Pirates;

namespace misc_example
{
    public class MiscBot : Pirates.IPirateBot
    {
        public void DoTurn(Pirates.IPirateGame game)
        {
            /**
             * In this example we will see a few useful utility functions
             * which will help you create more powerful Bots
             */

            // Use these functions to see the scores of each team and how many points we got on the last turn
            if (game.GetEnemyScore() > game.GetMyScore())
            {
                game.Debug("We are losing! Enemy has {0} points and we have {1}", game.GetEnemyScore(), game.GetMyScore());
            }
            game.Debug("Last turn we got {0} points", game.GetLastTurnPoints()[Consts.ME]);
            game.Debug("Last turn enemy got {0} points", game.GetLastTurnPoints()[Consts.ENEMY]);

            // We can check the size of the map
            game.Debug("The map has {0} rows and {1} columns", game.GetRows(), game.GetCols());

            // A turn may only take up till one second - you can check the time if your code is very slow
            game.Debug("We have {0} time remaining for our turn", game.TimeRemaining());
            // Sleep 20 miliseconds so that we can see that the time remainning changes
            Thread.Sleep(90);
            game.Debug("Now we have {0} time remaining for our turn", game.TimeRemaining());

            // We can see what turn it is and how many there will be
            game.Debug("It is turn #{0} of max {1}", game.GetTurn(), game.GetMaxTurns());

            // We can see how many points we need go get
            game.Debug("We should get {0} points before the enemy", game.GetMaxPoints());

            // This is the attack radius - if the distance between ships is equal or less than this number then they will attack
            game.Debug("Attack radius is {0}", game.GetAttackRadius());

            // This is the number of turns it will take lost ships to return to the game
            game.Debug("Lost ships return in {0} turns", game.GetSpawnTurns());

        }
    }
}
