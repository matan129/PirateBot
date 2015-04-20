#region #Usings

using System.Collections.Generic;
using System.Linq;
using Pirates;

#endregion

namespace Britbot
{
    internal class Zone
    {
        #region Static Fields & Consts

        public static List<Zone> Zones;

        #endregion

        #region Fields & Properies

        public readonly int Capacity;
        public int AvaliableSpots { get; private set; }
        public List<int> PiratesInZone { get; private set; }
        public List<int> Groups { get; private set; }

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     Static ctor
        /// </summary>
        static Zone()
        {
            Zone.Zones = new List<Zone>();
            Zone.IdentifyZones();
        }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="avaliableSpots"></param>
        public Zone(int avaliableSpots)
            : this()
        {
            this.Capacity = avaliableSpots;
            this.AvaliableSpots = avaliableSpots;
        }

        /// <summary>
        ///     Private ctor
        /// </summary>
        private Zone()
        {
            this.PiratesInZone = new List<int>();
            this.Groups = new List<int>();
        }

        #endregion

        /// <summary>
        ///     Marks that some pirates are anout to be allocated in this zone.
        /// </summary>
        /// <param name="count"></param>
        public void AddGroup(int count)
        {
            if (this.AvaliableSpots >= count)
            {
                this.AvaliableSpots -= count;
                this.Groups.Add(count);
            }
            else
            {
                throw new AllocationException("Not enough space in the zone");
            }
        }

        /// <summary>
        ///     Determines if a pirate is in the zones
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool InZone(Pirate p)
        {
            foreach (int pete in this.PiratesInZone)
            {
                if (Bot.Game.InRange(p, Bot.Game.GetMyPirate(pete)))
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     Splits all out pirates into different zones
        /// </summary>
        /// <returns></returns>
        private static void IdentifyZones()
        {
            IEnumerable<Pirate> pirates = Bot.Game.AllMyPirates();

            foreach (Pirate pete in pirates)
            {
                Zone currentZone = new Zone();
                currentZone.PiratesInZone.Add(pete.Id);

                // the following line is a closure, so some compilers may break it if we don't re assign it
                Pirate temp = pete;
                IEnumerable<Zone> conatainsPete = Zone.Zones.Where(z => z.InZone(temp));

                Zone.Zones.RemoveAll(z => z.InZone(pete));

                foreach (Zone z in conatainsPete)
                {
                    currentZone.PiratesInZone.AddRange(z.PiratesInZone);
                }

                Zone.Zones.Add(currentZone);
            }
        }

        /// <summary>
        ///     Splits a zone to a required config
        /// </summary>
        /// <param name="physicalZone"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IEnumerable<List<int>> SplitZone(List<int> physicalZone, List<int> config)
        {
            Bot.Game.Debug("Physical allocation in progrees. Configuration of " + string.Join(",", config) +
                           "at zone with " + physicalZone.Count + " pirates");

            //Check if the data is correct
            if (physicalZone.Count != config.Sum())
                throw new AllocationException(
                    string.Format("Bad zone split. Expected {0} pirates and got {1} in the config", physicalZone.Count,
                        config.Sum()));

            //yield each group in the zone
            foreach (int groupInZone in config)
            {
                yield return Zone.SelectGroup(ref physicalZone, groupInZone);
            }
        }

        /// <summary>
        ///     Selects a group from a given pool of pirates (no pun intneded)
        /// </summary>
        /// <param name="avaliablePirates">The IDs of the pirates avaliable to choose</param>
        /// <param name="amount">How many pirates should this method choose from the list?</param>
        /// <returns>List of pirate IDs for a group</returns>
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

                //find the left and top most pirate
                foreach (Pirate pete in pirates)
                {
                    if (pete.Loc.Col < leftMax)
                    {
                        leftMost = pete;
                    }
                }

                //the left most pirate is the "anchor" for the group
                groupPiratesId.Add(leftMost.Id);

                while (groupPiratesId.Count < amount)
                {
                    //find the closest pirate to the currenly selected group (greedy alg)
                    Pirate current = Zone.AverageClosest(groupPiratesId.ConvertAll(p => Bot.Game.GetMyPirate(p)),
                        pirates);

                    groupPiratesId.Add(current.Id);
                    avaliablePirates.Remove(current.Id);
                }
            }

            return groupPiratesId;
        }

        /// <summary>
        ///     Finds the closest pirate to another pirate from a group of avaliable pirates
        /// </summary>
        /// <param name="group">The already selected pirates</param>
        /// <param name="pool">The avaliable pirates to choose from</param>
        /// <returns>A pirate closest on average to the give pirates</returns>
        public static Pirate AverageClosest(IEnumerable<Pirate> group, IEnumerable<Pirate> pool)
        {
            int minDistance = 999999;

            Location average = new Location(0, 0);

            foreach (Pirate pete in group)
            {
                average.Col += pete.Loc.Col;
                average.Row += pete.Loc.Row;
            }

            average.Col /= group.Count();
            average.Row /= group.Count();

            Pirate minPirate = pool.First();
            foreach (Pirate pete in pool)
            {
                int d = Bot.Game.Distance(average, pete.Loc);

                if (d < minDistance)
                {
                    minDistance = d;
                    minPirate = pete;
                }
            }

            return minPirate;
        }
    }
}