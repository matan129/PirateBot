using System;
using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace SarcasticBot
{
    /// <summary>
    /// Represents a group of our pirates that have the same target and path
    /// </summary>
    public class Group : ITarget
    {
        private List<FriendPirate> Pirates;
        private ITarget Target;
        private Dictionary<ITarget, ScoreStruct> Priorities;
        private Path Trajectory;

        public delegate void ManueverAction();

        /// <summary>
        /// Initializes a new group
        /// </summary>
        /// <param name="size">Group size</param>
        /// <param name="index">The index in the pirate list to start at</param>
        public Group(int size, int index)
        {
            this.Pirates = new List<FriendPirate>();
            this.Priorities = new Dictionary<ITarget, ScoreStruct>();
            this.Trajectory = new Path();

            for (; index < size + index; index++)
            {
                this.Pirates.Add(new FriendPirate(index));
            }

            this.CalculatePriorities();
        }

        /// <summary>
        /// Initializes a new group
        /// </summary>
        /// <param name="pirates">The pirates to include in the group</param>
        public Group(IEnumerable<FriendPirate> pirates)
        {
            this.Pirates = new List<FriendPirate>(pirates);
            this.Priorities = new Dictionary<ITarget, ScoreStruct>();
            this.Trajectory = new Path();
            this.CalculatePriorities();
        }

        /// <summary>
        /// Move all our pirates along their path
        /// </summary>
        private void Move()
        {
            Location nextLoc = this.Trajectory.GetNextLocation();
            this.Pirates.Where(pete => pete.IsAlive()).ToList().ForEach(pete =>
            {
                pete.SetSail(Bot.Game.GetDirections(pete, nextLoc));
                nextLoc = Bot.Game.GetMyPirate(pete.Index).Loc;
            });
        }

        /// <summary>
        /// Calculates the priorities for the group, the best ones being on top of the list
        /// </summary>
        /// <returns>Dictionary including a target and its score parameters</returns>
        public Dictionary<ITarget, ScoreStruct> CalculatePriorities()
        {
            var prioritiesDictionary = new Dictionary<ITarget, ScoreStruct>();
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
                var leavingPirates = new List<FriendPirate>();

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
        public Group Split(params FriendPirate[] sPirates)
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
    }
}