#region #Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace Britbot
{
    /// <summary>
    ///     This is a utility class for doing the hard job of choosing spesific pirate for groups, etc
    /// </summary>
    internal static class Allocator
    {
        /// <summary>
        ///     Get spesific pirate for each group in the given config
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static List<List<int>> PhysicalSplit(params int[] config)
        {
            List<List<int>> groups = new List<List<int>>(config.Length);
            try
            {
                Bot.Game.Debug("=== ALLOCATOR ===");
                Bot.Game.Debug("Got config of " + string.Join(",", config));

                List<Zone> zones = Zone.Zones;
                List<Zone> zoneConfig;

                Bot.Game.Debug("Zones: " + string.Join(",", zones.ConvertAll(z => z.PiratesInZone.Count)));

                config = Allocator.AutoCorrectConfig(config);

                Bot.Game.Debug("Adjusted config to " + string.Join(",", config));

                zoneConfig = Allocator.CorrectByZones(config);

                Bot.Game.Debug("Final Config: " +
                               string.Join(",", zoneConfig.ConvertAll(z => z.Capacity - z.AvaliableSpots)));

                //sort zones by size
                zoneConfig.Sort((a, b) => a.Capacity.CompareTo(b.Capacity));

                //this comes already sorted by size.
                List<int>[] physicalZones = zones.ConvertAll(z => z.PiratesInZone).ToArray();

                for (int i = 0; i < zoneConfig.Count; i++)
                {
                    groups.AddRange(Zone.SplitZone(physicalZones[i], zoneConfig[i].Groups));
                }
            }
            catch (Exception ex)
            {
                Bot.Game.Debug("==========ALLOCATOR EXCEPTION============");
                Bot.Game.Debug("Allocation stopped because of exception: " + ex.Message);

                StackTrace exTrace = new StackTrace(ex, true);
                StackFrame frame = exTrace.GetFrame(0);
                Bot.Game.Debug("The exception was thrown from method {0} at file {1} at line #{2}", frame.GetMethod(),
                    frame.GetFileName(), frame.GetFileLineNumber());

                Bot.Game.Debug("==========ALLOCATOR EXCEPTION============");
            }

            return groups;
        }

        /// <summary>
        ///     Adjusts the comfig according to zones
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static List<Zone> CorrectByZones(int[] config)
        {
            Bot.Game.Debug("Adjusting config according to zones...");

            //sort the zones by size (bigger first)
            Zone.Zones.Sort((a, b) => a.PiratesInZone.Count.CompareTo(b.PiratesInZone.Count));

            //sort the config (bigger values first)
            Array.Sort(config, (a, b) => a.CompareTo(b));

            //i.e. 4, {2,2} = zone of 4 pirates divided to 2 groups of two
            List<Zone> zoneConfig = new List<Zone>(Zone.Zones.Count);

            //init list
            foreach (Zone z in Zone.Zones)
            {
                if (z.PiratesInZone.Count == 0)
                    continue;

                zoneConfig.Add(new Zone(z.PiratesInZone.Count));
            }

            //first of all, match groups which fill 100% of a zone
            foreach (Zone zone in zoneConfig)
            {
                if (zone.AvaliableSpots == 0)
                    continue;

                for (int k = 0; k < config.Length; k++)
                {
                    if (config[k] == 0)
                        continue;

                    if (config[k] == zone.AvaliableSpots)
                    {
                        zone.AddGroup(config[k]);
                        config[k] = 0;
                    }
                }
            }

            firstRound:
            //first matching round
            foreach (Zone zone in zoneConfig)
            {
                if (zone.AvaliableSpots == 0)
                    continue;

                for (int k = 0; k < config.Length; k++)
                {
                    int gConf = config[k];

                    if (gConf == 0)
                        continue;

                    if (zone.AvaliableSpots >= gConf)
                    {
                        zone.AddGroup(gConf);
                        config[k] = 0;
                    }
                }
            }

            //second match round. If required, it will fragmate the groups
            foreach (Zone zone in zoneConfig)
            {
                //skip full zones
                if (zone.AvaliableSpots == 0)
                    continue;

                for (int i = 0; i < config.Length; i++)
                {
                    //skip over matched groups
                    if (config[i] == 0)
                        continue;

                    config[i] -= zone.AvaliableSpots;
                    zone.AddGroup(zone.AvaliableSpots);
                }
            }

            if (zoneConfig.Any(z => z.AvaliableSpots != 0))
                goto firstRound; //kill me.

            Bot.Game.Debug("Zone adjusted configuration: " +
                           string.Join(",", zoneConfig.ConvertAll(z => z.PiratesInZone.Count)));

            return zoneConfig;
        }

        /// <summary>
        ///     Autocorrects a config
        /// </summary>
        /// <param name="autoCorrectLevel"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static int[] AutoCorrectConfig(int[] config,
            AutoCorrectOptions autoCorrectLevel = AutoCorrectOptions.All)
        {
            int sum = config.Sum();
            int pirateCount = Bot.Game.AllMyPirates().Count;

            //Check # of pirates and alert if something is wrong
            if (sum != pirateCount)
                if (autoCorrectLevel == AutoCorrectOptions.None)
                    throw new AllocationException(string.Format("ALLOCATED ERROR: Expected {0} pirates, but got {1}",
                        pirateCount, sum));
                else
                {
                    Bot.Game.Debug("ALLOCATED WARNING: Expected {0} pirates, but got {1}", pirateCount, sum);

                    //Autocorrecting
                    List<int> correctedConfig = new List<int>();

                    //Sort the array by length (critical for autocrrect). Bigger values first
                    Array.Sort(config, (a, b) => a.CompareTo(b));

                    if (sum > pirateCount)
                    {
                        if (autoCorrectLevel < AutoCorrectOptions.Higher)
                            throw new AllocationException(
                                string.Format("ALLOCATED ERROR: Expected {0} pirates, but got {1}",
                                    pirateCount, sum));

                        Bot.Game.Debug("Correcting config...");

                        int roof = 0;
                        int i = 0;
                        int delta = 0;

                        while (roof <= pirateCount && i < config.Length)
                        {
                            int currGroup = config[i];

                            if (roof + currGroup <= pirateCount)
                            {
                                correctedConfig.Add(currGroup);
                                roof += currGroup;
                            }
                            else
                            {
                                delta = Math.Abs(pirateCount - roof);
                                break;
                            }

                            i++;
                        }

                        //add the delta to the smallest group
                        correctedConfig[correctedConfig.Count - 1] += delta;
                        config = correctedConfig.ToArray();
                    }
                    else //this means that the config is using less pirates than possible.
                    {
                        if (autoCorrectLevel < AutoCorrectOptions.Lower)
                            throw new AllocationException(
                                string.Format("ALLOCATED ERROR: Expected {0} pirates, but got {1}", pirateCount, sum));

                        Bot.Game.Debug("Correcting config...");

                        //So just add couple of pirates to the smallest group
                        config[config.Length - 1] += pirateCount - sum;
                    }
                }

            return config;
        }
    }
}