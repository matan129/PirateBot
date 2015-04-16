#region #Usings

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Britbot
{
    internal class TheD
    {
        #region Static Fields & Consts

        public static Dictionary<string, List<long>> times = new Dictionary<string, List<long>>();
        public static Dictionary<string, long> begins = new Dictionary<string, long>();
        public static Dictionary<string, int> count = new Dictionary<string, int>();
        #endregion

        public static void BeginTime(string key)
        {
            Bot.Game.Debug("8======================> Debug Begining " + key);
            if (TheD.begins.ContainsKey(key))
            {
                TheD.begins[key] = Commander.TurnTimer.ElapsedMilliseconds;
            }
            else
            {
                TheD.begins.Add(key, Commander.TurnTimer.ElapsedMilliseconds);
            }
        }

        public static void StopTime(string key)
        {
            Bot.Game.Debug("8======================> Debug stoping " + key);
            if (TheD.times.ContainsKey(key))
            {
                TheD.times[key].Add(Commander.TurnTimer.ElapsedMilliseconds - TheD.begins[key]);
            }
            else
            {
                TheD.times.Add(key, new List<long>());
                TheD.times[key].Add(Commander.TurnTimer.ElapsedMilliseconds - TheD.begins[key]);
            }
        }

        public static void Count(string key)
        {
            if (TheD.count.ContainsKey(key))
            {
                TheD.count[key]++;
            }
            else
            {
                TheD.count.Add(key, 1);
            }
        }
        public static void Debug()
        {
            double avg = 0;
            Bot.Game.Debug("------------------------PROFILING-----------------------");
            foreach (KeyValuePair<string, List<long>> kv in TheD.times)
            {
                avg += kv.Value.Average();
                Bot.Game.Debug(kv.Key + " Avg: " + kv.Value.Average() + "\t Max: " + kv.Value.Max());
            }
            Bot.Game.Debug("Total avg: " + avg);
        }
    }
}