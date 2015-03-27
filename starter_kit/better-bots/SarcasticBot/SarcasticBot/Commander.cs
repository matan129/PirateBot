using System;
using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace SarcasticBot
{
    public static class Commander
    {
        public static List<Group> Groups { get; private set; }
        private static List<int> Configuration;
        public static Queue<CommanderAction> Actions;

        /// <summary>
        /// Do something!
        /// </summary>
        public static void Play()
        {
            //if we are done with the current instructions determined by the Ai class
            if (Actions.Count == 0)
            {
                Ai.SetCommanderActions();
            }

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
            
            Ai.SetCommanderActions();
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
                //Sort pirates by distance from (0,0). This is now actually true chunking, but I think it will do
                myPirates.Sort((a, b) => (Bot.Game.Distance(a,0,0)).CompareTo(Bot.Game.Distance(b,0,0)));

                int index = 0;
                foreach (int size in Configuration)
                {
                    //I know you have lambda
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
            

            throw new NotImplementedException();
        }

        /// <summary>
        /// Join two groups together and add the to the list of groups
        /// </summary>
        /// <param name="groupA">The first group to join</param>
        /// <param name="groupB">Thr second group to join</param>
        public static void Join(Group groupA, Group groupB)
        {
            //Add all the pirate in group B to group A, and ignore duplicates (although there shouldn't be)
            groupA.Pirates.AddRange(groupB.Pirates.Where(p => groupA.Pirates.Contains(p) != true));

            //The remove nmethod uses the Equal object method so I implemented it in the Group class
            Groups.Remove(groupB);
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