using System;
using System.CodeDom;
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
        private List<int> pirates { get; set; }
        public int Target { get; set; }
        private List<int> priorities; 

        public Fleet(int index, int size, List<int> f)
        {
            this.pirates = new List<int>();
            this.Allocate(index, size);
            this.ReSort(f);
        }

        public bool Status()
        {
            if (Bot.Game.GetIsland(this.Target).Owner == Consts.ME)
            {
                return true;
            }

            if (Bot.Game.GetMyPirate(this.pirates.First()).IsLost)
            {
                return true;
            }

            if (Bot.Game.GetIsland(this.Target).TeamCapturing == Consts.ENEMY)
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
                Pirate pete = Bot.Game.GetMyPirate(p);
                if (!pete.IsLost)
                {
                    d = Bot.Game.GetDirections(pete, Bot.Game.GetIsland(this.Target).Loc).First();
                    Bot.Game.SetSail(pete, d);
                }
            }
        }

        private int GetAlivePirates()
        {
            int sum = 0;
            foreach (int pirate in pirates)
            {
                Pirate pete = Bot.Game.GetMyPirate(pirate);
                if (!pete.IsLost)
                {
                    sum++;
                }
            }
            return sum;
        }

        public int GetScore(int t, List<int> friends)
        {
            int score = 0;
            Island isle = Bot.Game.GetIsland(t);
            if (friends.Contains(t))
            {
                score -= 10000;
            }
            if (isle.Owner == Consts.ME && isle.TeamCapturing != Consts.ENEMY)
            {
                score -= 10000;
            }

            if (this.GetAlivePirates() < Ai.EstimatePiratesNearIsland(t))
            {
                score -= 100000;
            }
            else
            {
                score += (this.GetAlivePirates() - Ai.EstimatePiratesNearIsland(t)) * 200;
            }

            score += (isle.Value - 1)*100;
            score -= Bot.Game.Distance(isle.Loc, Bot.Game.GetMyPirate(this.pirates.First()).Loc);
            
            return score;
        }

        public void ReSort(List<int> friends)
        {
            this.priorities = new List<int>(Bot.Game.Islands().ConvertAll(isle => isle.Id));
            this.priorities.Sort((b, a) => GetScore(a, friends).CompareTo(GetScore(b, friends)));
            this.Target = this.priorities.First();
        }
    }
}


