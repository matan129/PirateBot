#region Usings

using System;
using System.Collections.Generic;
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
        /// Counter of the number of turn this groupis trying to get into structure
        /// </summary>
        private int _formTurnsAttempt;

        /// <summary>
        ///     Direction of the group to make navigation more precise
        /// </summary>
        public HeadingVector Heading { get; private set; }

        /// <summary>
        ///     List of the indexes of the pirates in this group
        /// </summary>
        public List<int> Pirates { get; private set; }

        /// <summary>
        ///     The target of the Group
        /// </summary>
        public ITarget Target { get; private set; }

        /// <summary>
        ///     The group's role (i.e. destroyer or attacker)
        /// </summary>
        public GroupRole Role { get; private set; }

        /// <summary>
        ///     List of priorities for this group
        /// </summary>
        public List<Score> Priorities { get; private set; }

        /// <summary>
        ///     A thread for complex calculations that can be ran in parallel to other stuff
        /// </summary>
        public Thread CalcThread { get; private set; }

        /// <summary>
        ///     static member to give each group a unique id based on its number of creation
        /// </summary>
        public static int GroupCounter { get; private set; }

        /// <summary>
        ///     The required location for each pirate in the group to get to attack structure
        /// </summary>
        public Dictionary<int, Location> FormOrders { get; private set; }

        #endregion

        #region Constructors & Initializers

        /// <summary>
        ///     Creates a new group with set amount of ships (without thinking to much)
        /// </summary>
        /// <param name="index">The stating index of the fist pirate in the group</param>
        /// <param name="amount">How many pirates will be in the group</param>
        public Group(int index, int amount)
        {
            this.Pirates = new List<int>();
            this.Heading = new HeadingVector(0, 0);
            this.Priorities = new List<Score>();
            this.Role = new GroupRole();

            //get id and update counter
            this.Id = Group.GroupCounter++;

            Bot.Game.Debug("===================GROUP {0}===================", this.Id);

            //Add pirates
            for (; amount > 0; amount--)
            {
                this.Pirates.Add(index + amount - 1);
            }

            //generate forming (getting into structure) instructions
            this.GenerateFormationInstructions();
        }

        #endregion

        public override string ToString()
        {
            return "Group - id: " + this.Id + " pirate count: " + this.Pirates.Count + "location: " +
                   this.FindCenter(true)
                   + " heading: " + Heading;
        }

        public void Debug()
        {
            Bot.Game.Debug("------------------------GROUP " + Id + " ----------------------------------");
            Bot.Game.Debug(this.Pirates.Count + " Pirates: " + string.Join(", ", this.Pirates));
            Bot.Game.Debug("Location: " + GetLocation().ToString() + " Heading: " + Heading.ToString());
            Bot.Game.Debug("Target: " + Target.ToString() + " Location: " + Target.GetLocation());
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
            }
            else if (!object.Equals(this.Target, target))
            {
                this.Target = target;
                this.Heading.SetCoordinates();
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
                this.Pirates = new List<int>();

            foreach (int pirate in this.Pirates)
            {
                x += Bot.Game.GetMyPirate(pirate).Loc.Col;
                y += Bot.Game.GetMyPirate(pirate).Loc.Row;
            }

            try
            {
                return new Location(y / this.Pirates.Count, x / this.Pirates.Count);
            }
            catch (Exception) //TODO WHY THERE IS A CATCH HERE?
            {
                return new Location(0, 0);
            }
        }

        /// <summary>
        ///     Decides where to move each pirate in the group
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A list that matches each pirate in the group a location to move to</returns>
        public IEnumerable<KeyValuePair<Pirate, Direction>> GetGroupMoves(CancellationToken cancellationToken)
        {
            //Note that IEnumerable gives you the possibility of doing a yield return statement
            //yield return returns one element each time, 
            //So we don't have to explicitly keep a list of the moves in this function
            int tryAlternateDirection = Bot.Game.GetTurn() % 2;

            //Check if the group is formed into structure. If not, get the moves to get into the structure
            if (!this.IsFormed())
            {
                //return for each of our pirate its move
                foreach (KeyValuePair<Pirate, Direction> keyValuePair in this.GetStructureMoves(cancellationToken))
                    yield return keyValuePair;
            }
            else //if the group is in structure and ready to attack
            {
                //Convert the list of pirate indexes we have into a list of pirates
                List<Pirate> myPirates = this.Pirates.ConvertAll(p => Bot.Game.GetMyPirate(p));

                //Proceed to moving to the target unless it's a NoTarget - then we stay in place
                if (this.Target.GetTargetType() != TargetType.NoTarget)
                {
                    /*
                    //Target location
                    Location targetLoc = this.Target.GetLocation();

                    //Get all the moves from the center pirate in the group to the target
                    List<Direction> possibleDirections = Bot.Game.GetDirections(this.FindCenter(true),
                        targetLoc);

                    //Throwing an exception if cancellation was requested.
                    cancellationToken.ThrowIfCancellationRequested();

                    //A list of the directions we can actually move in (see below)
                    List<Direction> filteredDirections = new List<Direction>(possibleDirections.Count);

                    //iterate over all the possible directions
                    foreach (Direction dir in possibleDirections)
                    {
                        //check if all the pirates in the group can go in that direction (not out of the map / enemy zone)
                        //if OK add this direction to the list of actually possible directions
                        if (myPirates.All(pete => Bot.Game.IsPassable(Bot.Game.Destination(pete, dir))))
                            filteredDirections.Add(dir);
                    }

                    //since we are in structure all the group should travel in sync. So everyone's direction is the master direction
                    Direction master = Direction.NOTHING;

                    //if we already have a pirate on the target do not move
                    if (myPirates.Any(pete => pete.Loc == targetLoc))
                        master = Direction.NOTHING;

                    //If we need to move, check if we can alternate directions 
                    //(usually there are 2 possible directions so we zig-zag)
                    if (filteredDirections.Count >= tryAlternateDirection + 1)
                        master = filteredDirections[tryAlternateDirection];
                    else if (filteredDirections.Count != 0)
                        master = filteredDirections.First();
                    else //if there are no possible direction we're stuck, and need to move back
                    {
                        foreach (Direction dir in possibleDirections)
                        {
                            //get the direction oppsite to the direction in possbileDirection (which are seemingly impossible at this point)
                            Direction opposite = dir.Oppsite();

                            //check if this direction is OK with all the pirates
                            if (myPirates.All(pete => Bot.Game.IsPassable(Bot.Game.Destination(pete, opposite))))
                            {
                                master = opposite;
                                break;
                            }
                        }
                    }

                    //sort the pirates in a way the closest ones to the target will travel first in order to avoid collisions
                    myPirates.Sort(
                        (b, a) => Bot.Game.Distance(a.Loc, targetLoc).CompareTo(Bot.Game.Distance(b.Loc, targetLoc)));
                    */
                    Direction master = Target.GetDirection(this);

                    //sort the pirates in a way the closest ones to the target will travel first in order to avoid collisions
                    myPirates =
                        myPirates.OrderBy(
                            p =>
                                Navigator.CalcDistFromLine(new Location(0, 0), GetLocation(),
                                    (new HeadingVector(master)).Orthogonal())).ToList();
                    //return for each pirate the pirate and its direction
                    foreach (Pirate myPirate in myPirates)
                    {
                        yield return new KeyValuePair<Pirate, Direction>(myPirate, master);
                    }

                    //update heading
                    this.Heading.adjustHeading(master);
                }
                else //stay if we are on target
                {
                    //return Direction.NOTHING for all pirates we got
                    foreach (Pirate myPirate in myPirates)
                    {
                        yield return new KeyValuePair<Pirate, Direction>(myPirate, Direction.NOTHING);
                    }
                }
            }
        }

        /// <summary>
        /// Get the correct moves to get into structure
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<Pirate, Direction>> GetStructureMoves(CancellationToken cancellationToken)
        {
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
        }

        /// <summary>
        ///     Return the percent of lost pirates in this group
        /// </summary>
        /// <returns></returns>
        private double CasualtiesPercent()
        {
            return 100 * (this.Pirates.Count(p => Bot.Game.GetMyPirate(p).IsLost) / this.Pirates.Count);
        }

        /// <summary>
        ///     Checks if the group is formed
        /// </summary>
        /// <returns></returns>
        private bool IsFormed(bool checkCasualties = true, int casualtiesThreshold = 20)
        {
            if (checkCasualties)
                if (this.CasualtiesPercent() > casualtiesThreshold) //if there are many casualties
                    return false;

            //the offsets from the original location.
            //this test assumes that the group has moves but *relatively* everyone is in place.
            int deltaCol = 0, deltaRow = 0;

            //a flag to indicate of the current 
            bool deltaFlag = true;

            //bool to flag if there is need for another formation test (see below)
            bool confirmUnstructured = false;
            Pirate pete = null;

            //iterate over the forming instructions 
            foreach (KeyValuePair<int, Location> formOrder in this.FormOrders)
            {
                //get the actual pirate from its ID
                pete = Bot.Game.GetMyPirate(formOrder.Key);

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
                Bot.Game.Debug("Group {0} is formed", this.Id);
                return true;
            }
            //else, proceed to the next test

            //find the central pirate in the group
            Location pivot = this.FindCenter(true);

            //the group's new structure (formation)
            Location[] structureFull = null;

            //try to get a new formation
            try
            {
                structureFull = this.GenerateGroupStructure(pivot);
            }
            catch //if there's an exception (such as InvalidLocationException) return false
            {
                Bot.Game.Debug("Group {0} is not formed yet", this.Id);
                return false;
            }

            //if we still failed to get a new structure for whatever reason...
            if (structureFull == null)
            {
                //...return false
                Bot.Game.Debug("Group {0} is not formed yet", this.Id);
                return false;
            }

            //if we got a new structure successfully...
            //this is the number of empty location in the group's structure
            int emptyCells = 0;

            //iterate over all the locations in the new structure
            foreach (Location loc in structureFull)
            {
                //try to find a pirate in the location
                Pirate p = Bot.Game.GetPirateOn(loc);

                //if there's not pirate of ours on the location
                if (!(p != null && p.Owner == Consts.ME))
                    //advance the counter
                    emptyCells++;
            }

            //check if the empty cells is what it should be
            //(the structure array might be larger than the # of pirates in the group,
            //like if we have 4 pirates we need the 2nd ring (index 1) but but there will be on free spot)
            if (emptyCells == structureFull.Length - this.Pirates.Count)
            {
                Bot.Game.Debug("Group {0} is formed", this.Id);
                return true;
            }

            //if we are still not formed, return the right answer
            Bot.Game.Debug("Group {0} is not formed yet", this.Id);
            return false;
        }

        /// <summary>
        ///     Generates instructions to form into optimal structure
        /// </summary>
        /// <param name="structure">Optional pre-calculated structure to generate instructions to</param>
        private void GenerateFormationInstructions(Location[] structure = null)
        {
            //reser the forming attempts counter
            this._formTurnsAttempt = 0;

            //find the average location of the group (not the center pirate!)
            Location center = this.FindCenter(false);

            //if we didn't get a pre calculated structure, calculate it below
            if (structure == null)
            {
                Bot.Game.Debug("Generating group structure");

                //Generate location array (structure) for the group
                while (true)
                {
                    try
                    {
                        //generate the structure
                        structure = this.GenerateGroupStructure(center);
                        break;
                    }
                    catch (InvalidLocationException ex)
                    {
                        //if the location is invalid (i.e. the current center requires a location outside the map)
                        //advance the location towards the center of the map
                        center = center.AdvancePivot();
                    }
                }
            }

            //flag array to signal if a location in the structure is already taken 
            bool[] matchedLocations = new bool[structure.Length];

            //the orders to form to
            Dictionary<Pirate, Location> orders = new Dictionary<Pirate, Location>();

            //all the pirates in the group converted from their IDs
            List<Pirate> groupPirates = this.Pirates.ConvertAll(p => Bot.Game.GetMyPirate(p));

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
            Bot.Game.Debug("====FORMING TO====");
            foreach (KeyValuePair<int, Location> formOrder in this.FormOrders)
            {
                Bot.Game.Debug(Bot.Game.GetMyPirate(formOrder.Key) + "," + formOrder.Value);
            }
            Bot.Game.Debug("==================");
        }

        /// <summary>
        ///     Get the structure for the group
        /// </summary>
        /// <param name="pivot">The center of the group</param>
        /// <returns></returns>
        private Location[] GenerateGroupStructure(Location pivot)
        {
            //find the required ring index for this group (see proff in calculation folder in the repo)
            int requiredRing = (int) Math.Ceiling((decimal) (this.Pirates.Count - 1) / 4);

            //list of location in all the rings
            List<Location> rings = new List<Location>();

            //generate the location for each ring
            for (int ordinal = 0; ordinal <= requiredRing; ordinal++)
            {
                rings.AddRange(Group.GenerateRingLocations(pivot, ordinal));
            }

            //convert the list into array and return it
            return rings.Take(this.Pirates.Count).ToArray();
        }

        /// <summary>
        /// returns the amount of rings in the formation for given amount of pirates
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
        /// <exception cref="InvalidLocationException">This method will throw this exception if a location it generated is not passable</exception>
        /// <returns></returns>
        private static List<Location> GenerateRingLocations(Location pivot, int ringOrdinal)
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
                if (Bot.Game.IsInMap(y1) || !Bot.Game.IsPassable(y1))
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
                if (Bot.Game.IsInMap(y2) || !Bot.Game.IsPassable(y2))
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
        ///     Find the center of the group
        /// </summary>
        /// <param name="enforcePirate">
        ///     if you ant the function to strictly return a location of a pirate or just the average
        ///     location
        /// </param>
        /// <returns>The center pirate</returns>
        public Location FindCenter(bool enforcePirate)
        {
            //convert all the pirates indexes to actual pirate list
            List<Pirate> myPirates = this.Pirates.ConvertAll(p => Bot.Game.GetMyPirate(p));

            //init the average location
            Location averageLocation = new Location(0, 0);

            //iterate over all the pirates and add their locations to the sum
            foreach (Pirate myPirate in myPirates)
            {
                averageLocation.Row += myPirate.Loc.Row;
                averageLocation.Col += myPirate.Loc.Col;
            }

            //calc the average
            averageLocation.Col /= myPirates.Count;
            averageLocation.Row /= myPirates.Count;

            //if the caller strictly wants the central pirate of the group, 
            //calcute it by assuming that the center pirate is the closest to the average location
            if (enforcePirate)
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
            return this.Pirates.ConvertAll(p => Bot.Game.GetMyPirate(p)).Count(p => !p.IsLost);
        }

        /// <summary>
        ///     Calculate target priorities for this group
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
        public void CalcPriorities(CancellationToken cancellationToken)
        {
            //inital path finding for this group
            Navigator.UpdateMap(this.Pirates.Count);

            //init some lists
            List<ITarget> priorityList = new List<ITarget>();
            List<Score> scores = new List<Score>();

            //Add all targets to the list

            //TODO Fix enemy group targeting
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
    }
}