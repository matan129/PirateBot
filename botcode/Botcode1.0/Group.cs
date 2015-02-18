using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pirates;

namespace Botcode1_0
{
    public class GroupBABA
    {
        private List<Pirate> gPirates{ get; set; }     
        private IPirateGame game;
        public Island target {get; set;}

        public Group(IPirateGame g, int index, int size, List<Island> f)
        {
            this.game = g;
            this.gPirates = new List<Pirate>();
            this.Allocate(index,size);

            this.ReSort(f);
        }


        private void Allocate(int index, int size)
        {
            for (int i = index; i < index + size; i++)
            {
                gPirates.Add(game.GetMyPirate(i));
            }
        }

        public void SailAll()
        {
            Direction d;            
            foreach (Pirate p in this.gPirates)
            {
                d = game.GetDirections(p, this.target.Loc).First();
                game.SetSail(p,d);                
            }
        }

        private static int GetDistnace(Location Loc1, Location Loc2)
        {
            return Math.Abs(Loc1.Row - Loc2.Row) + Math.Abs(Loc1.Col - Loc2.Col);
        }

        public int GetScore(Island t, List<Island> friends)
        {
            int score = 0;
            if (friends.Contains(t))
            {
                score  -= 1000000;
            }

            if (t.Owner == Consts.ME)
            {
                score -= 1000000;
            }

            score -= GetDistnace(t.Loc, this.gPirates.First().Loc);
            return score;           
        }

        public void ReSort(List<Island> friends)
        {
            List<Island> allIslands = this.game.Islands();

            foreach (Island isle in allIslands)
            {
                if (GetScore(isle,friends) > GetScore(this.target,friends))
                {
                    this.target = isle;
                }
            }
           // temp.Sort(delegate(Island L1, Island L2) { return this.GetScore(L1, friends) - this.GetScore(L2, friends); });
        }        
    }
}
