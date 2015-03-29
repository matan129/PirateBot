﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Pirates;

namespace Britbot
{
    /// <summary>
    /// Represents a structure of few of our pirates that have a common goal
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Creates a new group with set amount of ships (without thinking to much)
        /// </summary>
        /// <param name="amount">How many pirates will be in the group</param>
        public Group(int amount)
        {
            //TODO try to auto choose group members by distance?
            //get id and update counter
            this.Id = GroupCounter++;

            //counting variable for the added pirates
            int count = 0;

            for (int i = 0; i < Bot.Game.MyPirates().Count && count < amount; i++)
            {
                if (!Commander.IsEmployed(i))
                {
                    //add pirate and update count
                    Pirates.Add(i);
                    count++;
                }
            }
        }

        /// <summary>
        /// Sets the target of the group, while doing so also resets the heading vector
        /// if there is need (meaning if we didn't choose the same target again).
        /// </summary>
        /// <param name="target">the new target</param>
        public void SetTarget(ITarget target)
        {
            //if it isn't the same target as before update and reset heading
            if (Target != target)
            {
                Target = target;
                Heading.SetCoordinates(0, 0);
            }
        }

        /// <summary>
        /// Returns the average location for this group
        /// </summary>
        /// <returns>Returns the average location for this group</returns>
        public Location GetLocation()
        {
            //assigning X and Y to hold the sum
            int x = 0;
            int y = 0;

            foreach (int pirate in Pirates)
            {
                x += Bot.Game.GetEnemyPirate(pirate).Loc.Col;
                y += Bot.Game.GetEnemyPirate(pirate).Loc.Row;
            }

            return new Location(y/Pirates.Count, x/Pirates.Count);
        }

        /// <summary>
        /// Move the pirates!
        /// </summary>
        public void Move()
        {
            //get Direction of movement
            Direction newDir = Target.GetDirection(this);

            //update heading
            Heading += newDir;

            //first move the first pirate 
            Pirate leader = Bot.Game.GetMyPirate(this.Pirates.First());
            Bot.Game.SetSail(leader, newDir);

            //if there are others, move them after him
            if (Pirates.Count > 1)
            {
                /* this foreach lambda explained:
                 * we take the list of the group's pirates which is actually the list of the pirates IDs
                 * We skip the first pirate because he is the leader and we have already moved it at line 130
                 * We convert back the collection (which the Skip() method returned) of IDs with the first pirate ID taken out to a normal list<int>
                 * We convert this list of ints to pirates via the ConvertAll() method, with p being a pirate's ID 
                 * Then we iterate over each pirate in this list.
                 * voilà!
                 * 
                 * P.S unlike the previous version of this method, although my lines are not the shortest,
                 * it is readable. Seriously, try reading it.
                 */
                foreach (Pirate pete in this.Pirates.Skip(1).ToList().ConvertAll(p => Bot.Game.GetMyPirate(p)))
                {
                    Direction order = Bot.Game.GetDirections(pete, leader).First();
                    Bot.Game.SetSail(pete, order);
                }
            }
        }

        /// <summary>
        /// Calculate target priorities for this group
        /// </summary>
        public void CalcPriorities()
        {
            throw new NotImplementedException();
        }

        #region Members

        /// <summary>
        /// Direction of the group to make navigation more precise
        /// </summary>
        public HeadingVector Heading;

        /// <summary>
        /// The Group ID number
        /// useful for debugging
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// List of the indexes of the pirates in this group
        /// </summary>
        public List<int> Pirates { get; private set; }

        /// <summary>
        /// The target of the Group
        /// </summary>
        private ITarget Target;

        /// <summary>
        /// The group's role (i.e. destroyer or attacker)
        /// </summary>
        public GroupRole Role { get; private set; }

        /// <summary>
        /// List of priorities for this group
        /// </summary>
        public List<Score> Priorities { get; private set; }

        /// <summary>
        /// A thread for complex calculations that can be ran in parallel to other stuff
        /// </summary>
        public Thread CalcThread { get; private set; }

        /// <summary>
        /// static member to give each group a unique id based on its number of creation
        /// </summary>
        private static int GroupCounter;

        #endregion
    }
}