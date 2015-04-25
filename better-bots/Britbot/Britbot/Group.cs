#region #Usings

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Pirates;

#endregion

namespace Britbot
{
    /// <summary>
    ///     Represents a structure of few of our pirates that have a common goal
    /// </summary>
    public class Group
    {
        #region Fields & Properies

        /// <summary>
        ///     The Group ID number
        ///     useful for debugging
        /// </summary>
        public readonly int Id;

        /// <summary>
        ///     Counter of the number of turn this groupis trying to get into structure
        /// </summary>
        private int _formTurnsAttempt;

        /// <summary>
        ///     has the group changed this turn?
        /// </summary>
        private bool _hasChanged;

        /// <summary>
        ///     Direction of the group to make navigation more precise
        /// </summary>
        public HeadingVector Heading { get; private set; }

        /// <summary>
        ///     List of the indexes of the pirates in this group
        /// </summary>
        public ObservableCollection<int> Pirates { get; private set; }

        /// <summary>
        ///     The target of the Group
        /// </summary>
        public ITarget Target { get; private set; }

        /// <summary>
        ///     List of priorities for this group
        /// </summary>
        public List<Score> Priorities { get; private set; }

        /// <summary>
        ///     static member to give each group a unique id based on its number of creation
        /// </summary>
        public static int GroupCounter { get; private set; }

        /// <summary>
        ///     The required location for each pirate in the group to get to attack structure
        /// </summary>
        public Dictionary<int, Location> FormOrders { get; private set; }

        

        /// <summary>
        ///     The Distance Of a pirate in this group From it's target
        /// </summary>
        public int DistanceFromTarget
        {
            get
            {
                return Bot.Game.Distance(Bot.Game.AllMyPirates()[this.Pirates[0]].Loc, this.Target.GetLocation());
            }
        }

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     Creates a new group with the given pirates
        /// </summary>
        /// <param name="piratesId">the pirates indexes to add to this group</param>
        public Group(int[] piratesId)
            : this()
        {
            foreach (int index in piratesId)
                this.AddPirate(index);

            //generate forming (getting into structure) instructions
            this.GenerateFormationInstructions();
        }

        /// <summary>
        ///     Just a ctor to do to common stuff (called from the other ctors above by "this()" statement)
        /// </summary>
        private Group()
        {
            this.Pirates = new ObservableCollection<int>();

            this.Pirates.CollectionChanged += delegate
            {
                this._hasChanged = true;
                Logger.Write("Update Registered at group " + this.Id);
            };

            this.Heading = new HeadingVector(1, 0);
            this.Priorities = new List<Score>();

            //get id and update counter
            this.Id = Group.GroupCounter++;

            Logger.Write(string.Format("===================GROUP {0}===================", this.Id));
        }

        //[Obsolete("Please use the constructor that takes spesific pirates")]
        public Group(int index, int amount)
            : this()
        {
            //Add pirates
            for (; amount > 0; amount--)
            {
                this.Pirates.Add(index + amount - 1);
            }

            //generate forming (getting into structure) instructions
            this.GenerateFormationInstructions();
        }

        #endregion

        /// <summary>
        ///     You know what this does, don't you?
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Group - id: " + this.Id + " pirate count: " + this.Pirates.Count + "location: " +
                   this.FindCenter(true)
                   + " heading: " + this.Heading;
        }

        /// <summary>
        ///     Sets the target of the group, while doing so also resets the heading vector
        ///     if there is need (meaning if we didn't choose the same target again).
        /// </summary>
        /// <param name="target">the new target</param>
        public void SetTarget(ITarget target)
        {
            //if it isn't the same target as before update and reset heading
            if (this.Target == null)
            {
                this.Target = target;
                this.Heading.SetCoordinates();
                //tell the target it got assigned
                this.Target.TargetAssignmentEvent();
            }
            else if (!object.Equals(this.Target, target))
            {
                //tell the previous target it was dessigned
                this.Target.TargetDessignmentEvent();

                //update
                this.Target = target;
                this.Heading.SetCoordinates();

                //tell new target it got assigned
                this.Target.TargetAssignmentEvent();
            }
        }

        /// <summary>
        ///     Returns the average location for this group
        /// </summary>
        /// <returns>Returns the average location for this group</returns>
        public Location GetLocation()
        {
            //assigning X and Y to hold the sum
            int x = 0;
            int y = 0;

            if (this.Pirates == null)
                this.Pirates = new ObservableCollection<int>();

            foreach (int pirate in this.Pirates)
            {
                x += Bot.Game.GetMyPirate(pirate).Loc.Col;
                y += Bot.Game.GetMyPirate(pirate).Loc.Row;
            }

            try
            {
                return new Location(y / this.Pirates.Count, x / this.Pirates.Count);
            }
            catch (DivideByZeroException) // altough pirates count shouldn't be 0, just in case
            {
                return new Location(0, 0);
            }
        }

        /// <summary>
        ///     counts how many fighters does an enemy group has, not including ones capturing islands
        /// </summary>
        /// <returns>how many fighters does an enemy group has, not including ones capturing islands</returns>
        public int FightCount()
        {
            int count = 0;
            foreach (int pirate in this.Pirates)
            {
                if (!Bot.Game.isCapturing(Bot.Game.GetMyPirate(pirate)))
                    count++;
            }
            return count;
        }

        /// <summary>
        ///     Decides where to move each pirate in the group
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A list that matches each pirate in the group a location to move to</returns>
        public IEnumerable<KeyValuePair<Pirate, Direction>> GetGroupMoves(CancellationToken cancellationToken)
        {
            Logger.BeginTime("GetGroupMoves at group " + this.Id);
            //inital path finding for this group
            Logger.BeginTime("UpdateMap");
            Navigator.UpdateMap(this);
            Logger.StopTime("UpdateMap");

            //Check if the group is formed into structure. If not, get the moves to get into the structure
            if (!this.IsFormed())
            {
                this.Heading = new HeadingVector();
                //return for each of our pirate its move
                foreach (KeyValuePair<Pirate, Direction> keyValuePair in this.GetStructureMoves(cancellationToken))
                    yield return keyValuePair;
            }
            else //if the group is in structure and ready to attack
            {
                if (this.Target == null)
                    this.Target = new NoTarget();

                Direction master = this.Target.GetDirection(this);

                //Convert the list of pirate indexes we have into a list of pirates
                List<Pirate> myPirates = this.Pirates.ToList().ConvertAll(p => Bot.Game.GetMyPirate(p));

                //Proceed to moving to the target unless it's a NoTarget - then we stay in place
                if (this.Target.GetTargetType() != TargetType.NoTarget)
                {
                    //sort the pirates in a way the closest ones to the target will travel first in order to avoid collisions

                    //return for each pirate the pirate and its direction
                    foreach (Pirate myPirate in myPirates)
                        yield return new KeyValuePair<Pirate, Direction>(myPirate, master);

                    //update heading
                    this.Heading.adjustHeading(master);
                }
                else //stay if we are on target
                {
                    //return Direction.NOTHING for all pirates we got
                    foreach (Pirate myPirate in myPirates)
                        yield return new KeyValuePair<Pirate, Direction>(myPirate, Direction.NOTHING);
                }
            }
            Logger.StopTime("GetGroupMoves at group " + this.Id);
        }

        /// <summary>
        ///     Gets the minimal number of turns for the group to reach the given location
        /// </summary>
        /// <param name="location">the location tested</param>
        /// <returns>minimal number of turns till the group gets to the location</returns>
        public double MinimalETATo(Location location)
        {
            double min = Bot.Game.GetCols() + Bot.Game.GetRows();
            foreach (int pirate in this.Pirates)
            {
                if (Bot.Game.Distance(location, this.GetLocation()) < min)
                    min = Bot.Game.EuclidianDistanceSquared(location, this.GetLocation());
            }
            return min;
        }

        /// <summary>
        ///     Get the correct moves to get into structure
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<Pirate, Direction>> GetStructureMoves(CancellationToken cancellationToken)
        {
            Logger.BeginTime("GetStructureMoves");
            //check if we are not stuck try to get into formation for too long
            if (this._formTurnsAttempt > this.Pirates.Count * 2)
                //if we are stuck, request new instructions. This will reset the _formTurnsAttempt counter
                this.GenerateFormationInstructions();

            //advance the counter
            this._formTurnsAttempt++;

            //iterate over all the structure (each pirate in the group has a reserved location in the structure)
            foreach (KeyValuePair<int, Location> formOrder in this.FormOrders)
            {
                //Throwing an exception if cancellation was requested.
                cancellationToken.ThrowIfCancellationRequested();

                //Get the actual pirate object by its ID
                Pirate pete = Bot.Game.GetMyPirate(formOrder.Key);

                //if the pirate is already in place, make it stay in place
                if (pete.Loc.Col == formOrder.Value.Col && pete.Loc.Row == formOrder.Value.Row)
                {
                    yield return new KeyValuePair<Pirate, Direction>(pete, Direction.NOTHING);
                }
                else //if the pirate need to move into position
                {
                    //All the possible direction from the pirate to its position the the structure
                    List<Direction> possibleDirections = Bot.Game.GetDirections(pete, formOrder.Value);

                    int tryAlternate = Bot.Game.GetTurn() % 2;
                    List<Direction> filteredDirections = new List<Direction>(possibleDirections.Count);

                    //iterate over the possible directions
                    foreach (Direction t in possibleDirections)
                    {
                        //check if the direction is NOTHING - it means that the pirate is in place (double check with the previous if)
                        if (t == Direction.NOTHING)
                        {
                            filteredDirections.Add(t);
                            break;
                        }

                        //if the direction is actually passable add it to the list
                        if (Bot.Game.Destination(pete.Loc, t).IsActuallyPassable())
                            filteredDirections.Add(t);
                    }

                    //and return it
                    if (filteredDirections.Count == 0)
                        yield return new KeyValuePair<Pirate, Direction>(pete, Direction.NOTHING);
                    else if (Bot.Game.Distance(pete.Loc, formOrder.Value) <= 15)
                        yield return new KeyValuePair<Pirate, Direction>(pete, filteredDirections.First());
                    else if (filteredDirections.Count >= tryAlternate + 1)
                        yield return new KeyValuePair<Pirate, Direction>(pete, filteredDirections[tryAlternate]);
                    else
                        yield return new KeyValuePair<Pirate, Direction>(pete, filteredDirections.First());
                }
            }
            Logger.StopTime("GetStructureMoves");
        }

        /// <summary>
        ///     Return the percent of lost pirates in this group
        /// </summary>
        /// <returns></returns>
        private double CasualtiesPercent()
        {
            if(this.Pirates.Count > 0)
                return 100 * (this.Pirates.Count(p => Bot.Game.GetMyPirate(p).IsLost) / this.Pirates.Count);
            return 0;
        }

        /// <summary>
        ///     Checks if the group is formed
        /// </summary>
        /// <returns></returns>
        private bool IsFormed(bool checkCasualties = true, int casualtiesThresholdPercent = 20)
        {
            Logger.BeginTime("IsFormed at group " + this.Id);

            if (checkCasualties)
                if (this.CasualtiesPercent() > casualtiesThresholdPercent) //if there are many casualties
                {
                    Logger.StopTime("IsFormed at group " + this.Id);
                    return false;
                }

            //the offsets from the original location.
            //this test assumes that the group has moves but *relatively* everyone is in place.
            int deltaCol = 0, deltaRow = 0;

            //a flag to indicate of the current 
            bool deltaFlag = true;

            //bool to flag if there is need for another formation test (see below)
            bool confirmUnstructured = false;

            //iterate over the forming instructions 
            foreach (KeyValuePair<int, Location> formOrder in this.FormOrders)
            {
                //get the actual pirate from its ID
                Pirate pete = Bot.Game.GetMyPirate(formOrder.Key);

                if (pete != null)
                {
                    //ignore dead pirates
                    if (pete.IsLost)
                        continue;

                    //we calculate the deltas for the first pirate only
                    if (deltaFlag)
                    {
                        deltaCol = formOrder.Value.Col - pete.Loc.Col;
                        deltaRow = formOrder.Value.Row - pete.Loc.Row;
                        deltaFlag = false;

                        //skip to the next pirate
                        continue;
                    }

                    //check if the pirate is in its right spot relatively to the group
                    if (pete.Loc.Col + deltaCol != formOrder.Value.Col || pete.Loc.Row + deltaRow != formOrder.Value.Row)
                    {
                        //if the pirate is not in place, proceed to the next test. 
                        //because there's slight chance one of the pirates got stuck for a little bit but it is still in OK location
                        confirmUnstructured = true;
                        break;
                    }
                }
            }

            //if the group passed the previous test, return true (meaning that the group is formed)
            if (!confirmUnstructured)
            {
                Logger.Write(string.Format("Group {0} is formed", this.Id), true);
                Logger.StopTime("IsFormed at group " + this.Id);
                return true;
            }

            //else, proceed to the next test

            //find the central pirate in the group
            Location pivot = this.FindCenter(true);

            //the group's new structure (formation)
            Location[][] structureFull = null;

            //try to get a new formation
            try
            {
                structureFull = this.GenerateGroupStructure(pivot, false);
            }
            catch //if there's an exception (such as InvalidLocationException) return false
            {
                Logger.Write(String.Format("Group {0} is not formed yet", this.Id), true);
                Logger.StopTime("IsFormed at group " + this.Id);
                return false;
            }

            //if we still failed to get a new structure for whatever reason...
            if (structureFull == null)
            {
                //...return false
                Logger.Write(String.Format("Group {0} is not formed yet", this.Id), true);
                Logger.StopTime("IsFormed at group " + this.Id);
                return false;
            }

            //if we got a new structure successfully...
            //this is the number of empty location in the group's structure
            int emptyCells = 0;

            //iterate over all the locations in the new structure
            for (int i = 0; i < structureFull.Length; i++)
            {
                for (int k = 0; k < structureFull[i].Length; k++)
                {
                    //try to find a pirate in the location
                    Pirate p = Bot.Game.GetPirateOn(structureFull[i][k]);

                    //if there's not pirate of ours on the location
                    if (!(p != null && p.Owner == Consts.ME))
                    {
                        //check if this is the last ring, where some empty spots are OK
                        if (i == structureFull.Length - 1)
                            //advance the counter
                            emptyCells++;
                        else
                        {
                            //quit the iterations because the group is not formed for sure
                            //I know goto is bad but this is a legitimate case for this (rare one!)
                            goto ReturnFalse;
                        }
                    }
                }
            }

            //check if the empty cells is what it should be
            //(the structure array might be larger than the # of pirates in the group,
            //like if we have 4 pirates we need the 2nd ring (index 1) but but there will be on free spot)
            if (emptyCells == structureFull.Length - this.Pirates.Count)
            {
                Logger.Write(String.Format("Group {0} is formed", this.Id), true);
                Logger.StopTime("IsFormed at group " + this.Id);
                return true;
            }

            ReturnFalse:
            //if we are still not formed, return the right answer
            Logger.Write(String.Format("Group {0} is not formed yet", this.Id), true);

            Logger.StopTime("IsFormed at group " + this.Id);
            return false;
        }

        /// <summary>
        ///     Generates instructions to form into optimal structure
        /// </summary>
        /// <param name="structure">Optional pre-calculated structure to generate instructions to</param>
        private void GenerateFormationInstructions(Location[] structure = null)
        {
            //reset the forming attempts counter
            this._formTurnsAttempt = 0;

            //find the average location of the group (not the center pirate!)
            Location center = this.FindCenter(false);

            //if we didn't get a pre calculated structure, calculate it below
            if (structure == null)
            {
                Logger.Write("Generating group structure");

                //Generate location array (structure) for the group
                while (true)
                {
                    try
                    {
                        //generate the structure
                        structure = this.GenerateGroupStructure(center).Flatten();
                        break;
                    }
                    catch (InvalidLocationException)
                    {
                        //if the location is invalid (i.e. the current center requires a location outside the map)
                        //advance the location towards the center of the map
                        center = center.AdvancePivot();
                    }
                }
            }

            Logger.Write("Generating group structure OK");

            //flag array to signal if a location in the structure is already taken 
            bool[] matchedLocations = new bool[structure.Length];

            //the orders to form to
            Dictionary<Pirate, Location> orders = new Dictionary<Pirate, Location>();

            //all the pirates in the group converted from their IDs
            List<Pirate> groupPirates = this.Pirates.Distinct().ToList().ConvertAll(p => Bot.Game.GetMyPirate(p));

            //sort the pirates by distance to the center (closer are first)
            groupPirates.Sort((b, a) => Bot.Game.Distance(a.Loc, center).CompareTo(Bot.Game.Distance(b.Loc, center)));

            //Match a pirate for each location in the structure
            foreach (Pirate pirate in groupPirates)
            {
                Location closestLocation = null;
                int minDistance = Bot.Game.GetCols() + Bot.Game.GetRows();
                int matchIndex = 0;

                //iterate over all the location in the structure
                for (int i = 0; i < structure.Length; i++)
                {
                    //Skip over taken spots
                    if (matchedLocations[i])
                        continue;

                    //find the best location in the structure, which is the closest one
                    if (Bot.Game.Distance(pirate.Loc, structure[i]) < minDistance)
                    {
                        minDistance = Bot.Game.Distance(pirate.Loc, structure[i]);
                        closestLocation = structure[i];
                        matchIndex = i;
                    }
                }

                //flag that this location is taken
                matchedLocations[matchIndex] = true;

                //add the instruction
                orders.Add(pirate, closestLocation);
            }

            //sort the orders so the closest pirates are first to avoid collisions and set them to the right class property
            this.FormOrders = orders.OrderBy(pair => Bot.Game.Distance(pair.Key.Loc, pair.Value))
                .ToDictionary(pair => pair.Key.Id, pair => pair.Value);

            //debug prints
            Logger.Write("====FORMING TO====");
            foreach (KeyValuePair<int, Location> formOrder in this.FormOrders)
            {
                Logger.Write(Bot.Game.GetMyPirate(formOrder.Key) + "," + formOrder.Value);
            }
            Logger.Write("==================");
        }

        /// <summary>
        ///     Get the structure for the group
        /// </summary>
        /// <param name="pivot">The center of the group</param>
        /// <param name="trim">If  to trim the locations to the number of pirates in this group</param>
        /// <returns>An array of location array per ring required</returns>
        private Location[][] GenerateGroupStructure(Location pivot, bool trim = true)
        {
            //find the required ring index for this group (see proof in calculation folder in the repo)
            int maxRing = (int) Math.Ceiling((decimal) (this.Pirates.Count - 1) / 4);

            //list of location in all the rings. Note that we add one because ring # is 0-based
            Location[][] rings = new Location[maxRing + 1][];

            //generate the locations for each ring
            for (int i = 0; i < rings.Length; i++)
            {
                rings[i] = Group.GenerateRingLocations(pivot, i).ToArray();
            }

            /*
             * trim the last ring if required. i.e. if we have 4 pirate, maxRing will be 1 and it will have 5 spots.
             * so if trimming was required the last ring will be trimmed to 3 (so 3 + 1 is the number of pirates in this group).
             */
            if (trim)
            {
                int spareSpots = Group.GetStructureVolume(maxRing) - this.Pirates.Count;
                rings[maxRing] = rings[maxRing].Take(rings[maxRing].Length - spareSpots).ToArray();
            }


            //return the location array
            return rings;
        }

        /// <summary>
        ///     returns the amount of rings in the formation for given amount of pirates
        /// </summary>
        /// <param name="pirateNum">amount of pirates in formation</param>
        /// <returns>the amount of rings in the formation</returns>
        public static int GetRingCount(int pirateNum)
        {
            return (int) Math.Ceiling((decimal) (pirateNum - 1) / 4);
        }

        /// <summary>
        ///     Get the ring of the specified index relative to the given pivot
        /// </summary>
        /// <param name="pivot">The ring's center</param>
        /// <param name="ringOrdinal">the index of the ring</param>
        /// <exception cref="InvalidLocationException">
        ///     This method will throw this exception if a location it generated is not
        ///     passable
        /// </exception>
        /// <returns></returns>
        internal static List<Location> GenerateRingLocations(Location pivot, int ringOrdinal)
        {
            //check if the ring index is OK
            if (ringOrdinal < 0)
                throw new InvalidRingException("Ring ordinal must be non-negative");

            //create a list of this ring's locations
            List<Location> ring = new List<Location>(ringOrdinal * 4);
            int a = pivot.Row;
            int b = pivot.Col;

            //this solves the equation I described in the calculations folder
            for (int x = a - ringOrdinal; x <= a + ringOrdinal; x++)
            {
                //first solution
                Location y1 =
                    new Location(x,
                        (int)
                            ((2 * b +
                              Math.Sqrt(4 * Math.Pow(b, 2) +
                                        4 * (Math.Pow(ringOrdinal - Math.Abs(a - x), 2) - Math.Pow(b, 2)))) / 2));

                //if the location is nit passable, throw an expection (caught by the calling function)
                if (!Bot.Game.IsInMap(y1) || !Bot.Game.IsPassable(y1))
                    throw new InvalidLocationException("Location is not passable!");

                //add the location to the ring
                ring.Add(y1);

                //second solution
                Location y2 =
                    new Location(x,
                        (int)
                            ((2 * b -
                              Math.Sqrt(4 * Math.Pow(b, 2) +
                                        4 * (Math.Pow(ringOrdinal - Math.Abs(a - x), 2) - Math.Pow(b, 2)))) / 2));

                //if the location is nit passable, throw an expection (caught by the calling function)
                if (!Bot.Game.IsInMap(y2) || !Bot.Game.IsPassable(y2))
                    throw new InvalidLocationException("Location is not passable!");

                //Check for duplicates
                if (y1.Col != y2.Col || y1.Row != y2.Row)
                    //if the two solution are different, add the second one
                    ring.Add(y2);
            }
            //return the list of location of the ring
            return ring;
        }

        /// <summary>
        ///     Does updates for the group when changed.
        /// </summary>
        public void Update()
        {
            if (this._hasChanged /*|| this.Pirates.Any(p => Bot.Game.GetMyPirate(p).IsLost)*/)
            {
                //this.Pirates.RemoveAll(p => Bot.Game.GetMyPirate(p).IsLost);
                this.GenerateFormationInstructions();
                this._hasChanged = false;
            }
        }

        /// <summary>
        ///     Find the center of the group
        /// </summary>
        /// <param name="forcePirate">
        ///     if you ant the function to strictly return a location of a pirate or just the average
        ///     location
        /// </param>
        /// <returns>The center pirate</returns>
        public Location FindCenter(bool forcePirate)
        {
            if (this.Pirates.Count == 0)
                return new Location(0, 0);

            //convert all the pirates indexes to actual pirate list
            List<Pirate> myPirates = this.Pirates.ToList().ConvertAll(p => Bot.Game.GetMyPirate(p));

            //init the average location
            Location averageLocation = new Location(0, 0);

            //iterate over all the pirates and add their locations to the sum
            foreach (Pirate myPirate in myPirates)
            {
                if (myPirate == null)
                    continue;

                averageLocation.Row += myPirate.Loc.Row;
                averageLocation.Col += myPirate.Loc.Col;
            }

            //calc the average
            averageLocation.Col /= myPirates.Count;
            averageLocation.Row /= myPirates.Count;

            //if the caller strictly wants the central pirate of the group, 
            //calcute it by assuming that the center pirate is the closest to the average location
            if (forcePirate)
            {
                int minDistance = Bot.Game.GetCols() + Bot.Game.GetCols();
                Pirate pete = null;

                //iterate over all the pirate and find the one with the minimun distance to the average location
                foreach (Pirate pirate in myPirates)
                {
                    if (pirate.IsLost)
                        continue;

                    int currDistance = Bot.Game.Distance(averageLocation, pirate.Loc);
                    if (currDistance < minDistance)
                    {
                        minDistance = currDistance;
                        pete = pirate;
                    }
                }

                //set the returned location to the central pirate location
                if (pete != null)
                    averageLocation = pete.Loc;
            }

            //return the location
            return averageLocation;
        }

        /// <summary>
        ///     counts how many living pirates are in the group
        /// </summary>
        /// <returns>how many living pirates are in the group</returns>
        public int LiveCount()
        {
            if(this.Pirates != null)
                return this.Pirates.ToList().ConvertAll(p => Bot.Game.GetMyPirate(p)).Count(p => !p.IsLost);
            return 0;
        }

        /// <summary>
        ///     Calculate target priorities for this group
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
        public void CalcPriorities(CancellationToken cancellationToken)
        {
            Logger.BeginTime("CalcPriorities");
            //init some lists
            List<ITarget> priorityList = new List<ITarget>();
            List<Score> scores = new List<Score>();

            //Add all targets to the list

            //check if we need to chaise ships, if so add them to calculation
            if (Enemy.ShouldWeTryToCatchEnemyShips())
                priorityList.AddRange(Enemy.Groups);
            priorityList.AddRange(SmartIsland.IslandList);

            //Add a score for each target we got
            foreach (ITarget target in priorityList)
            {
                //Throwing an exception if cancellation was requested.
                cancellationToken.ThrowIfCancellationRequested();

                //calculate the score for this specific target
                Score newScore = target.GetScore(this);
                //check if score wasn't null, meaning if target was disqualified
                if (newScore != null)
                    scores.Add(newScore);
            }

            //set it to this instance of Group
            this.Priorities = scores;

            //check if priorities empty, if so add NoTarget To prevent dimension problem
            if (this.Priorities.Count == 0)
            {
                //create a NoTarget
                NoTarget noTarget = new NoTarget();
                this.Priorities.Add(noTarget.GetScore(this));
            }

            //limit outselfs to so and so targets
            //first sort by something (meanwhile distance)
            this.Priorities = this.Priorities.OrderBy(score => score.Eta).ToList();
            //throw away all but CalcMaxPrioritiesNum
            this.Priorities = this.Priorities.Take(Commander.CalcMaxPrioritiesNum()).ToList();

            Logger.StopTime("CalcPriorities");
        }

        /// <summary>
        ///     Adds a pirate to this group, removing it from any other group it was in.
        /// </summary>
        /// <param name="index">The pirate's index</param>
        public void AddPirate(int index)
        {
            //make sure that the pirate is not controlled by other groups by removing it from them
            foreach (Group g in Commander.Groups)
            {
                //NOTE! that is correct - it does not remove *at* the index but find the right one
                g.Pirates.Remove(index);
            }

            //add the pirate to this group index
            this.Pirates.Add(index);
        }

        public static int GetStructureVolume(int maxRing)
        {
            //this is basic summation of arithmetic sequence, excluding the 0th ring because it's special. then we add it back.
            return (((2 * maxRing + 2) * maxRing) + 1);
        }

        /// <summary>
        ///     Splits 'num' pirates from the group
        /// </summary>
        /// <param name="num">number of pirates to split</param>
        public IEnumerable<Group> Split(int num)
        {
            int[] outboundPirates = this.Pirates.Take(num).ToArray();
            this.Pirates.RemoveAll(outboundPirates.Contains);

            for (int i = 0; i < outboundPirates.Length; i++)
            {
                yield return new Group(new[] {outboundPirates[i]});
            }
        }

        /// <summary>
        ///     Joins a group to this group
        /// </summary>
        /// <param name="g">a group to be joind to this one</param>
        /// <param name="remove"></param>
        public void Join(Group g, bool remove = true)
        {
            foreach (int pirate in g.Pirates)
            {
                this.AddPirate(pirate);
            }

            if(remove)
                Commander.Groups.RemoveAll(group => group.Id == g.Id);
        }

        /// <summary>
        ///     Given two groups it will rearange the pirates in them so that the big group
        ///     will have the pirates who are the most addvanced
        /// </summary>
        /// <param name="bigGroup">the big group</param>
        /// <param name="smallGroup">the small group</param>
        public static void Switch(Group bigGroup, Group smallGroup)
        {
            //if we have no heading to work with
            if (bigGroup.Heading.Norm() == 0)
                return;

            //define the list of pirates of both groups
            List<int> pirateList = new List<int>();

            //add pirates of the biggroup
            pirateList.AddRange(bigGroup.Pirates);
            pirateList.AddRange(smallGroup.Pirates);
            Logger.Write("-------------------------------------Switch",true);
            Logger.Write("Biggroup: " + string.Join(", ", bigGroup.Pirates));
            Logger.Write("smallgroup: " + string.Join(", ", smallGroup.Pirates));
            Logger.Write("pirate list: " + string.Join(", ", pirateList));
            Logger.Write("heading: " + bigGroup.Heading);


            //sort array by allignment with the vector specified
            pirateList.Sort((p1, p2) => -1 * Navigator.ComparePirateByDirection(p1, p2, bigGroup.Heading));

            Logger.Write("pirate list: " + string.Join(", ", pirateList));

            //replace pirates
            bigGroup._hasChanged = true;
            bigGroup.Pirates = new ObservableCollection<int>(pirateList.GetRange(0, bigGroup.Pirates.Count));

            pirateList.RemoveRange(0, bigGroup.Pirates.Count);

            smallGroup._hasChanged = true;
            smallGroup.Pirates = new ObservableCollection<int>(pirateList);

            Logger.Write("Biggroup: " + string.Join(", ", bigGroup.Pirates));
            Logger.Write("smallgroup: " + string.Join(", ", smallGroup.Pirates));
        }

        /// <summary>
        ///     checks if two groups are messed around in one another
        ///     TODO: make this smarter
        /// </summary>
        /// <param name="g1">first group</param>
        /// <param name="g2">second group</param>
        /// <returns>true if they are close enouth</returns>
        public static bool CheckIfGroupsIntersects(Group g1, Group g2)
        {
            //first check direction stuff
            HeadingVector diff = HeadingVector.CalcDifference(g1.FindCenter(true), g2.FindCenter(true));

            //if they are not in the same direction or something
            if ((diff * g1.Heading < 0) || (g2.Heading * g1.Heading > 0))
            {
                if((g1.IsFormed()) && (g2.IsFormed()))
                    return false;
            }

            //going over all the pirates in G1
            foreach (int intPirate1 in g1.Pirates)
            {
                //convertion
                Pirate pirate1 = Bot.Game.GetMyPirate(intPirate1);

                //going over pirates in G2
                foreach (int intPirate2 in g2.Pirates)
                {
                    //conversion
                    Pirate pirate2 = Bot.Game.GetMyPirate(intPirate2);

                    //check if distance is small enough
                    if (Bot.Game.EuclidianDistanceSquared(pirate1.Loc, pirate2.Loc) <= Magic.GroupIntersectionDistance)
                        return true;
                }
            }

            //otherwise return false
            return false;
        }

        /// <summary>
        ///     Minimal distance between groups
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public int MinDistance(Group b)
        {
            int minD = 9999999;

            foreach (int pA in this.Pirates)
            {
                Pirate pat = Bot.Game.GetMyPirate(pA);

                foreach (int pB in b.Pirates)
                {
                    Pirate pete = Bot.Game.GetMyPirate(pB);

                    int d = Bot.Game.Distance(pat, pete);

                    if (d < minD)
                        minD = d;
                }
            }

            return minD;
        }
    }
}