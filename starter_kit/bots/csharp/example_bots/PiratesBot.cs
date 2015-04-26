using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pirates;

namespace pirates_example
{
    public class PiratesBot : Pirates.IPirateBot
    {
        public void DoTurn(Pirates.IPirateGame game)
        {
            /**
            * In this example we will see how to use all the API that deals with the pirate ships
            * First, we will see functions for getting pirates (enemy pirates, my pirates, etc.. )
            * Then, we will use the properties/members of each pirate
            * Finally, we will show how to make your own function for returning a list of pirates.
            */
            game.Debug("You have {0} Pirates in the game", game.AllMyPirates().Count());
            game.Debug("You have {0} Pirates on the map this turn", game.MyPirates().Count());

            game.Debug("The enemy has {0} Pirates in the game", game.AllEnemyPirates().Count());
            game.Debug("The enemy has {0} Pirates on the map this turn", game.EnemyPirates().Count());

            // check if we have no more pirates on the map
            if (game.MyPirates().Count() == 0)
            {
                game.Debug("We have no pirates! nothing to do..");
                return;
            }

            // check if we have less pirates than the enemy
            if (game.EnemyPirates().Count() > game.MyPirates().Count())
            {
                game.Debug("Oh no! There are more enemies on the map!");
            }

            
            // choose some pirate
            // game.GetMyPirate(int id) gets the pirate that has that id.
            Pirate my_pirate = game.GetMyPirate(1);
            // choose some enemy pirate
            Pirate enemy = game.GetEnemyPirate(2);
            // choose some island
            Island target_island = game.GetIsland(2);

            if (my_pirate.IsLost)
            {
                // our pirate is lost... we shouldn't do anything
                game.Debug("Pirate is lost :( ");
                return;
            }

            // print some information about our pirate:
            game.Debug("Us >> Id: {0}, Owner: {1}, Location {2}, InitialLocation: {3}" , my_pirate.Id, my_pirate.Owner, my_pirate.Loc, my_pirate.InitialLocation);
            // print some information about enemy pirate:
            game.Debug("Enemy >> Id: {0}, Owner: {1}, Location {2}, InitialLocation: {3}", enemy.Id, enemy.Owner, enemy.Loc, enemy.InitialLocation);

            Direction dir;
            // if the enemy isn't lost we will go to it - otherwise we will go to the island
            if (!enemy.IsLost)
            {
                // enemy is in the game! go to it.
                game.Debug("Enemy isnt lost - going to enemy. distance is: {0}", game.Distance(my_pirate, enemy));
                dir = game.GetDirections(my_pirate, enemy).Last();
            }
            else
            {
                // enemy isn't in the game. go to the island.
                game.Debug("Enemy is lost! and will return in {0} turns. Going to island", enemy.TurnsToRevive);
                dir = game.GetDirections(my_pirate, target_island).Last();
            }
            game.SetSail(my_pirate, dir);

            // check if enemy is on the island
            if (game.GetPirateOn(target_island) == enemy)
            {
                game.Debug("Enemy is on the island!");
            }

            // check if we are on the island
            if (game.GetPirateOn(target_island) == my_pirate)
            {
                game.Debug("My pirate is on the island!");
            }

            // print how many lost pirates we have with the function we write
            game.Debug("We have {0} lost pirates", this.MyLostPirates(game).Count());
        }


        // This function will return us a list of our lost pirates
        // For advanced programmers - you can implement this function in one line only!
        // Hint: return game.AllMyPirates().Where(pirate => pirate.IsLost);
        List<Pirate> MyLostPirates(Pirates.IPirateGame game) {

            // Create an empty list
            List<Pirate> lostPirates = new List<Pirate>();

            // for loop - go through all the pirates in AllMyPirates()
            foreach (Pirate pir in game.AllMyPirates()) {
                // check if pirate is lost
                if (pir.IsLost) {
                    // add lost pirate to the list
                    lostPirates.Add(pir);
                }
            }

            // return the list
            return lostPirates;
        }

    }
}
