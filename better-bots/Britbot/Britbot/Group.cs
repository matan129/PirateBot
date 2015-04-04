using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Pirates;

namespace Britbot
{
    /// <summary>
    /// Represents a structure of few of our pirates that have a common goal
    /// </summary>
    public class Group
    {
        #region constructor

        /// <summary>
        /// Creates a new group with set amount of ships (without thinking to much)
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

            for (; amount > 0; amount--)
            {
                this.Pirates.Add(index + amount - 1);
            }
            
            this.FormDictionary();
        }

        #endregion

        /// <summary>
        /// Sets the target of the group, while doing so also resets the heading vector
        /// if there is need (meaning if we didn't choose the same target again).
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
        /// Returns the average location for this group
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
                return new Location(y/this.Pirates.Count, x/this.Pirates.Count);
            }
            catch (Exception)
            {
                return new Location(0, 0);
            }
        }

        /// <summary>
        /// Decides where to move each pirate in the group
        /// </summary>
        /// <returns>A list that matches each pirate in the group a location to move to</returns>
        public IEnumerable<KeyValuePair<Pirate,Direction>> GetGroupMoves()
        {
            int tryAlternateDirection = Bot.Game.GetTurn() % 2;

            //Note that IEnumerable gives you the possibility of doing a yield return statement
            //yield return returns one element each time, 
            //So we don't have to explicitly keep a list of the moves
            if (!this.IsFormed())
            {
                foreach (KeyValuePair<int, Location> formOrder in this.FormOrders)
                {
                    Pirate pete = Bot.Game.GetMyPirate(formOrder.Key);
                    if (pete.Loc.Col == formOrder.Value.Col && pete.Loc.Row == formOrder.Value.Row)
                    {
                        yield return new KeyValuePair<Pirate, Direction>(pete,Direction.NOTHING);
                        //Bot.Game.Debug("Skipping over " + pete);
                    }
                    else
                    {
                        //Bot.Game.Debug("Entering forming loop for {0} to go to {1}", pete, formOrder.Value);
                        List<Direction> possibleDirections = Bot.Game.GetDirections(pete, formOrder.Value);
                        Direction dir = Direction.NOTHING;

                        //Bot.Game.Debug("Possible Directions: " + String.Join(",", possibleDirections));

                        List<Direction> actualDirections = new List<Direction>();

                        for (int i = 0; i < possibleDirections.Count; i++)
                        {
                            dir = possibleDirections[i];

                            if (dir == Direction.NOTHING)
                                break;

                            if (Bot.Game.Destination(pete.Loc, dir).IsActuallyPassable())
                                break;
                        }

                        yield return new KeyValuePair<Pirate, Direction>(pete, dir);
                        
                        /*if(actualDirections.Count > 1)
                            
                        else if(actualDirections.Count == 1)
                            yield return new KeyValuePair<Pirate, Direction>(pete, actualDirections[0]);
                        else
                            yield return new KeyValuePair<Pirate, Direction>(pete,Direction.NOTHING);*/
                    }
                }
            }
            else
            {
                List<Pirate> myPirates = this.Pirates.ConvertAll(p => Bot.Game.GetMyPirate(p));

                if(this.Target.GetTargetType() != TargetType.NoTarget)
                {
                    List<Direction> possibleDirections = Bot.Game.GetDirections(this.FindCenter(true), this.Target.GetLocation());
                    
                    Direction master;
                    if (possibleDirections.Count > 1)
                        master = possibleDirections[tryAlternateDirection];
                    else
                        master = possibleDirections.First();
                    
                    Location targetLoc = this.Target.GetLocation();
                    myPirates.Sort(
                                   (b, a) => Bot.Game.Distance(a.Loc, targetLoc).CompareTo(Bot.Game.Distance(b.Loc, targetLoc)));

                    foreach (Pirate myPirate in myPirates)
                    {
                        yield return new KeyValuePair<Pirate, Direction>(myPirate, master);
                    }
                }
                else //stay
                {
                    foreach (Pirate myPirate in myPirates)
                    {
                        yield return new KeyValuePair<Pirate, Direction>(myPirate, Direction.NOTHING);
                    }
                }
            }
        }

        /// <summary>
        /// Return the percent of lost pirates in this group
        /// </summary>
        /// <returns></returns>
        private double CasualtiesPercent()
        {
            return 100*(this.Pirates.Count(p => Bot.Game.GetMyPirate(p).IsLost)/this.Pirates.Count);
        }

        /// <summary>
        /// Checks if the group is formed
        /// </summary>
        /// <returns></returns>
        private bool IsFormed()
        {
            if (this.CasualtiesPercent() > 20)
                return false;


            int deltaCol = 0, deltaRow = 0;
            bool deltaFlag = false;
            bool confirmUnstructured = false;
            Pirate pete = null;

            foreach (KeyValuePair<int, Location> formOrder in this.FormOrders)
            {
                pete = Bot.Game.GetMyPirate(formOrder.Key);

                if (pete != null)
                {
                    if (pete.IsLost)
                        continue;

                    if (!deltaFlag)
                    {
                        deltaCol = formOrder.Value.Col - pete.Loc.Col;
                        deltaRow = formOrder.Value.Row - pete.Loc.Row;
                        deltaFlag = true;
                    }

                    if (pete.Loc.Col + deltaCol != formOrder.Value.Col || pete.Loc.Row + deltaRow != formOrder.Value.Row)
                    {
                        //proceed to the next test
                        confirmUnstructured = true;
                        break;
                    }
                }
            }

            if (!confirmUnstructured)
            {
                Bot.Game.Debug("Group {0} is formed", this.Id);
                return true;
            }

            pete = Bot.Game.GetPirateOn(this.FindCenter(false));

            Location[] structure = null;

            if (pete != null)
            {
                try
                {
                    structure = this.GetStructure(pete.Loc);
                }
                catch (InvalidLocationException ex)
                {
                    Bot.Game.Debug("Group {0} is not formed yet", this.Id);
                    return false;
                }


                if (structure.All(loc => Bot.Game.GetPirateOn(loc) != null))
                {
                    Bot.Game.Debug("Group {0} is formed", this.Id);
                    return true;
                }
            }

            Bot.Game.Debug("Group {0} is not formed yet", this.Id);
            this.FormDictionary(structure);
            return false;
        }

        /// <summary>
        /// Forms the group to the optimal shape
        /// </summary>
        private void FormDictionary(Location[] structure = null)
        {
            Location center = this.FindCenter(false);
            
            if(structure == null)
            {
                Bot.Game.Debug("Forming group structure");

                while (true)
                {
                    try
                    {
                        structure = this.GetStructure(center);
                        break;
                    }
                    catch (InvalidLocationException ex)
                    {
                        center = this.AdvancePivot(center);
                       // Bot.Game.Debug("New Center is " + center);
                    }
                }
            }

            bool[] matchedLocations = new bool[structure.Length];
            Dictionary<Pirate, Location> orders = new Dictionary<Pirate, Location>();
            List<Pirate> groupPirates = this.Pirates.ConvertAll(p => Bot.Game.GetMyPirate(p));
            groupPirates.Sort((b, a) => Bot.Game.Distance(a.Loc, center).CompareTo(Bot.Game.Distance(b.Loc, center)));

            //Bot.Game.Debug("Structure is: " + String.Join(",", (object[]) structure));

            //Match a pirate for each location in the structure
            foreach (Pirate pirate in groupPirates)
            {
                Location closestLocation = null;
                int minDistance = Bot.Game.GetCols() + Bot.Game.GetRows();
                int matchIndex = 0;

                for (int i = 0; i < structure.Length; i++)
                {
                    //Skip over taken spots
                    if (matchedLocations[i]) continue;

                    if (Bot.Game.Distance(pirate.Loc, structure[i]) < minDistance)
                    {
                        minDistance = Bot.Game.Distance(pirate.Loc, structure[i]);
                        closestLocation = structure[i];
                        matchIndex = i;
                    }
                }

                matchedLocations[matchIndex] = true;
                orders.Add(pirate, closestLocation);
            }

            //sort the orders so the closest pirates are first to avoid collisions
            this.FormOrders = orders.OrderBy(pair => Bot.Game.Distance(pair.Key.Loc, pair.Value))
                .ToDictionary(pair => pair.Key.Id, pair => pair.Value);

            Bot.Game.Debug("====FORMING TO====");
            foreach (KeyValuePair<int, Location> formOrder in this.FormOrders)
            {
                Bot.Game.Debug(Bot.Game.GetMyPirate(formOrder.Key) + "," + formOrder.Value);
            }
            Bot.Game.Debug("==================");
        }

        /// <summary>
        /// Get the structure for the group
        /// </summary>
        /// <param name="pivot">The center of the group</param>
        /// <returns></returns>
        private Location[] GetStructure(Location pivot)
        {
            int requiredRing = (int) Math.Ceiling((decimal) (this.Pirates.Count - 1)/4);
            //Bot.Game.Debug("Required ring for this group with {0} pirates is #{1} for pivot {2}", this.Pirates.Count,requiredRing, pivot);

            List<Location> rings = new List<Location>();

            for (int ordinal = 0; ordinal <= requiredRing; ordinal++)
            {
                rings.AddRange(Group.GetRing(pivot, ordinal));
            }

            return rings.Take(this.Pirates.Count).ToArray();
        }

        /// <summary>
        /// Moves a location closer to the center of the map
        /// </summary>
        /// <param name="pivot"></param>
        /// <returns></returns>
        private Location AdvancePivot(Location pivot)
        {
            int maxCols = Bot.Game.GetCols();
            int maxRows = Bot.Game.GetRows();

            int addCol = 0, addRow = 0;
            int deltaCol = maxCols - pivot.Col;
            int deltaRow = maxRows - pivot.Row;

            if (deltaCol > pivot.Col)
                addCol++;
            else if (deltaCol < pivot.Col)
                addCol--;

            if (deltaRow > pivot.Row)
                addRow++;
            else if (deltaRow < pivot.Row)
                addRow--;

            pivot = new Location(pivot.Row + addRow, pivot.Col + addCol);
            //Bot.Game.Debug("#### Pivot advanced to " + pivot + " ####");
            return pivot;
        }

        /// <summary>
        /// Get the ring of the specified index relative to the given pivot
        /// </summary>
        /// <param name="pivot">The ring's center</param>
        /// <param name="ringOrdinal">the index of the ring</param>
        /// <returns></returns>
        private static List<Location> GetRing(Location pivot, int ringOrdinal)
        {
            //Bot.Game.Debug("Emitting Locations for ring #{0} from pivot {1}", ringOrdinal, pivot);

            if (ringOrdinal >= 0)
            {
                List<Location> ring = new List<Location>(ringOrdinal*4);
                int a = pivot.Row;
                int b = pivot.Col;

                //this solves the equation I described in the calculations folder
                //Rings_proof.jpg
                for (int x = a - ringOrdinal; x <= a + ringOrdinal; x++)
                {
                    Location y1 =
                        new Location(x,
                            (int)
                                ((2*b +
                                  Math.Sqrt(4*Math.Pow(b, 2) +
                                            4*(Math.Pow(ringOrdinal - Math.Abs(a - x), 2) - Math.Pow(b, 2))))/2));

                    //we do not want to form on a island not ours - it's dangerous
                    if (!Bot.Game.IsPassable(y1))
                        throw new InvalidLocationException("Location is not passable!");

                    ring.Add(y1);

                    Location y2 =
                        new Location(x,
                            (int)
                                ((2*b -
                                  Math.Sqrt(4*Math.Pow(b, 2) +
                                            4*(Math.Pow(ringOrdinal - Math.Abs(a - x), 2) - Math.Pow(b, 2))))/2));

                    if (!Bot.Game.IsPassable(y2))
                        throw new InvalidLocationException("Location is not passable!");

                    //Check for duplicates
                    if (y1.Col != y2.Col || y1.Row != y2.Row)
                        ring.Add(y2);
                }

                //Put the locations of the ring at the debug log
              /*  StringBuilder ringLocationsDesc = new StringBuilder();
                ring.ForEach(x => ringLocationsDesc.Append(x + " "));
                Bot.Game.Debug(ringLocationsDesc.ToString());*/

                return ring;
            }
            throw new InvalidRingException("Ring ordinal must be non-negative");
        }

        /// <summary>
        /// Find the center of the group
        /// </summary>
        /// <param name="enforcePirate">if you ant the function to strictly return a location of a pirate or just the average location</param>
        /// <returns>The center pirate</returns>
        private Location FindCenter(bool enforcePirate)
        {
            List<Pirate> myPirates = this.Pirates.ConvertAll(p => Bot.Game.GetMyPirate(p));

            Location averageLocation = new Location(0, 0);

            foreach (Pirate myPirate in myPirates)
            {
                averageLocation.Row += myPirate.Loc.Row;
                averageLocation.Col += myPirate.Loc.Col;
            }

            averageLocation.Col /= myPirates.Count;
            averageLocation.Row /= myPirates.Count;

            if(enforcePirate)
            {
                int minDistance = Bot.Game.GetCols() + Bot.Game.GetCols();
                Pirate pete = null;

                foreach (Pirate pirate in myPirates)
                {
                    int currDistance = Bot.Game.Distance(averageLocation, pirate.Loc);
                    if (currDistance < minDistance)
                    {
                        minDistance = currDistance;
                        pete = pirate;
                    }
                }

                averageLocation = pete.Loc;
            }

            //Bot.Game.Debug("Center : {0}", averageLocation);
            return averageLocation;
        }

        /// <summary>
        /// counts how many living pirates are in the group
        /// </summary>
        /// <returns>how many living pirates are in the group</returns>
        public int LiveCount()
        {
            return this.Pirates.ConvertAll(p => Bot.Game.GetMyPirate(p)).Count(p => !p.IsLost);
        }

        /// <summary>
        /// Calculate target priorities for this group
        /// </summary>
        public void CalcPriorities()
        {
            //init some lists
            List<ITarget> priorityList = new List<ITarget>();
            List<Score> scores = new List<Score>();

            //Add all targets to the list

            //TODO Fix enemy group targeting
            //priorityList.AddRange(Enemy.Groups);
            priorityList.AddRange(SmartIsland.IslandList);

            //Add a score for each target we got
            foreach (ITarget target in priorityList)
            {
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

            //Bot.Game.Debug("Priorities Count: " + this.Priorities.Count);
        }
        /// <summary>
        /// Adds a pirate to this group, removing it from any other group it was in.
        /// </summary>
        /// <param name="index">The pirate's index</param>
        public void AddPirate(int index)
        {
            foreach (Group g in Commander.Groups)
            {
                g.Pirates.Remove(index);
            }
            this.Pirates.Add(index);
        }

        #region Members

        /// <summary>
        /// Direction of the group to make navigation more precise
        /// </summary>
        public HeadingVector Heading { get; private set; }

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
        public ITarget Target { get; private set; }

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
        public static int GroupCounter { get; private set; }

        /// <summary>
        /// The required location for each pirate in the group to get to attack structure
        /// </summary>
        public Dictionary<int, Location> FormOrders { get; private set; }

        #endregion
    }

}