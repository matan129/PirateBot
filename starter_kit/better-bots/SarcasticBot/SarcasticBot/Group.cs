using System;
using System.Collections.Generic;
using System.Linq;
using Pirates;
using System.Threading;

namespace SarcasticBot
{
    /// <summary>
    /// Represents a group of our pirates that have the same target and path
    /// </summary>
    public class Group : ITarget
    {
        internal List<int> Pirates { get; private set; }
        internal ITarget Target;
        internal Dictionary<ITarget, ScoreStruct> Priorities { get; private set; }
        private Path Trajectory;

        public Thread CalcThread;

        /// <summary>
        /// Initializes a new group
        /// </summary>
        /// <param name="size">Group size</param>
        /// <param name="index">The index in the pirate list to start at</param>
        public Group(int size, int index)
        {
            this.Pirates = new List<int>();
            this.Priorities = new Dictionary<ITarget, ScoreStruct>();
            this.Trajectory = new Path();

            for (; index < size + index; index++)
            {
                this.Pirates.Add(index);
            }
            
            this.StartCalcThread();
        }


        /// <summary>
        /// Initializes a new group
        /// </summary>
        /// <param name="pirates">The pirates to include in the group</param>
        public Group(IEnumerable<int> pirates)
        {
            this.Pirates = new List<int>(pirates);
            this.Priorities = new Dictionary<ITarget, ScoreStruct>();
            this.Trajectory = new Path();
            this.StartCalcThread();
        }

        /// <summary>
        /// Move all our pirates along their path
        /// </summary>
        public void Move()
        {
            Location nextLoc = this.Trajectory.GetNextLocation();

            foreach(int pete in this.Pirates.Where(pete => !Bot.Game.GetMyPirate(pete).IsLost))
            {
                Direction d = Bot.Game.GetDirections(pete, nextLoc).First();
                Bot.Game.SetSail(pete, d);
                nextLoc = Utility.AddLoc(nextLoc, d);
            }
        }

        /// <summary>
        /// Starts the priority calculation on a new thread
        /// </summary>
        public void StartCalcThread()
        {
            this.CalcThread = new Thread(delegate()
            {
                lock (this.Priorities)
                {
                    this.Priorities = this.CalculatePriorities();
                }
            });
            this.CalcThread.Start();
        }

        /// <summary>
        /// Calculates the priorities for the group, the best ones being on top of the list
        /// </summary>
        /// <returns>Dictionary including a target and its score parameters</returns>
        public Dictionary<ITarget, ScoreStruct> CalculatePriorities()
        {
            Dictionary<ITarget, ScoreStruct> prioritiesDictionary = new Dictionary<ITarget, ScoreStruct>();
            Utility.GetAllTargets()
                .ForEach(target => prioritiesDictionary.Add(target, target.GetScore(this, this.Trajectory, false)));
           
            return prioritiesDictionary.OrderByDescending(x => x.Value.Score).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Returns the group's score relative to another group
        /// </summary>
        /// <param name="origin">The group asking for the score</param>
        /// <param name="path">Path to the target</param>
        /// <param name="isFast">Use fast approximation or not</param>
        /// <returns>A ScoreStruct for the target</returns>
        public ScoreStruct GetScore(Group origin, Path path = null, bool isFast = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Splits the current group to two groups and returns the extra one
        /// </summary>
        /// <param name="size">How many pirates should the other group contain</param>
        /// <returns>A new group with the specified size</returns>
        public Group Split(int size)
        {
            if (size < this.Pirates.Count)
            {
                var leavingPirates = new List<int>();

                size--;
                for (; size >= 0; size--)
                {
                    leavingPirates.Add(this.Pirates[size]);
                    this.Pirates.RemoveAt(size);
                }

                return new Group(leavingPirates);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Splits the current group to two groups and returns the extra one
        /// </summary>
        /// <param name="sPirates">The pirates to include in the new group</param>
        /// <returns>A new group with the specified size</returns>
        public Group Split(params int[] sPirates)
        {
            if (sPirates.Length > 0 && sPirates.Length < this.Pirates.Count)
            {
                sPirates.ToList().ForEach(sP => this.Pirates.Remove(sP));
                return new Group(sPirates);
            }
            else if(sPirates.Length == 0)
            {
                throw new ArgumentException("No pirates specified");
            }
            else
            {
                throw new ArgumentException("Too many pirates!");
            }
        }

        /// <summary>
        /// Checks if two groups are the same by checking if thier pirates are the same
        /// </summary>
        /// <param name="obj">The object to comare to</param>
        /// <returns>Return true if the groups are identical</returns>
        public override bool Equals(object obj)
        {
            if (obj is Group)
            {
                Group g = (Group) obj;
                if (g.Pirates == this.Pirates)
                {
                    return true;
                }
            }
            return false;
            
        }
    }
}