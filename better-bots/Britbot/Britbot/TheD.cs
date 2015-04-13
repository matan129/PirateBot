using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Britbot
{
    class TheD
    {
        public static Dictionary<string, List<long>> times = new Dictionary<string,List<long>>();
        public static Dictionary<string, long> begins = new Dictionary<string, long>();

        

        public static void BeginTime(string key)
        {
            if (begins.ContainsKey(key))
            {
                begins[key] = Commander.TurnTimer.ElapsedMilliseconds;
            }
            else
            {
                begins.Add(key, Commander.TurnTimer.ElapsedMilliseconds);
            }
        }

        public static void StopTime(string key)
        {
            if (times.ContainsKey(key))
            {
                times[key].Add(Commander.TurnTimer.ElapsedMilliseconds - begins[key]) ;
            }
            else
            {
                times.Add(key, new List<long>());
                times[key].Add(Commander.TurnTimer.ElapsedMilliseconds - begins[key]);
            }
        }

        public static void Debug()
        {
            double avg = 0;
            Bot.Game.Debug("------------------------PROFILING-----------------------");
            foreach (KeyValuePair<string, List<long>> kv in times)
            {
                avg += kv.Value.Average();
                Bot.Game.Debug(kv.Key + " Avg: " + kv.Value.Average() + "\t Max: " + kv.Value.Max());
            }
            Bot.Game.Debug("Total avg: " + avg);
        }
    }
}
