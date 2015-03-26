using System.Collections.Generic;
using Pirates;
namespace SarcasticBot
{
    public static class SmartGame
    {
        public static List<SmartIsland> SmartIslands;
        public static List<FriendPirate> MySmartPirates;
        public static List<EnemyPirate> EnemySmartPirates;

        //pirates
       public static Pirate GetMyPirate(int index)
        {
            return Bot.Game.GetMyPirate(index);
        }
       public static Pirate GetEnemyPirate(int index)
       {
           return Bot.Game.GetEnemyPirate(index);
       }
    }
}