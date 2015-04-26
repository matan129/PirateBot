
package bots;
import pirates.game.PirateGame;

// A utility class to demonstrate the usage of another file in a project
public class MyBotUtils {

    // Returns true if the turn number divides by 100 with no remainder
    public static boolean isTurnMultipleOf100(PirateGame game) {
        return game.getTurn() % 100 == 0;
    }

}
