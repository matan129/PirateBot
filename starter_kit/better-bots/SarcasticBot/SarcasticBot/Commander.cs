using System;
using System.Collections.Generic;
using System.Linq;

namespace SarcasticBot
{
    public static class Commander
    {
        public static List<Group> Groups { get; private set; }
        private static List<int> Configuration;
        public static Queue<CommanderStrategy> Strategy;

        public static void Play()
        {
            throw new NotImplementedException();
        }

        public static void Initialize()
        {
            Strategy = new Queue<CommanderStrategy>();
            throw new NotImplementedException();
        }

        public static void Distribute()
        {
            throw new NotImplementedException();
        }

        public static void AssignTargets()
        {
            throw new NotImplementedException();
        }

        public static void Join(Group GroupA, Group GroupB)
        {
            throw new NotImplementedException();
        }

        public static double Casualties()
        {
            return Bot.Game.AllMyPirates().Count(p => p.IsLost)/Bot.Game.AllMyPirates().Count;

        }
    }
}