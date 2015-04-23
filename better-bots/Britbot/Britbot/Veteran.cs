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
        ///     A method that reffers to the ULTIMATE CONFIGURATION and splits the gruops as needed
        /// </summary>
        public static void GroupSplitting()
        {
            List<int> Ucon = Commander.GetUltimateGameConfig();
            List<Group> Ccon = Commander.Groups;


            Ccon.Sort((a, b) => a.Pirates.Count.CompareTo(b.Pirates.Count));
            Ucon.Sort((a, b) => a.CompareTo(b));

            for (int i = 0; i < Math.Min(Ucon.Count, Ccon.Count); i++)
            {
                if (Ccon[i].Pirates.Count > Ucon[i])
                    Ccon[i].Split(Ccon[i].Pirates.Count - Ucon[i]);
            }
        }

        /// <summary>
        ///     A method that reffers to the ULTIMATE CONFIGURATION and joins the gruops as needed
        /// </summary>
        public static void GroupJoining()
        {
            List<int> Ucon = Commander.GetUltimateGameConfig();
            List<Group> Ccon = Commander.Groups;
            List<Pirate> myPirates = Bot.Game.AllMyPirates();

            Group temp = null;
            int minD = Magic.MaxDistance + 1; //minimum distance

            Ccon.Sort((a, b) => a.Pirates.Count.CompareTo(b.Pirates.Count));
            Ucon.Sort((a, b) => a.CompareTo(b));

            for (int i = 0; i < Math.Min(Ucon.Count, Ccon.Count); i++)
            {
                if (Ccon[i].Pirates.Count < Ucon[i])
                {
                    foreach (Group g in Ccon)
                    {
                        if ((g.Pirates.Count == 1) &&
                            (Bot.Game.Distance(myPirates[Ccon[i].Pirates[0]], myPirates[g.Pirates[0]]) < minD) &&
                            (Ccon[i].Pirates[0] != g.Pirates[0]))
                        {
                            temp = g;
                            minD = Bot.Game.Distance(myPirates[Ccon[i].Pirates[0]], myPirates[g.Pirates[0]]);
                        }
                    }
                    if (minD <= Magic.MaxDistance)
                    {
                        if (temp != null)
                            Ccon[i].Join(temp);
                    }
                }
            }
        }
    }
}