using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
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

        public int ID;

        public Fleet(int index, int size, List<int> f)
        {
            this.pirates = new List<int>();
            this.Allocate(index, size);
            this.ReSort(f);
        }

        /*blic int MinDistance(Location loc)
        {
            return pirates.ConvertAll(p => Bot.Game.GetMyPirate(p)).Select(pete => Bot.Game.Distance(loc, pete.Loc)).Concat(new[] {99999}).Min();
        }*/

        public bool Status()
        {
            /*int timeRemaining = 0;
            if (Bot.Game.GetIsland(this.Target).TeamCapturing == Consts.ME)
            {
                timeRemaining = Bot.Game.GetIsland(this.Target).CaptureTurns -
                                    Bot.Game.GetIsland(this.Target).TurnsBeingCaptured;
            }
            else
            {
                timeRemaining = Bot.Game.GetIsland(this.Target).CaptureTurns -
                                pirates.ConvertAll(p => Bot.Game.GetMyPirate(p))
                                    .Select(pete => Bot.Game.Distance(Bot.Game.GetIsland(this.Target).Loc, pete.Loc))
                                    .Concat(new[] {99999})
                                    .Min();
            }

            if (Ai.EnemiesNearLocation(timeRemaining + Bot.Game.GetAttackRadius(), Bot.Game.GetIsland(this.Target).Loc) > this.GetAlivePirates() - 1)
            {
                if (Ai.EnemiesNearLocation(timeRemaining, Bot.Game.GetIsland(this.Target).Loc) ==
                    this.GetAlivePirates())
                {
                    foreach (int i in pirates)
                    {
                        Pirate pete = Bot.Game.GetMyPirate(i);
                        if (pete.Loc == Bot.Game.GetIsland(this.Target).Loc)
                        {
                            foreach (Location location in pete.EnumerateLocationsNearPirate())
                            {
                                if (Bot.Game.GetPirateOn(location) == null)
                                {
                                    Bot.Game.Debug("Manuevering");
                                    pete.SetSail(location);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Bot.Game.Debug(this.ID + " Escaping 1");
                    return true;
                }
            }
            */

            if (this.GetAlivePirates() < Ai.EstimatePiratesNearIsland(this.Target))
            {
                Bot.Game.Debug(this.ID + " Escaping 2");
                return true;
            }

            if (Bot.Game.GetIsland(this.Target).Owner == Consts.ME)
            {
                return true;
            }

            /*if (Bot.Game.GetMyPirate(this.pirates.First()).IsLost)
            {
                return true;
            }*/

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

        private bool alternate;

        public void SailAll()
        {
            Direction d;
            foreach (int p in this.pirates)
            {
                Pirate pete = Bot.Game.GetMyPirate(p);
                if (!pete.IsLost)
                {
                    if (alternate)
                    {
                        d = Bot.Game.GetDirections(pete, Bot.Game.GetIsland(this.Target).Loc).First();
                    }
                    else
                    {
                        try
                        {
                            d = Bot.Game.GetDirections(pete, Bot.Game.GetIsland(this.Target).Loc)[1];
                        }
                        catch (Exception)
                        {
                            d = Bot.Game.GetDirections(pete, Bot.Game.GetIsland(this.Target).Loc).First();    
                        }
                        
                    }
                    
                    Bot.Game.SetSail(pete, d);
                }
            }
            alternate = !alternate;
        }

        public int GetAlivePirates()
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

            int enemyCount = 0;
            foreach (Pirate enPirate in Bot.Game.AllEnemyPirates())
            {
                if (Bot.Game.Distance((Bot.Game.GetMyPirate(this.pirates.First())), isle) <
                    Bot.Game.Distance(enPirate, isle))
                    enemyCount++;
            }
            if (enemyCount >= this.GetAlivePirates())
                score -= 1000000;
            
            score += (isle.Value - 1)*100;
            score -= Bot.Game.Distance(isle.Loc, Bot.Game.GetMyPirate(this.pirates.First()).Loc);
            
            return score;
        }

        public string GetTargetLocationString()
        {
            return Bot.Game.GetIsland(this.Target).Loc.ToString();

        }

        public void ReSort(List<int> friends)
        {
            this.priorities = new List<int>(Bot.Game.Islands().ConvertAll(isle => isle.Id));
            this.priorities.Sort((b, a) => GetScore(a, friends).CompareTo(GetScore(b, friends)));
            this.Target = this.priorities.First();
        }
        
    }
}


