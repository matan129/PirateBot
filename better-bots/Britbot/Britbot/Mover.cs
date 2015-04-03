using System.Collections.Generic;
using Pirates;

namespace Britbot
{
    /// <summary>
    /// This class is used for the physical moving of pirates in the game
    /// </summary>
    static class Mover
    {
        /// <summary>
        /// Moves pirate according to the moves dictionary it got
        /// </summary>
        /// <param name="moves">A dictionary of consists of entries that have a pirate and a corresponding direction to move at</param>
        public static void MoveAll(Dictionary<Pirate, Direction> moves)
        {
            //iterate over all moves and execute them
            foreach (KeyValuePair<Pirate, Direction> move in moves)
            {
                if(!move.Key.IsLost)
                    Bot.Game.SetSail(move.Key,move.Value);
            }
        }
    }
}
