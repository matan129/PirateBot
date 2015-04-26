package bots;

import pirates.game.Constants;
import pirates.game.Direction;
import pirates.game.Island;
import pirates.game.Pirate;
import pirates.game.PirateBot;
import pirates.game.PirateGame;

/**
 * In this example we will see how to use all the API regarding ghost-ships. The Bot is not very smart
 * but it will show you how to use all the functions.
 * Becoming a ghost ship (=cloak) makes your ship invisible to the other team but your ship can not 
 * attack, support or capture islands until it reveals itself.
 * Only one of your ships may be cloaked on a turn. After the ship reveals itself there is a cooldown for 
 * some turns in which you cannot cloak again. The cooldown is a number which goes down by one
 * every turn after you reveal.
 * For example, if the cooldown is 50 - you will wait 50 turns until you can cloak again.
 * Important: making a ship into a ghost or back with: 'cloak' and 'reveal' are instead of moving on that turn.
 * Enjoy the Example!
 */
public class MyBot implements PirateBot{

	@Override
	public void doTurn(PirateGame game) {
        // use game.GetMyCloaked() to find your ghost-ship. It will return null if you dont have one now.
        game.debug("My cooldown: %s. Other team cooldown: %s", game.getCloakCooldowns()[0], game.getCloakCooldowns()[1]);
        Pirate ghost_pirate = game.getMyCloaked();
        game.debug("GetMyCloaked returns: %s", game.getMyCloaked());
        if (ghost_pirate != null)
        {
            game.debug("We have a ship that is ghost");
        } else {
            game.debug("We do not have a ship that is ghost");
        }

        // use game.CanCloak() to see if you can use cloak on this turn. What does it check?
        // it checks that you don't have a ghost ship and that the cooldown is 0
        game.debug("Can we make a ship into a ghost this turn? %s", game.canCloak());
        if (!game.canCloak()) {
            // We can use game.GetCloakCooldowns()[0] to see in how many turns we can cloak
            // To see in how many turns the other player can cloak use game.GetCloakCooldowns()[1]
            game.debug("We can ghost ship in %s turns", game.getCloakCooldowns()[0]);
        }

        // pick some island to go to
        Island island = game.getIsland(2);
        game.debug("island with id: %s , which belongs to %s", island.getId(), island.getOwner());
        
        // if we don't have a ghost ship - choose another one.
        if (ghost_pirate == null) {
             ghost_pirate = game.getMyPirate(3);
        }
        // use IsCloaked to see if a pirate is a ghost ship or not
        game.debug("Pirate %s is ghost: %s", ghost_pirate.getId(), ghost_pirate.isCloaked());
        
        // if we are on island && it doesnt belong to us 
        if ((game.getPirateOn(island) == ghost_pirate) && (island.getOwner() != Constants.ME)) {
            // if we are ghost ship it is important to reveal so that we can capture the island
            if (ghost_pirate.isCloaked()) {
                game.debug("About to reveal the ghost ship!");
                game.reveal(ghost_pirate);
                return;
            } else {
                // don't do anything - we need to wait to capture the island
                return;
            }
        }

        // if we are not ghost ship && we can become ghost.
        if ((!ghost_pirate.isCloaked()) && (game.canCloak())) {
            // we should become ghost ship
            game.debug("Making our ship a ghost now!");
            game.cloak(ghost_pirate);
            return;
        } else {
            // we are cloaked already - just sail to the island
            game.debug("Sailing to island!");
            Direction d = game.getDirections(ghost_pirate, island).get(0);
            game.setSail(ghost_pirate, d);
        }
	}
}
