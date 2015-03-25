using Pirates;

namespace SarcasticBot
{
    public class FriendPirate : SmartPirate
    {
        /// <summary>
        /// Friend Pirate Constructor - Represents a pirate of our side
        /// </summary>
        /// <param name="index">Pirate index</param>
        public FriendPirate(int index)
        {
            this.Index = index;
        }

        /// <summary>
        /// Set a pirate to move at a direction
        /// </summary>
        /// <param name="direction">The direction to move to</param>
        public void SetSail(Direction direction)
        {
            Bot.Game.SetSail(Bot.Game.GetMyPirate(this.Index),direction);
        }


        public bool IsAlive()
        {
            return Bot.Game.GetMyPirate(this.Index).IsLost;
        }
    }
}