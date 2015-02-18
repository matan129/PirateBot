using System;
using System.Collections.Generic;
using Pirates;
using System.Linq;

namespace Botcode1_0
{
    public class MyNewBot : Pirates.IPirateBot
    {
        const int SIZE = 3;
        bool flag = false;
        Group g1, g2;

        

        public void DoTurn(IPirateGame game)
        {
            
            if (!flag)
            {
                g1 = new Group(game, 0, SIZE, new List<Island>());
                g2 = new Group(game, SIZE, SIZE, new List<Island>() { game.GetIsland(g1.target) });
                flag = true;
            }

            /*if (game.GetIsland(g1.target).Owner ==Consts.ME)
            {
                g1.numOfDeath = 0;
                g1.numOfDeath = 1;
            }*/

            if (g2.numOfDeath > 0)
            {
                g2.numOfDeath = 0;
                g2.gPirates.Add(0);
                g1.gPirates.Remove(0);
            }

            
                if (g1.Status())
                {
                    g1.ReSort(new List<Island>() { game.GetIsland(g2.target) });
                }

                g1.SailAll();

                if (g2.Status())
                {
                    g2.ReSort(new List<Island>() { game.GetIsland(g1.target) });
                }

                g2.SailAll();
                     
        }
    }




    public class Group
    {
        public List<int> gPirates { get; set; }
        private IPirateGame game;
        public int target;
        public int numOfDeath = 0;

        public Group(IPirateGame g, int index, int size, List<Island> f)
        {
            this.game = g;
            this.gPirates = new List<int>();
            this.Allocate(index, size);

            this.ReSort(f);
        }

        public bool Status()
        {
            if (this.game.GetIsland(this.target).Owner == Consts.ME)
            {
                this.numOfDeath = 0;
                return true;
            }

            if (this.game.GetMyPirate(this.gPirates.First()).IsLost)
            {
                if (this.game.GetMyPirate(this.gPirates.First()).TurnsToRevive == 1)
                    this.numOfDeath++;
                return true;
            }

            if (this.game.GetIsland(this.target).TeamCapturing == Consts.ENEMY)
            {
                return true;
            }

            return false;
        }
        private void Allocate(int index, int size)
        {
            for (int i = index; i < index + size; i++)
            {
                gPirates.Add(i);
            }

        }

        public void SailAll()
        {

            Direction d;
            foreach (int p in this.gPirates)
            {
                Pirate pete = this.game.GetMyPirate(p);
                if (!pete.IsLost)
                {
                d = game.GetDirections(pete, this.game.GetIsland(this.target).Loc).First();
                this.game.SetSail(pete, d);
				}
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
                score -= 1000000;
            }
            if (t.Owner == Consts.ME && t.TeamCapturing != Consts.ENEMY)
            {
                score -= 1000000;
            }
            

            score -= GetDistnace(t.Loc, this.game.GetMyPirate(this.gPirates.First()).Loc);

            return score;
        }

        public void ReSort(List<Island> friends)
        {
            this.target = 0;
            List<Island> allIslands = this.game.Islands();

            for (int i = 1; i < this.game.Islands().Count; i++)
            {
                if (GetScore(this.game.GetIsland(i), friends) > GetScore(this.game.GetIsland(this.target), friends))
                {
                    this.target = i;
                }
            }
            // temp.Sort(delegate(Island L1, Island L2) { return this.GetScore(L1, friends) - this.GetScore(L2, friends); });
        }

        public bool compareTarget()
        {
            if (this.game.GetIsland(this.target).Owner == Consts.NO_OWNER)
                return true;
            else
                return false;
        }
    }
}
