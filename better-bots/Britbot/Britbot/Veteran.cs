#region #Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     An experienced ally to the commander who does bugger all
    /// </summary>
    internal class Veteran
    {
        /// <summary>
        ///     Splits and joins the current groups to match the ultimate configuration
        /// </summary>
        public static void ReConfigure()
        {
            List<int> ultimateGameConfig = Commander.GetUltimateGameConfig();
            Veteran.GroupSplitting(ultimateGameConfig);
            Veteran.GroupJoining(ultimateGameConfig);
        }
        
        /// <summary>
        ///     A method that refers to the ULTIMATE CONFIGURATION and splits the gruops as needed
        /// </summary>
        private static void GroupSplitting(List<int> ultimateConfig)
        {
            List<Group> currentConfig = Commander.Groups;
            List<Group> newGroups = new List<Group>();

            ultimateConfig.Sort((a, b) => a.CompareTo(b));
            currentConfig.Sort((a, b) => a.Pirates.Count.CompareTo(b.Pirates.Count));

            for (int i = 0; i < Math.Min(ultimateConfig.Count, currentConfig.Count); i++)
            {
                if (currentConfig[i].Pirates.Count > ultimateConfig[i])
                    newGroups.AddRange(currentConfig[i].Split(currentConfig[i].Pirates.Count - ultimateConfig[i]));
            }

            Commander.Groups.AddRange(newGroups);
        }
 
        /// <summary>
        ///     A method that reffers to the ULTIMATE CONFIGURATION and joins the group as needed
        /// </summary>
        private static void GroupJoining(List<int> ultimateConfig)
        {
            Group tempGroup = null;
            List<Group> currentConfig = Commander.Groups;
            int minDistance = Magic.MaxDistance + 1; //maximun joining distance

            //sort configs by size
            currentConfig.Sort((a, b) => a.Pirates.Count.CompareTo(b.Pirates.Count));
            ultimateConfig.Sort((a, b) => a.CompareTo(b));

            List<int> joinedGroups = new List<int>();

            //go over the config and correct it
            for (int i = 0; i < Math.Min(ultimateConfig.Count, currentConfig.Count); i++)
            {
                if (currentConfig[i].Pirates.Count < ultimateConfig[i])
                {
                    //find the best group to join to the current group
                    foreach (Group g in currentConfig.Where(g => g.Pirates.Count == 1 && joinedGroups.Contains(g.Id)))
                    {
                        //minimal distance between the two groups
                        int tempDistance = currentConfig[i].MinDistance(g);

                        //if the current minimun is better than the last minimun and the group do not cintain the same pirates..
                        if((tempDistance < minDistance) && (!currentConfig[i].Pirates.Intersect(g.Pirates).Any()))
                        {
                            tempGroup = g;
                            minDistance = tempDistance;
                        }
                    }
                    //join the groups if they are close enough
                    if (minDistance <= Magic.MaxDistance)
                    {
                        if (tempGroup != null)
                        {
                            currentConfig[i].Join(tempGroup);
                            joinedGroups.Add(tempGroup.Id);
                        }
                    }
                }
            }
        }
    }
}