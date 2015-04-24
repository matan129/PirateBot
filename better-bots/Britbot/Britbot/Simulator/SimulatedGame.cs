﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Britbot.PriorityQueue;
using Pirates;
namespace Britbot.Simulator
{
    /// <summary>
    /// This class represents a possibility of a game based on an assignment
    /// </summary>
    class SimulatedGame
    {
        /// <summary>
        /// A priority queue representing the events to come in this scenario
        /// Events are prioratized in a cronological order
        /// </summary>
        private HeapPriorityQueue<SimulatedEvent> CommingEvents;

        /// <summary>
        /// List of the islands in the game
        /// </summary>
        public SimulatedIsland [] Islands;

        /// <summary>
        /// List of all friendly groups
        /// </summary>
        private Dictionary<int,SimularedGroup> MyGroups;

        /// <summary>
        /// List of all enemy groups
        /// </summary>
        private Dictionary<int, SimularedGroup> EnemyGroups;

        /// <summary>
        /// the score
        /// </summary>
        private int Score;

        /// <summary>
        /// the current turn of the simulation
        /// </summary>
        public int CurrTurn;

        public SimulatedGame()
        {
            //initialize stuff
            this.CommingEvents = new HeapPriorityQueue<SimulatedEvent>((int)(2 * Magic.MaxCalculableDistance));
            this.Islands = new SimulatedIsland[Bot.Game.Islands().Count];
            this.MyGroups = new Dictionary<int, SimularedGroup>();
            this.EnemyGroups = new Dictionary<int, SimularedGroup>();

            //add the ending Event
            this.CommingEvents.Enqueue(null, Magic.MaxCalculableDistance);

            this.CurrTurn = 0;
            this.Score = 0;
            //set my groups
            foreach(Group group in Commander.Groups)
            {
                this.MyGroups.Add(group.Id,new SimularedGroup(group.Id, Consts.ME, group.LiveCount()));
            }

            //set enemy group
            foreach(EnemyGroup eGroup in Enemy.Groups)
            {
                this.EnemyGroups.Add(eGroup.Id, new SimularedGroup(eGroup.Id, Consts.ENEMY, eGroup.GetMaxFightPower()));
            }

        }

        /// <summary>
        /// This enqueues a new event to the pending events
        /// </summary>
        /// <param name="newEvent">The new event</param>
        /// <param name="turn">the turn that the event will take place in</param>
        public void AddEvent(SimulatedEvent newEvent, int turn)
        {
            this.CommingEvents.Enqueue(newEvent, turn);
        }
    }
}
