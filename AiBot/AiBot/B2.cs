using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;


namespace AiBot
{
    public class B2 : IPirateBot
    {
        public static IPirateGame Game;

        private Commander _commander;
        private bool _commander_inited = false;

        private int[] _initialConfig = {2, 2, 1, 1};

        public void DoTurn(IPirateGame state)
        {
            Game = state;

            if (!this._commander_inited)
            {
                this._commander = new Commander(this._initialConfig);
            }

            this._commander.Play();
        }
    }


    public class Commander
    {
        public Commander(int[] config)
        {
            
        }

        public void Play()
        {
            B2.Game.Debug(String.Join(", ", Ai.GetEnemyConfig()));
        }
    }

    public static class Ai
    {
        public static int[] GetEnemyConfig() //[2,2,2] is 2+2+2 config (indexes irrelevant)
        {
            List<int> ePirates = GetEnemyAlivePirates();
            List<List<int>> clusters = new List<List<int>>();

            for (int i = 0; i < ePirates.Count; i++)
            {
                int clustersIn = 0;
                foreach (List<int> cluster in clusters)
                {
                    if (InCluster(i, cluster))
                    {
                        cluster.Add(i);
                        clustersIn++;
                    }
                }

                if (clustersIn == 0)
                {
                    clusters.Add(new List<int>() {i});
                }
            }
            

            List<List<int>> indexesToJoin = new List<List<int>>();

            for (int i = 0; i < clusters.Count; i++)
            {
                indexesToJoin.Add(new List<int>() {i});

                for (int k = 0; k < clusters.Count; k++)
                {
                    if (k != i)
                    {
                        List<IEnumerable<int>> intersectionList = new List<IEnumerable<int>>();

                        foreach (int j in indexesToJoin.Last())
                        {
                            intersectionList.Add(clusters[j].Intersect(clusters[k]));
                        }

                        foreach (IEnumerable<int> intersection in intersectionList)
                        {
                            if (intersection.Any())
                            {
                                indexesToJoin.Last().Add(k);
                            }
                        }
                    }
                }
            }

            List<List<int>> joinedClusters = new List<List<int>>();

            for (int i = 0; i < indexesToJoin.Count; i++)
            {
                joinedClusters.Add(new List<int>());

                foreach (int cIndex in indexesToJoin[i].Distinct())
                {
                    joinedClusters.Last().AddRange(clusters[cIndex]);
                }
            }
            

            joinedClusters = joinedClusters.Distinct().ToList();
            for (int i = 0; i < joinedClusters.Count; i++)
            {
                joinedClusters[i] = joinedClusters[i].Distinct().ToList();
            }
            
            int[] enemyConfig = new int[joinedClusters.Count];

            for (int i = 0; i < enemyConfig.Length; i++)
            {
                enemyConfig[i] = joinedClusters[i].Count;
            }

            if (enemyConfig.Sum() != 6)
            {
                B2.Game.Debug(String.Join(", ", enemyConfig));
                throw new Exception("Erm... did the enemy get backup or what?");
            }

            return enemyConfig;
        }

        private static List<int> GetEnemyAlivePirates()
        {
            List<Pirate> ePirates = B2.Game.AllEnemyPirates();
            ePirates.RemoveAll(x => B2.Game.EnemyLostPirates().Contains(x));
            int[] alive = new int[ePirates.Count];

            for (int i = 0; i < alive.Length; i++)
            {
                alive[i] = ePirates[i].Id;
            }

            return alive.ToList();
        }

        private static bool InCluster(int ePirateIndex, List<int> cluster)
        {
            Pirate testPirate = B2.Game.GetEnemyPirate(ePirateIndex);
            foreach (int pIndex in cluster)
            {
                Pirate pete = B2.Game.GetEnemyPirate(pIndex);

                if (InRange(pete.Loc, testPirate.Loc))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool InRange(Location l1, Location l2, int range = 2)
        {
            return B2.Game.Distance(l1, l2) <= range;
        }
    }
}
