package bots;

import pirates.game.PirateBot;
import pirates.game.PirateGame;


public class MyBot implements PirateBot{

    @Override
    public void doTurn(PirateGame game) {
        // Demonstrating usage of another file
        if (MyBotUtils.isTurnMultipleOf100(game)) {
            game.debug("Turn divides by 0");
        }
    }

}
