using System;
using System.CodeDom;
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
        /// Indicates if this group is still forming into attack structure
        /// </summary>
        public bool IsForming { get; private set; }

        /// <summary>
        /// The required location for each pirate in the group to get to attack structure
        /// </summary>
        public Dictionary<int, Location> FormOrders { get; private set; }

        #endregion

        #region constructor

        /// <summary>
        /// Creates a new group with set amount of ships (without thinking to much)
        /// </summary>
        /// <param name="index">The stating index of the fist pirate in the group</param>
        /// <param name="amount">How many pirates will be in the group</param>
        public Group(int index, int amount)
        {
            Bot.Game.Debug("*******************************************");
            this.Pirates = new List<int>();
            this.Heading = new HeadingVector(0, 0);
            this.Priorities = new List<Score>();
            this.Role = new GroupRole();
            this.IsForming = true;

            //get id and update counter
            this.Id = GroupCounter++;

            for (; amount > 0; amount--)
            {
                Bot.Game.Debug("Adding pirate at index {0} to this groups pirates", index + amount - 1);
                this.Pirates.Add(index + amount - 1);
            }

            Bot.Game.Debug("\n");
            
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
            else if (!Equals(this.Target, target))
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
                return new Location(y/Pirates.Count, x/Pirates.Count);
            }
            catch (Exception)
            {
                return new Location(0, 0);
            }
        }

        /// <summary>
        /// Move the pirates!
        /// </summary>
        public void Move()
        {
            if (!this.IsFormed())
            {
                foreach (KeyValuePair<int, Location> formOrder in this.FormOrders)
                {
                    Pirate pete = Bot.Game.GetMyPirate(formOrder.Key);
                    if (pete.Loc.Col == formOrder.Value.Col && pete.Loc.Row == formOrder.Value.Row)
                    {
                        Bot.Game.SetSail(pete, Direction.NOTHING);
                        Bot.Game.Debug("Skipping over " + pete);
                        continue;
                    }
                    else
                    {
                        Bot.Game.Debug("Entering forming loop for {0} to go to {1}", pete, formOrder.Value);
                        List<Direction> possibleDirections = Bot.Game.GetDirections(pete, formOrder.Value);
                        Direction dir = Direction.NOTHING;
                        
                        Bot.Game.Debug("Possible Directions: " + String.Join(",",possibleDirections));

                        for (int i = 0; i < possibleDirections.Count; i++)
                        {
                            dir = possibleDirections[i];

                            if(dir == Direction.NOTHING)
                                break;

                            if (Bot.Game.Destination(pete.Loc, dir).IsActuallyPassable())
                                break;
                        }

                        Bot.Game.SetSail(pete, dir);
                    }
                }
            }
            else
            {
                List<Direction> possibleDirections = Bot.Game.GetDirections(this.FindCenter(),this.Target.GetLocation());

                int tryAlternateDirection = Bot.Game.GetTurn() % 2;

                Direction master;
                if (possibleDirections.Count > 1)
                    master = possibleDirections[tryAlternateDirection];
                else
                    master = possibleDirections.First();

                List<Pirate> myPirates = this.Pirates.ConvertAll(p => Bot.Game.GetMyPirate(p));
                Location targetLoc = this.Target.GetLocation();
                myPirates.Sort(
                    (b, a) => Bot.Game.Distance(a.Loc, targetLoc).CompareTo(Bot.Game.Distance(b.Loc, targetLoc)));

                foreach (Pirate myPirate in myPirates)
                {
                    Bot.Game.SetSail(myPirate,master);
                }
            }
        }

        /// <summary>
        /// Return the percent of lost pirates in this group
        /// </summary>
        /// <returns></returns>
        private double CasualtiesPercent()
        {
            return 100* (this.Pirates.Count(p => Bot.Game.GetMyPirate(p).IsLost)/this.Pirates.Count);
        }

        /// <summary>
        /// Checks if the group is formed
        /// </summary>
        /// <returns></returns>
        private bool IsFormed()
        {
            if(this.CasualtiesPercent() > 30)
                return false;

            int deltaCol = 0, deltaRow = 0;
            bool deltaFlag = false;
            int formCounter = 0;
            foreach (KeyValuePair<int, Location> formOrder in this.FormOrders)
            {
                Pirate pete = Bot.Game.GetMyPirate(formOrder.Key);

                if(pete.IsLost)
                    continue;

                if (!deltaFlag)
                {
                    deltaCol = formOrder.Value.Col - pete.Loc.Col;
                    deltaRow = formOrder.Value.Row - pete.Loc.Row;
                    deltaFlag = true;
                }
                if (pete.Loc.Col + deltaCol != formOrder.Value.Col || pete.Loc.Row + deltaRow != formOrder.Value.Row)
                {
                    formCounter++;
                }
            }

            if (formCounter > 0)
            {
                Bot.Game.Debug("Group {0} is not formed yet", this.Id);
                return false;
            }

            Bot.Game.Debug("Group {0} is formed", this.Id);
            return true;
        }

        /// <summary>
        /// Forms the group to the optimal shape
        /// </summary>
        private void FormDictionary()
        {
            Bot.Game.Debug("Forming group structure");
            Location center = this.FindCenter();
            Location[] structure = GetStructure(center);
            bool[] matchedLocations = new bool[structure.Length];
            Dictionary<Pirate,Location> orders = new Dictionary<Pirate, Location>();
            List<Pirate> groupPirates = this.Pirates.ConvertAll(p => Bot.Game.GetMyPirate(p));
            groupPirates.Sort((b,a) => Bot.Game.Distance(a.Loc,center).CompareTo(Bot.Game.Distance(b.Loc,center)));
            
            Bot.Game.Debug("Structure is: " + String.Join(",", (object[]) structure));

            //Match a pirate for each location in the structure
            foreach (Pirate pirate in groupPirates)
            {
                Location closestLocation = null;
                int minDistance = Bot.Game.GetCols() + Bot.Game.GetRows();
                int matchIndex = 0;

                for (int i = 0; i < structure.Length; i++)
                {
                    //Skip over taken spots
                    if (matchedLocations[i] == true) continue;

                    if (Bot.Game.Distance(pirate.Loc, structure[i]) < minDistance)
                    {
                        minDistance = Bot.Game.Distance(pirate.Loc, structure[i]);
                        closestLocation = structure[i];
                        matchIndex = i;
                    }
                }

                matchedLocations[matchIndex] = true;
                orders.Add(pirate,closestLocation);
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
            Bot.Game.Debug("Required ring for this group with {0} pirates is #{1} for pivot {2}",this.Pirates.Count, requiredRing, pivot);

            List<Location> rings = new List<Location>();

            for (int ordinal = 0; ordinal <= requiredRing; ordinal++)
            {
                rings.AddRange(GetRing(pivot,ordinal));
            }

            return rings.Take(this.Pirates.Count).ToArray();
        }

        /// <summary>
        /// Get the ring of the specified index relative to the given pivot
        /// </summary>
        /// <param name="pivot">The ring's center</param>
        /// <param name="ringOrdinal">the index of the ring</param>
        /// <returns></returns>
        private static List<Location> GetRing(Location pivot, int ringOrdinal)
        {
            Bot.Game.Debug("Emitting Locations for ring #{0} from pivot {1}", ringOrdinal, pivot);

            if (ringOrdinal >= 0)
            {
                List<Location> ring = new List<Location>(ringOrdinal * 4);
                int a = pivot.Row;
                int b = pivot.Col;

                //this solves the equation I described in the calculations folder
                for (int x = a - ringOrdinal; x <= a + ringOrdinal; x++)
                {
                    Location y1 =
                        new Location(x,
                            (int)
                                ((2*b +
                                  Math.Sqrt(4*Math.Pow(b, 2) +
                                            4*(Math.Pow(ringOrdinal - Math.Abs(a - x), 2) - Math.Pow(b, 2))))/2));
                    ring.Add(y1);

                    Location y2 =
                        new Location(x,
                            (int)
                                ((2*b -
                                  Math.Sqrt(4*Math.Pow(b, 2) +
                                            4*(Math.Pow(ringOrdinal - Math.Abs(a - x), 2) - Math.Pow(b, 2))))/2));
                    if (y1.Col != y2.Col || y1.Row != y2.Row)
                        ring.Add(y2);
                }

                ring.ForEach(x => Bot.Game.Debug(x + " "));
                return ring;
            }
            else
            {
                throw new InvalidRingException("Ring ordinal must be non-negative");
            }
        }

        /// <summary>
        /// Find the center pirate in the group, which is the pirate closest to the average locationof the group
        /// </summary>
        /// <returns>The center pirate</returns>
        private Location FindCenter()
        {
            Pirate center = null;
            decimal minDistance = Bot.Game.GetCols() + Bot.Game.GetRows();
            List<Pirate> myPirates = this.Pirates.ConvertAll(p => Bot.Game.GetMyPirate(p));

            Location averageLocation = new Location(0,0);

            foreach (Pirate myPirate in myPirates)
            {
                averageLocation.Row += myPirate.Loc.Row;
                averageLocation.Col += myPirate.Loc.Col;
            }

            averageLocation.Col /= myPirates.Count;
            averageLocation.Row /= myPirates.Count;

            Bot.Game.Debug("Center : {0}", averageLocation);
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

            Bot.Game.Debug("Priorities Count: " + this.Priorities.Count);
        }

        /// <summary>
        /// Combines a location and a direction to the location the direction leads to
        /// </summary>
        /// <param name="cur"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static Location GetFutureLocation(Location cur, Direction dir)
        {
            switch (dir)
            {
                case Direction.NORTH:
                    cur.Row--;
                    break;

                case Direction.SOUTH:
                    cur.Row++;
                    break;

                case Direction.EAST:
                    cur.Col++;
                    break;

                case Direction.WEST:
                    cur.Col--;
                    break;
            }

            return cur;
        }
    }
}