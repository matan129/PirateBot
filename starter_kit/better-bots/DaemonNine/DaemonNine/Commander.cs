using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace DaemonNine
{
    internal static class Commander
    {
        public static Dictionary<int, int> Assignments { get; private set; }
        public static List<int> AIslands { get; private set; }

        public static List<int> Epc { get; private set; }
        public static List<int> Mpc { get; private set; }

        static Commander()
        {
            Assignments = new Dictionary<int, int>();
            AIslands = new List<int>();

            Epc = Bot.Game.AllEnemyPirates().ConvertAll(p => p.Id);
            Mpc = Bot.Game.AllMyPirates().ConvertAll(p => p.Id);
        }

        private static void Play()
        {
            for (int i = 0; i < Ai.CBots; i++)
            {
                MovePirate();
            }

            List<int> opirate = GetFreePirates();
            int newAssigments = FindAttack(opirate);

            if (newAssigments != 0)
            {
                foreach (int p in opirate)
                {
                    SetMove(Bot.Game.GetMyPirate(p), Bot.Game.GetDirections(Bot.Game.GetMyPirate(p),Bot.Game.GetIsland(newAssigments)).First());
                }
            }

            MoveAll();
        }

        private static void MoveAll()
        {
            throw new NotImplementedException();
        }

        private static void SetMove(Pirate getMyPirate, Direction first)
        {
            throw new NotImplementedException();
        }

        private static int FindAttack(List<int> opirate)
        {
            throw new NotImplementedException();
        }

        private static List<int> GetFreePirates()
        {
            return Mpc.Where(p => Assignments.ContainsKey(p)).ToList();
        }

        private static void MovePirate()
        {
            

        }
    }
}
