using System;
using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace SarcasticBot
{
    public static class Commander
    {
        public static List<int> Configuration { get; private set; }
        public static Queue<CommanderAction> Actions { get; private set; }
        public static List<Group> Groups { get; private set; }
        public static PriorityQueue<int, int> TargetQueue { get; private set; }

        /// <summary>
        /// Do something!
        /// </summary>
        public static void Play()
        {
            //Re-prioritize the islands in the game
            TargetQueue.Clear();
            TargetQueue = Ai.PrioritizeTargets();

            //if we are done with the current instructions determined by the Ai class
            if (Actions.Count == 0)
            {
                Actions = Ai.SuggestCommanderActions();
            }

            try
            {
                //Get the next instruction, if there is one, from the queue
                CommanderAction instruction = Actions.Dequeue();

                //If the instruction is special (like join all forces), do it before assigning targets and moving the groups.
                switch (instruction)
                {
                    case CommanderAction.JoinAll:
                        Configuration.Clear();
                        Configuration.Add(Bot.Game.AllMyPirates().Count);
                        Distribute();
                        Play();
                        break;
                    case CommanderAction.SplitAll:
                        Configuration.Clear();
                        Configuration.AddRange(Enumerable.Repeat(1, Bot.Game.AllMyPirates().Count));
                        Distribute();
                        Play();
                        break;
                    default:
                        //If the instruction is regular (like Aggressive Attack), assign targets with that instruction in mind
                        AssignTargets(instruction);
                        break;
                }
            }
            catch (InvalidOperationException)
            {
                AssignTargets(CommanderAction.Default);
            }

            //Actually move the stuff
            Execute();

            //Force garbage collection, because why not
            GC.Collect();
        }

        /// <summary>
        /// Init the commander
        /// </summary>
        public static void Initialize()
        {
            Actions = new Queue<CommanderAction>();
            Groups = new List<Group>();
            Configuration = new List<int>();

            Ai.SuggestCommanderActions();
        }

        /// <summary>
        /// Distribute our pirates to groups based on the config
        /// </summary>
        public static void Distribute()
        {
            //Re distribute only if the new config is different
            //The no lambda way is very long
            if (Groups.ConvertAll(g => g.Pirates.Count).Intersect(Configuration).Count() != Configuration.Count)
            {
                Groups.Clear();

                List<Pirate> myPirates = Bot.Game.AllMyPirates();

                //TODO make the distribution consider grouping near pirates
                //I think this solves it, testing needed
                //Sort pirates by distance from (0,0). This is now actually true chunk-ing, but I think it will do
                myPirates.Sort((a, b) => (Bot.Game.Distance(a, 0, 0)).CompareTo(Bot.Game.Distance(b, 0, 0)));

                int index = 0;
                foreach (int size in Configuration)
                {
                    //I know you hate lambda
                    //Sorry!
                    //Explanation: init a new group with "size" pirates in it, starting from an index
                    //then we convert the pirate list we took from the list of all pirates to a list of the pirates IDs
                    Groups.Add(new Group(myPirates.GetRange(index, size).ConvertAll(p => p.Id)));
                    index += size;
                }
            }
        }

        /// <summary>
        /// Moves all the groups!
        /// </summary>
        public static void Execute()
        {
            //I could make this lambda, you know. Groups.ForEach(g => g.Move());
            foreach (Group g in Groups)
            {
                g.Move();
            }
        }

        /// <summary>
        /// Assigns targets to groups based on their priorities and the bias determined by Ai
        /// </summary>
        /// <param name="bias">The bias to consider (i.e. AggressiveAttack)</param>
        public static void AssignTargets(CommanderAction bias)
        {
            //this should be async somehow
            foreach (Group g in Groups)
            {
                g.StartCalcThread();
            }

            //TODO smarter assignment - Maybe external priority queue?
            switch (bias)
            {
                case CommanderAction.AggressiveConquest:
                    foreach (Group g in Groups)
                    {
                        //The groups target will be the highest scoring island that is not ours
                        g.Target =
                            g.Priorities.First(
                                priority =>
                                    //Priority's key is an ITarget, so we choose one that is an Island
                                    priority.Key is SmartIsland &&
                                    //Here we check the owner of the island
                                    Bot.Game.GetIsland(((SmartIsland) priority.Key).Id).Owner != Consts.ME
                                    &&
                                    //Confirm no target duplication
                                    Groups.TrueForAll(g2 => g2.Target != (SmartIsland)priority.Key)).Key;
                    }
                    break;
                case CommanderAction.Defend:
                    foreach (Group g in Groups)
                    {
                        //Here we set the target to the highest scoring island that is ours in order to defend it
                        g.Target =
                            g.Priorities.First(
                                priority =>
                                    priority.Key is SmartIsland &&
                                    Bot.Game.GetIsland(((SmartIsland) priority.Key).Id).Owner == Consts.ME).Key;
                    }
                    break;
                case CommanderAction.Naive:
                    foreach (Group g in Groups)
                    {
                        g.Target =
                            g.Priorities.First(
                                priority =>
                                   //Confirm no target duplication
                                    Groups.TrueForAll(g2 => g2.Target != (SmartIsland)priority.Key)).Key;
                    }
                    break;
                case CommanderAction.Default:
                    DefaultAssignment();
                    break;
            }
        }

        private static void DefaultAssignment()
        {
            try
            {
                SmartIsland island = new SmartIsland(TargetQueue.Dequeue().Key);

                int maxScore = 0;
                Group optimalAttacker = null;
                foreach (Group g in Groups)
                {
                    if (maxScore < g.Priorities[island].Score)
                    {
                        maxScore = g.Priorities[island].Score;
                        optimalAttacker = g;
                    }
                }

                if (optimalAttacker != null)
                {
                    optimalAttacker.Target = island;
                }
                else //No one can attack this - every score for this target is negative
                {
                    //try the next target
                    DefaultAssignment();
                }
            }
            catch (InvalidOperationException)
            {
                AssignTargets(CommanderAction.Naive);
            }
        }

        /// <summary>
        /// Fraction of how many of our ships are lost
        /// </summary>
        /// <returns>a double representing how many of our ships are lost (0.0-1.0)</returns>
        public static double Casualties()
        {
            return Bot.Game.AllMyPirates().Count(p => p.IsLost)/Bot.Game.AllMyPirates().Count;
        }
    }
}