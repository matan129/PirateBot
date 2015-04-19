#region #Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Pirates;

#endregion

namespace Britbot
{
    internal class Zone
    {
        #region Fields & Properies

        public readonly int Capacity;
        public List<int> PiratesInZone = new List<int>();
        public int AvaliablePirates { get; private set; }
        public List<int> Groups { get; private set; }

        #endregion

        #region Constructors & Initializers

        public Zone(int avaliablePirates)
        {
            this.Capacity = avaliablePirates;
            this.AvaliablePirates = avaliablePirates;
            this.Groups = new List<int>();
        }

        #endregion

        public void AddGroup(int count)
        {
            if (this.AvaliablePirates >= count)
            {
                this.AvaliablePirates -= count;
                this.Groups.Add(count);
            }
            else
            {
                throw new Exception("Not enough space in the zone");
            }
        }

        /// <summary>
        ///     Determines if a pirate is in the zones
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool IsInGroup(Pirate p)
        {
            foreach (int i in this.PiratesInZone)
            {
                Pirate pete = Bot.Game.GetMyPirate(i);

                if (Bot.Game.InRange(p, pete))
                    return true;
            }

            return false;
        }

        private static List<List<int>> Zones;

        /// <summary>
        ///     Splits all out pirates into different zones
        /// </summary>
        /// <returns></returns>
        internal static List<List<int>> IdentifyZones()
        {
            if (Zone.Zones != null)
                return Zones;

            List<Zone> zones = new List<Zone>();
            IEnumerable<Pirate> pirates = Bot.Game.AllMyPirates();

            foreach (Pirate pete in pirates)
            {
                Zone temp = new Zone(0);

                List<Zone> conatainsPete = zones.Where(z => z.IsInGroup(pete)).ToList();

                zones.RemoveAll(z => z.IsInGroup(pete));

                foreach (Zone z in conatainsPete)
                {
                    temp.PiratesInZone.AddRange(z.PiratesInZone);
                }

                zones.Add(temp);
            }

            List<List<int>> res = new List<List<int>>(zones.Count);

            foreach (Zone zone in zones)
            {
                res.Add(zone.Groups);
            }

            Zone.Zones = res;
            return res;
        }

        /// <summary>
        ///     Splits a zone to a required config
        /// </summary>
        /// <param name="physicalZone"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IEnumerable<List<int>> SplitZone(List<int> physicalZone, List<int> config)
        {
            if (physicalZone.Count != config.Sum())
                throw new AllocationException(
                    string.Format("Bad zone split. Expected {0} pirates and got {1} in the config", physicalZone.Count,
                        config.Sum()));

            foreach (int groupInZone in config)
            {
                yield return Zone.SelectGroup(ref physicalZone, groupInZone);
            }
        }

        private static List<int> SelectGroup(ref List<int> avaliablePirates, int amount)
        {
            List<int> groupPiratesId = new List<int>(amount);

            if (avaliablePirates.Count == amount)
            {
                for (int i = 0; i < avaliablePirates.Count; i++)
                {
                    groupPiratesId.Add(avaliablePirates[i]);
                    avaliablePirates.Remove(avaliablePirates[i]);
                }
            }
            else
            {
                List<Pirate> pirates = avaliablePirates.ConvertAll(p => Bot.Game.GetMyPirate(p));

                //choose the left most pirate, and the top one if the're a conflit
                Pirate leftMost = pirates.First();
                int leftMax = 999999;

                //sort the pirates so the top most ones are first
                pirates.Sort((a, b) => b.Loc.Row.CompareTo(a.Loc.Row));

                foreach (Pirate pete in pirates)
                {
                    if (pete.Loc.Col < leftMax)
                    {
                        leftMost = pete;
                    }
                }

                while (groupPiratesId.Count < amount)
                {
                    Pirate current = leftMost.FindClosest(pirates);
                    groupPiratesId.Add(current.Id);

                    avaliablePirates.Remove(current.Id);

                    leftMost = current;
                }
            }

            return groupPiratesId;
        }
    }
}