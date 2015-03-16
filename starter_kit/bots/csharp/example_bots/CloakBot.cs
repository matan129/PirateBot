using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pirates;

namespace cloak_example
{
    public class CloakBot : Pirates.IPirateBot
    {
        public void DoTurn(Pirates.IPirateGame game) {
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

            // use game.GetMyCloaked() to find your ghost-ship. It will return null if you dont have one now.
            game.Debug("My cooldown: {0}. Other team cooldown: {1}", game.GetCloakCooldowns()[0], game.GetCloakCooldowns()[1]);
            Pirate ghost_pirate = game.GetMyCloaked();
            game.Debug("GetMyCloaked returns: {0}", game.GetMyCloaked());
            if (ghost_pirate != null)
            {
                game.Debug("We have a ship that is ghost");
            } else {
                game.Debug("We do not have a ship that is ghost");
            }

            // use game.CanCloak() to see if you can use cloak on this turn. What does it check?
            // it checks that you don't have a ghost ship and that the cooldown is 0
            game.Debug("Can we make a ship into a ghost this turn? {0}", game.CanCloak());
            if (!game.CanCloak()) {
                // We can use game.GetCloakCooldowns()[0] to see in how many turns we can cloak
                // To see in how many turns the other player can cloak use game.GetCloakCooldowns()[1]
                game.Debug("We can ghost ship in {0} turns", game.GetCloakCooldowns()[0]);
            }

            // pick some island to go to
            Island island = game.GetIsland(2);
            game.Debug("island with id: {0} , which belongs to {1}", island.Id, island.Owner);
            
            // if we don't have a ghost ship - choose another one.
            if (ghost_pirate == null) {
                 ghost_pirate = game.GetMyPirate(3);
            }
            // use IsCloaked to see if a pirate is a ghost ship or not
            game.Debug("Pirate {0} is ghost: {1}", ghost_pirate.Id, ghost_pirate.IsCloaked);
            
            // if we are on island && it doesnt belong to us 
            if ((game.GetPirateOn(island) == ghost_pirate) && (island.Owner != Consts.ME)) {
                // if we are ghost ship it is important to reveal so that we can capture the island
                if (ghost_pirate.IsCloaked) {
                    game.Debug("About to reveal the ghost ship!");
                    game.Reveal(ghost_pirate);
                    return;
                } else {
                    // don't do anything - we need to wait to capture the island
                    return;
                }
            }

            // if we are not ghost ship && we can become ghost.
            if ((!ghost_pirate.IsCloaked) && (game.CanCloak())) {
                // we should become ghost ship
                game.Debug("Making our ship a ghost now!");
                game.Cloak(ghost_pirate);
                return;
            } else {
                // we are cloaked already - just sail to the island
                game.Debug("Sailing to island!");
                Direction d = game.GetDirections(ghost_pirate, island).First();
                game.SetSail(ghost_pirate, d);
            }
        }
    }
}
