using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace WeekOneBot
{
    class Fleet
    {
        private IPirateGame game;
        private List<int> pirates { get; set; }
        public int Target { get; set; }
        private List<int> priorities; 

        public Fleet(IPirateGame g, int index, int size, List<int> f)
        {
            this.game = g;
            this.pirates = new List<int>();
            this.Allocate(index, size);
            this.ReSort(f);
        }

        public bool Status()
        {
            if (this.game.GetIsland(this.Target).Owner == Consts.ME)
            {
                return true;
            }

            if (this.game.GetMyPirate(this.pirates.First()).IsLost)
            {
                return true;
            }

            if (this.game.GetIsland(this.Target).TeamCapturing == Consts.ENEMY)
            {
                return true;
            }

            return false;
        }

        private void Allocate(int index, int size)
        {
            for (int i = index; i < index + size; i++)
            {
                pirates.Add(i);
            }
        }

        public void SailAll()
        {
            Direction d;
            foreach (int p in this.pirates)
            {
                Pirate pete = this.game.GetMyPirate(p);
                if (!pete.IsLost)
                {
                    d = game.GetDirections(pete, this.game.GetIsland(this.Target).Loc).First();
                    this.game.SetSail(pete, d);
                }
            }
        }

        private static int GetDistance(Location Loc1, Location Loc2)
        {
            return Math.Abs(Loc1.Row - Loc2.Row) + Math.Abs(Loc1.Col - Loc2.Col);
        }

        public int GetScore(int t, List<int> friends)
        {
            int score = 0;
            Island isle = game.GetIsland(t);
            if (friends.Contains(t))
            {
                score -= 1000000;
            }
            if (isle.Owner == Consts.ME && isle.TeamCapturing != Consts.ENEMY)
            {
                score -= 1000000;
            }

            score -= GetDistance(isle.Loc, this.game.GetMyPirate(this.pirates.First()).Loc);

            return score;
        }

        public void ReSort(List<int> friends)
        {
            this.priorities = new List<int>(game.Islands().ConvertAll(isle => isle.Id));
            this.priorities.Sort((b, a) => GetScore(a, friends).CompareTo(GetScore(b, friends)));
            this.Target = this.priorities.First();

            foreach (int i in priorities)
            {
                game.Debug(i.ToString());
            }
            game.Debug("---\nTarget: {0}",this.Target);
        }
    }
}


