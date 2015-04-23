using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Britbot.PriorityQueue;

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
        private List<SimulatedIsland> Islands;

        /// <summary>
        /// List of all friendly groups
        /// </summary>
        private List<SimularedGroup> MyGroups;

        /// <summary>
        /// List of all enemy groups
        /// </summary>
        private List<SimularedGroup> EnemyGroups;


        public SimulatedGame()
        {
            //initialize stuff
            this.CommingEvents = new HeapPriorityQueue<SimulatedEvent>((int)(2 * Magic.MaxCalculableDistance));
            this.Islands = new List<SimulatedIsland>();
            this.MyGroups = new List<SimularedGroup>();
            this.EnemyGroups = new List<SimularedGroup>();

            //add the ending Event
            this.CommingEvents.Enqueue(null, Magic.MaxCalculableDistance);


            //set Islands

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
