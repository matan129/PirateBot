#region Usings

using System.Collections.Generic;
using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     This class is used for the physical moving of pirates in the game
    /// </summary>
    internal static class Mover
    {
        /// <summary>
        ///     Moves pirate according to the moves dictionary it got
        /// </summary>
        /// <param name="moves">A dictionary of consists of entries that have a pirate and a corresponding direction to move at</param>
        public static void MoveAll(Dictionary<Pirate, Direction> moves)
        {
            TheD.BeginTime("MoveAll");
            try
            {
                //iterate over all moves and execute them
                foreach (KeyValuePair<Pirate, Direction> move in moves)
                {
                    if (!move.Key.IsLost)
                        Bot.Game.SetSail(move.Key, move.Value);
                }
            }
            catch
            {
                //don't move anything if we crash, but this will save us from timeout
                foreach (Pirate p in Bot.Game.AllMyPirates())
                {
                    if (!p.IsLost)
                        Bot.Game.SetSail(p, Direction.NOTHING);
                }
            }
            TheD.StopTime("MoveAll");
        }
    }
}