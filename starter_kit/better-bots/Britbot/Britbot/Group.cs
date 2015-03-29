﻿using System;
using System.Collections.Generic;
using System.Threading;
using Pirates;

namespace Britbot
{
    /// <summary>
    /// Represents a structure of few of our pirates that have a common goal
    /// </summary>
    public class Group
    {
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
        static int GroupCounter = 0;
        #endregion

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
                Heading.SetCoordinates(0,0);
            }
        }

        /// <summary>
        /// Returns the average location for this group
        /// </summary>
        /// <returns>Returns the average location for this group</returns>
        public Location GetLocation()
        {
            //assigning X and Y to hold the sum
            int X = 0;
            int Y = 0;

            foreach (int pirate in Pirates)
            {
                X += Bot.Game.GetEnemyPirate(pirate).Loc.Col;
                Y += Bot.Game.GetEnemyPirate(pirate).Loc.Row;
            }

            return new Location(Y / Pirates.Count, X / Pirates.Count);
        }

        /// <summary>
        /// Creates a new group with set amount of ships (without thinking to much)
        /// </summary>
        /// <param name="id">how many pirates will be in the group</param>
        public Group(int amount)
        {
            //get id and update counter
            this.Id = GroupCounter++;

            //counting variable for the added pirates
            int Count = 0;

            for(int i = 0;i < Bot.Game.MyPirates().Count && Count < amount;i++)
            {
                if(!Commander.IsEmployed(i))
                {
                    //add pirate and update count
                    Pirates.Add(i);
                    Count++;
                }
            }

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

            //first move the first
            Bot.Game.SetSail(Bot.Game.MyPirates()[Pirates[0]], newDir);

            //if there are others, move them after him
            if (Pirates.Count > 1)
            {
                for (int i = 1; i < Pirates.Count; i++)
                {
                    //sail every other ship after the first
                    //----REMARK: THIS LINE IS TERRIBLE, WE NEED TO THINK OF A WAY TO MAKE IT SIMPLER
                    Bot.Game.SetSail(Bot.Game.MyPirates()[Pirates[i]],
                                     Bot.Game.GetDirections(Bot.Game.MyPirates()[Pirates[i]],Bot.Game.MyPirates()[Pirates[0]])[0]);
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
    }
}