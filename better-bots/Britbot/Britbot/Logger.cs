﻿#define DEBUG
#define PROFILING
#define DUMPFILE

//comment the folowwing lines if you want to enable debug and profiling.
#undef DEBUG
#undef PROFILING
#undef DUMPFILE

#region #Usings

using System.Collections.Generic;

#endregion

namespace Britbot
{
    internal static class Logger
    {
        #region Static Fields & Consts

        private static Dictionary<string, Queue<long>> times = new Dictionary<string, Queue<long>>();
        private static Dictionary<string, long> begins = new Dictionary<string, long>();
        private static Dictionary<string, int> count = new Dictionary<string, int>();

#if DUMPFILE
        private static FileStream _logFileStream =
            new FileStream(string.Format("C:\\log_{0}_{1}.txt", DateTime.Now, Commander.Version), FileMode.Create, FileAccess.ReadWrite);
#endif

        #endregion

        public static void BeginTime(string key)
        {
#if DEBUG
            Bot.Game.Debug("==> Debug Begining " + key + " time: " + Bot.Game.TimeRemaining());
#endif

#if DUMPFILE

            Bot.Game.Debug("=================================" +Logger._logFileStream);

            using (StreamWriter logWriter = new StreamWriter(Logger._logFileStream))
            {
                logWriter.WriteLine("==> Debug Begining " + key + " time: " + Bot.Game.TimeRemaining());
            }
#endif


            if (Logger.begins.ContainsKey(key))
            {
                Logger.begins[key] = Commander.TurnTimer.ElapsedMilliseconds;
            }
            else
            {
                Logger.begins.Add(key, Commander.TurnTimer.ElapsedMilliseconds);
            }
        }

        public static void StopTime(string key)
        {
#if DEBUG
            Bot.Game.Debug("==> Debug Stopping " + key + " time: " + Bot.Game.TimeRemaining());
#endif

#if DUMPFILE
            using (StreamWriter logWriter = new StreamWriter(Logger._logFileStream))
            {
                logWriter.WriteLine("==> Debug Stopping " + key + " time: " + Bot.Game.TimeRemaining());
            }
#endif

            if (Logger.times.ContainsKey(key))
            {
                Logger.times[key].Enqueue(Commander.TurnTimer.ElapsedMilliseconds - Logger.begins[key]);
            }
            else
            {
                Logger.times.Add(key, new Queue<long>());
                Logger.times[key].Enqueue(Commander.TurnTimer.ElapsedMilliseconds - Logger.begins[key]);

                //keep the logger small
                if (Logger.times[key].Count > 100)
                    Logger.times[key].Dequeue();
            }
        }

        public static void Count(string key)
        {
            if (Logger.count.ContainsKey(key))
            {
                Logger.count[key]++;
            }
            else
            {
                Logger.count.Add(key, 1);
            }
        }

        public static void Debug()
        {
#if PROFILING
            Bot.Game.Debug("------------------------PROFILING-----------------------");
            foreach (KeyValuePair<string, Queue<long>> kv in Logger.times)
            {
                double avg = 0;
                long[] arr = kv.Value.ToArray();
                for (int i = 0; i < arr.Length; i++)
                    avg += arr[i];

                Bot.Game.Debug(kv.Key + " Avg: " + avg/arr.Length );
            }
#endif

#if DUMPFILE
            using (StreamWriter logWriter = new StreamWriter(Logger._logFileStream))
            {
                double avg = 0;

                logWriter.WriteLine("------------------------PROFILING-----------------------");

                foreach (KeyValuePair<string, List<long>> kv in Logger.times)
                {
                    avg += kv.Value.Average();
                    logWriter.WriteLine(kv.Key + " Avg: " + kv.Value.Average() + "\t Max: " + kv.Value.Max());
                }

                logWriter.WriteLine("Total avg: " + avg);
            }
#endif
        }

        public static void DumpDebug()
        {
            Bot.Game.Debug(Logger.times.ToString());
        }
    }
}