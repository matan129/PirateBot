#region #Usings

using System.Collections.Generic;
using Britbot.PriorityQueue;
using Pirates;
using System;
using System.Threading;

#endregion

namespace Britbot.Simulator
{
    /// <summary>
    ///     This class represents a possibility of a game based on an assignment
    /// </summary>
    class SimulatedGame
    {
        #region Fields & Properies

        /// <summary>
        ///     A priority queue representing the events to come in this scenario
        ///     Events are prioratized in a cronological order
        /// </summary>
        private HeapPriorityQueue<SimulatedEvent> CommingEvents;

        /// <summary>
        ///     List of all enemy groups
        /// </summary>
        public Dictionary<int,SimulatedGroup> EnemyGroups;

        /// <summary>
        ///     List of the islands in the game
        /// </summary>
        public Dictionary<int,SimulatedIsland> Islands;

        /// <summary>
        ///     List of all friendly groups
        /// </summary>
        public Dictionary<int, SimulatedGroup> MyGroups;

        /// <summary>
        ///     the score
        /// </summary>
        public double Score { get; private set; }

        /// <summary>
        ///     the current turn of the simulation
        /// </summary>
        public int CurrentTurn { get; set; }


        public double MyIslandCount;
        public double EnemyIslandCount;
        public double MyDeadPirates;
        public double EnemyDeadPirates;

        public double OriginalMyIslandCount;
        public double OriginalEnemyIslandCount;
        public double OriginalMyDeadPirates;
        public double OriginalEnemyDeadPirates;

        /// <summary>
        /// a queue representing the constant events
        /// those that will happen in all possible assignments
        /// </summary>
        public List<SimulatedEvent> ConstantEvents;

        #endregion

        #region Constructors & Initializers

        /// <summary>
        /// A c'tor, this should allocate memory
        /// This should be called ONCE PER TURN
        /// </summary>
        public SimulatedGame()
        {
            //initialize stuff
            this.CommingEvents = new HeapPriorityQueue<SimulatedEvent>((int) (2 * Magic.MaxCalculableDistance));
            this.ConstantEvents = new List<SimulatedEvent>();
            this.Islands = new Dictionary<int,SimulatedIsland>();
            this.MyGroups = new Dictionary<int, SimulatedGroup>();
            this.EnemyGroups = new Dictionary<int, SimulatedGroup>();

            //add the ending Event
            this.ConstantEvents.Add(new SimulationEndEvent(Magic.SimulationLength));

            this.OriginalEnemyDeadPirates = 0;
            this.OriginalMyDeadPirates = 0;
            this.OriginalMyIslandCount = 0;
            this.OriginalEnemyIslandCount = 0;

            //set my groups
            foreach (Group group in Commander.Groups)
            {
                //check if capturing
                bool isCapturing = false;
                foreach(SmartIsland sIsland in SmartIsland.IslandList)
                {
                    if(group.MinimalETATo(sIsland.Loc) == 0)
                    {
                        isCapturing = true;
                        break;
                    }
                }
                this.MyGroups.Add(group.Id, new SimulatedGroup(group.Id, Consts.ME, group.LiveCount(), isCapturing));
            }

            //set enemy group
            foreach (EnemyGroup eGroup in Enemy.Groups)
            {
                //check if capturing
                bool isCapturing = false;
                foreach (SmartIsland sIsland in SmartIsland.IslandList)
                {
                    if (eGroup.MinimalETATo(sIsland.Loc) == 0)
                    {
                        isCapturing = true;
                        break;
                    }
                }
                this.EnemyGroups.Add(eGroup.Id, new SimulatedGroup(eGroup.Id, Consts.ENEMY, eGroup.GetMaxFightPower(), isCapturing));
            }

            //set up islands
            foreach (SmartIsland sIsland in SmartIsland.IslandList)
            {
                SimulatedIsland newIsland = new SimulatedIsland(id: sIsland.Id,
                                                                owner: sIsland.Owner,
                                                                turnsBeingCaptured: sIsland.TurnsBeingCaptured,
                                                                value: sIsland.Value);

                //we set capturing teams through events

                this.Islands.Add(newIsland.Id, newIsland);

                if (sIsland.Owner == Consts.ME)
                    OriginalMyIslandCount+= sIsland.Value;
                if (sIsland.Owner == Consts.ENEMY)
                    OriginalEnemyIslandCount += sIsland.Value;
            }

            //set constant events
            foreach (SmartIsland sIsland in SmartIsland.IslandList)
            {
                //go over the enemy list of each island
                foreach (KeyValuePair<EnemyGroup, bool> enemy in sIsland.approachingEnemies)
                {
                    SimulatedEvent newEvent;
                    //if it is likely that he will come to the island
                    if (enemy.Value)
                    {
                        newEvent = new GroupArrivalEvent((int)enemy.Key.MinimalETATo(sIsland.Loc),
                                      this.Islands[sIsland.Id],
                                      this.EnemyGroups[enemy.Key.Id]);
                        this.ConstantEvents.Add(newEvent);
                    }
                    else if ((int)enemy.Key.MinimalETATo(sIsland.Loc) < 0 || Commander.IsDefensive() )
                    {
                        newEvent = new PossibleArrivalEvent((int)enemy.Key.MinimalETATo(sIsland.Loc),
                                      this.Islands[sIsland.Id],
                                      this.EnemyGroups[enemy.Key.Id]);
                        this.ConstantEvents.Add(newEvent);
                    }
                 }


                    //add the event
                    
            

                //check if there are ships on the islands
                foreach(Group g in Commander.Groups)
                {
                    if(g.MinimalETATo(sIsland.Loc) <= 1)
                    {
                        this.Islands[sIsland.Id].CapturingGroup = this.MyGroups[g.Id];
                        break;
                    }
                }
            }

        }

        /// <summary>
        /// This resets the simulation to the current game status, it should be used for every assignment
        /// </summary>
        public void ResetSimulation()
        {
            //reset general stuff
            this.CurrentTurn = Bot.Game.GetTurn();
            this.Score = 0;
            this.EnemyDeadPirates = this.OriginalEnemyDeadPirates;
            this.MyDeadPirates = this.OriginalMyDeadPirates;
            this.EnemyIslandCount = this.OriginalEnemyIslandCount;
            this.MyIslandCount = this.OriginalMyIslandCount;

            //reset islands
            foreach(KeyValuePair<int, SimulatedIsland> sIsland in this.Islands)
            {
                sIsland.Value.Owner = sIsland.Value.OriginalOwner;
                sIsland.Value.TurnsBeingCaptured = sIsland.Value.OriginalTrunsBeingCaptured;
                sIsland.Value.CapturingGroup = sIsland.Value.OriginalCapturingGroup;
            }

            //reset my groups
            foreach(KeyValuePair<int,SimulatedGroup> sGroup in this.MyGroups)
            {
                sGroup.Value.IsBusy = false;
                sGroup.Value.IsAlive = sGroup.Value.OriginalIsAlive;
                sGroup.Value.IsCapturing = sGroup.Value.OriginalIsCapturing;
                sGroup.Value.ReviveTurn = sGroup.Value.OriginalReviveTurn;
            }

            //reset enemy groups
            foreach (KeyValuePair<int, SimulatedGroup> sGroup in this.EnemyGroups)
            {
                sGroup.Value.IsBusy = false;
                sGroup.Value.IsAlive = sGroup.Value.OriginalIsAlive;
                sGroup.Value.IsCapturing = sGroup.Value.OriginalIsCapturing;
                sGroup.Value.ReviveTurn = sGroup.Value.OriginalReviveTurn;
            }

            //reset events
            this.CommingEvents.Clear();
            foreach(SimulatedEvent sEvent in this.ConstantEvents)
            {
                this.CommingEvents.Enqueue(sEvent, sEvent.Turn);
            }
        }
        #endregion

        /// <summary>
        ///     This enqueues a new event to the pending events
        /// </summary>
        /// <param name="newEvent">The new event</param>
        /// <param name="turn">the turn that the event will take place in</param>
        public void AddEvent(SimulatedEvent newEvent)
        {
            this.CommingEvents.Enqueue(newEvent, newEvent.Turn);
        }

        /// <summary>
        /// Simulates the game and calculates the score
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public double RunSimulation(CancellationToken cancellationToken)
        {
            int nextTurn = 0;

            //variables for PPT
            double PPT;

            int count = 0;

            //start going over events
            while(this.CommingEvents.Count > 0)
            {
                count++;
                //calculate PPT
                PPT = this.CalculatePpt();

                nextTurn = this.CommingEvents.First.Turn;

                //update score
                this.Score += (nextTurn - this.CurrentTurn) * PPT;

                //update iteration      
                this.CurrentTurn = nextTurn;

                //activate the comming event, if it is the end then break
                if (this.CommingEvents.Dequeue().Activate(this))
                    break;
            }

            //Logger.Write(this.MyIslandCount.ToString(),true);
            //Logger.Write(this.EnemyIslandCount.ToString(),true);


            Logger.Write("COUNT: " + count);
            return Score;
        }

        /// <summary>
        /// calculates how many point per turn each side will get
        /// </summary>
        /// <param name="friendlyPPT"></param>
        /// <param name="enemyPPT"></param>
        public double CalculatePpt()
        {
            //helper variables
            double friendlyPPT, enemyPPT;

            //calc score
            friendlyPPT = Math.Floor(Math.Pow(2, MyIslandCount - 1));
            enemyPPT = Math.Floor(Math.Pow(2, EnemyIslandCount - 1));

            //account for dead ships
            friendlyPPT -= Math.Pow(Magic.FriendlyBaseFactor, this.MyDeadPirates);

            //account for dead enemy ships
            enemyPPT -= Math.Pow(Magic.EnemyBaseFactor, this.EnemyDeadPirates);

            return friendlyPPT - enemyPPT;
        }
    }
}
