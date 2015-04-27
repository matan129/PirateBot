#region #Usings

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Britbot
{
    /// <summary>
    ///     An experienced ally to the commander who does bugger all
    /// </summary>
    internal static class ConfigHelper
    {
        #region Static Fields & Consts

        private static List<int> _ultimateConfig;

        #endregion

        /// <summary>
        ///     Splits and joins the current groups to match the ultimate configuration
        /// </summary>
        public static void ReConfigure()
        {
            if ((Magic.DoConfiguration))
                _ultimateConfig = GetUltimateGameConfig();

            Logger.Write("Ultimate Config:");
            Logger.Write(string.Join(",", _ultimateConfig), true);

            GroupSplitting(_ultimateConfig);
            GroupJoining(_ultimateConfig);

            Logger.Write("New config Config:");
            Logger.Write(string.Join(",", Commander.Groups.ConvertAll(group => @group.Pirates.Count).ToArray()), true);
        }

        /// <summary>
        ///     A method that refers to the ULTIMATE CONFIGURATION and splits the gruops as needed
        /// </summary>
        private static void GroupSplitting(List<int> ultimateConfig)
        {
            List<Group> newGroups = new List<Group>();

            ultimateConfig.Sort((b, a) => a.CompareTo(b));
            Commander.Groups.Sort((b, a) => a.Pirates.Count.CompareTo(b.Pirates.Count));

            for (int i = 0; i < Math.Min(ultimateConfig.Count, Commander.Groups.Count); i++)
            {
                if (Commander.Groups[i].Pirates.Count > ultimateConfig[i])
                    newGroups.AddRange(Commander.Groups[i].Split(Commander.Groups[i].Pirates.Count - ultimateConfig[i]));
            }


            Commander.Groups.AddRange(newGroups);
            Commander.Groups.RemoveAll(g => g.Pirates.Count == 0);
        }

        /// <summary>
        ///     A method that reffers to the ULTIMATE CONFIGURATION and joins the group as needed
        /// </summary>
        private static void GroupJoining(List<int> ultimateConfig)
        {
            Group tempGroup = null;
            int minDistance = Magic.MaxJoinDistance + 1; //maximun joining distance

            //sort configs by size
            Commander.Groups.Sort((a, b) => a.Pirates.Count.CompareTo(b.Pirates.Count));
            ultimateConfig.Sort((a, b) => a.CompareTo(b));

            List<int> joinedGroups = new List<int>();

            //go over the config and correct it
            for (int i = 0; i < Math.Min(ultimateConfig.Count, Commander.Groups.Count); i++)
            {
                if (Commander.Groups[i].Pirates.Count < ultimateConfig[i])
                {
                    //find the best group to join to the current group
                    foreach (Group g in Commander.Groups.Where(g => g.Pirates.Count == 1 && joinedGroups.Contains(g.Id))
                        )
                    {
                        //minimal distance between the two groups
                        int tempDistance = Commander.Groups[i].MinDistance(g);

                        //if the current minimun is better than the last minimun and the group do not cintain the same pirates..
                        if ((tempDistance < minDistance) && (!Commander.Groups[i].Pirates.Intersect(g.Pirates).Any()))
                        {
                            tempGroup = g;
                            minDistance = tempDistance;
                        }
                    }
                    //join the groups if they are close enough
                    if (minDistance <= Magic.MaxJoinDistance)
                    {
                        if (tempGroup != null)
                        {
                            joinedGroups.Add(tempGroup.Id);
                            Commander.Groups[i].Join(tempGroup, false);
                        }
                    }
                }
            }

            //remove all joined group from the commander list
            Commander.Groups.RemoveAll(g => joinedGroups.Contains(g.Id));

            Logger.Write("Commander Groups After Reconfiguration:");
            foreach (Group g in Commander.Groups)
            {
                Logger.Write(g.Pirates.Count.ToString());
            }
        }

        /// <summary>
        ///     Returns a list of integers that describes the current wanted configuration
        /// </summary>
        /// <returns>The optimal group configuration</returns>
        private static List<int> GetUltimateGameConfig()
        {
            //Enemy configuration
            int[] eConfig = Enemy.Groups.ConvertAll(group => @group.EnemyPirates.Count).ToArray();

            //sort the enemy configuration by size
            Array.Sort(eConfig, (a, b) => b.CompareTo(a));

            //prepare return value
            List<int> ret = new List<int>();

            //count of all of our pirates
            int myPirates = Bot.Game.AllMyPirates().Count;

            for (int i = 0; i < eConfig.Length && myPirates > 0; i++)
            {
                if (((eConfig[i] + 1) < myPirates) && ((eConfig[i] + 1) < Magic.MaxGroupSize))
                {
                    ret.Add(eConfig[i] + 1);
                    myPirates -= eConfig[i] + 1;
                }
            }

            while (myPirates > 0)
            {
                ret.Add(1);
                myPirates--;
            }

            while (ret.Count > Bot.Game.Islands().Count || ret.Count > Magic.MaxGroups)
            {
                ret[ret.Count - 2] += ret.Last();
                ret.RemoveAt(ret.Count - 1);
            }

            return ret;
        }
    }
}