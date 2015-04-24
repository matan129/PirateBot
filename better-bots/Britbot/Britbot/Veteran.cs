#region #Usings

using System;
using System.Collections.Generic;
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

            ultimateConfig.Sort((a, b) => a.CompareTo(b));
            currentConfig.Sort((a, b) => a.Pirates.Count.CompareTo(b.Pirates.Count));

            for (int i = 0; i < Math.Min(ultimateConfig.Count, currentConfig.Count); i++)
            {
                if (currentConfig[i].Pirates.Count > ultimateConfig[i])
                    currentConfig[i].Split(currentConfig[i].Pirates.Count - ultimateConfig[i]);
            }
        }
 
        /// <summary>
        ///     A method that reffers to the ULTIMATE CONFIGURATION and joins the group as needed
        /// </summary>
        private static void GroupJoining(List<int> ultimateConfig)
        {
            List<Group> currentConfig = Commander.Groups;
            List<Pirate> myPirates = Bot.Game.AllMyPirates();

            Group temp = null;
            int minD = Magic.MaxDistance + 1; //minimum distance

            currentConfig.Sort((a, b) => a.Pirates.Count.CompareTo(b.Pirates.Count));
            ultimateConfig.Sort((a, b) => a.CompareTo(b));

            for (int i = 0; i < Math.Min(ultimateConfig.Count, currentConfig.Count); i++)
            {
                if (currentConfig[i].Pirates.Count < ultimateConfig[i])
                {
                    foreach (Group g in currentConfig)
                    {
                        if ((g.Pirates.Count == 1) &&
                            (Bot.Game.Distance(myPirates[currentConfig[i].Pirates[0]], myPirates[g.Pirates[0]]) < minD) &&
                            (currentConfig[i].Pirates[0] != g.Pirates[0]))
                        {
                            temp = g;
                            minD = Bot.Game.Distance(myPirates[currentConfig[i].Pirates[0]], myPirates[g.Pirates[0]]);
                        }
                    }
                    if (minD <= Magic.MaxDistance)
                    {
                        if (temp != null)
                            currentConfig[i].Join(temp);
                    }
                }
            }
        }
    }
}