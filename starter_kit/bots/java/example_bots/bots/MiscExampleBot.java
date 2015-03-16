package bots;

import javax.swing.plaf.basic.BasicInternalFrameTitlePane.MaximizeAction;

import pirates.game.Constants;
import pirates.game.PirateBot;
import pirates.game.PirateGame;

public class MyBot implements PirateBot{

	@Override
	public void doTurn(PirateGame game) {
        /**
         * In this example we will see a few useful utility functions
         * which will help you create more powerful Bots
         */

        // Use these functions to see the scores of each team and how many points we got on the last turn
        if (game.getEnemyScore() > game.getMyScore())
        {
            game.debug("We are losing! Enemy has %s points and we have %s", game.getEnemyScore(), game.getMyScore());
        }
        game.debug("Last turn we got %s points", game.getLastTurnPoints()[Constants.ME]);
        game.debug("Last turn enemy got %s points", game.getLastTurnPoints()[Constants.ENEMY]);

        // We can check the size of the map
        game.debug("The map has %s rows and %s columns", game.getRows(), game.getCols());

        // A turn may only take up till one second - you can check the time if your code is very slow
        game.debug("We have %s time remaining for our turn", game.getTimeRemaining());
        // Sleep 20 miliseconds so that we can see that the time remainning changes
        try {
			Thread.sleep(20);
		} catch (InterruptedException e) {
		}
        game.debug("Now we have %s time remaining for our turn", game.getTimeRemaining());

        // We can see what turn it is and how many there will be
        game.debug("It is turn #%s of max %s", game.getTurn(), game.getMaxTurns());

        // This is the attack radius - if the distance between ships is equal or less than this number then they will attack
        game.debug("Attack radius is %s", game.getAttackRadius());

        // This is the number of turns it will take lost ships to return to the game
        game.debug("Lost ships return in %s turns", game.getSpawnTurns());
        
        game.debug("Max cloak cooldown in turns: %s", game.getMaxCloakCooldown());


		
	}

}
