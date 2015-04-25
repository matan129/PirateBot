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
    internal static class Veteran
    {
        /// <summary>
        ///     Splits and joins the current groups to match the ultimate configuration
        /// </summary>
        public static void ReConfigure()
        {
            List<int> ultimatConfig = Commander.GetUltimateGameConfig();
         
            Logger.Write("Ultimate Config:", true);
            Logger.Write(string.Join(",", ultimatConfig), true);

            Veteran.GroupSplitting(ultimatConfig);
            Veteran.GroupJoining(ultimatConfig);
            Logger.Write("New config Config:", true);
            Logger.Write(string.Join(",", Commander.Groups.ConvertAll(group => group.Pirates.Count).ToArray()), true);
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
            int minDistance = Magic.MaxDistance + 1; //maximun joining distance

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
                    foreach (Group g in Commander.Groups.Where(g => g.Pirates.Count == 1 && joinedGroups.Contains(g.Id)))
                    {
                        //minimal distance between the two groups
                        int tempDistance = Commander.Groups[i].MinDistance(g);

                        //if the current minimun is better than the last minimun and the group do not cintain the same pirates..
                        if((tempDistance < minDistance) && (!Commander.Groups[i].Pirates.Intersect(g.Pirates).Any()))
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
                            joinedGroups.Add(tempGroup.Id);
                            Commander.Groups[i].Join(tempGroup,false);
                        }
                    }
                }
            }

            //remove all joined group from the commander list
            Commander.Groups.RemoveAll(g => joinedGroups.Contains(g.Id));

            Logger.Write("Commander Groups After Reconfiguration:",true);
            foreach (Group g in Commander.Groups)
            {
                Logger.Write(g.Pirates.Count.ToString(),true);
            }
        }

        /// <summary>
        ///     Reveals cloaked pirates when needed and cloaks uncloaked ones when needed
        /// </summary>
        public static void Cloaking()
        {
            
            Group g = Veteran.GetCloackedGroup();//the group that contains the cloaked pirate if one exists

            // if a pirate is cloaked and close enough to it's target, reveal it
            if ((g != null) && (g.DistanceFromTarget <= Magic.CloakRange))
            {
                Bot.Game.Reveal(Bot.Game.GetMyCloaked());
            }
            // if no pirate is cloaked and you can cloak one
            else if ((g==null)&&(Bot.Game.CanCloak()))
            {
                IEnumerable<Group> ones = Commander.Groups.Where(p => p.Pirates.Count == 1); //All the 1 pirate groups that can be cloaked
                //if there are any 1 pirate groups
                if (ones.Count()!=0)
                {
                    int minDistance = ones.Min(group => group.DistanceFromTarget);//the minimum distance from a target of one of the groups

                    //finds the group that the minimal distance belongs to and cloaks it
                    foreach (Group tc in ones)
                    {
                        if (tc.DistanceFromTarget == minDistance)
                        {
                            Bot.Game.Cloak(Bot.Game.AllMyPirates()[tc.Pirates[0]]);
                        }
                    }
                    
                }
            }

        }

        /// <summary>
        ///     Finds the group that contains the cloaked pirate
        /// </summary>
        /// <returns>The wanted group or null if no pirate is cloaked </returns>
        public static Group GetCloackedGroup()
        {
            int pirate = -1;
            for (int i = 0; i < Bot.Game.AllMyPirates().Count; i++)
            {
                if (Bot.Game.AllMyPirates()[i] == Bot.Game.GetMyCloaked())
                {
                    pirate = i;
                    break;
                }                
            }

            foreach(Group g in Commander.Groups)
            {
                if (g.Pirates.Contains(pirate))
                    return g;
            }

            return null;

        }
    }
}