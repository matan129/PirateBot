using System;
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
            this.Id = GroupCounter++;

            for (; amount > 0; amount--)
            {
                Bot.Game.Debug("Adding pirate at index {0} to this groups pirates", index + amount - 1);
                this.Pirates.Add(index + amount - 1);
            }

            Bot.Game.Debug("\n");
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
            //get Direction of movement
            Direction newDir = Target.GetDirection(this);

            foreach (Pirate pirate in this.Pirates.ConvertAll(p => Bot.Game.GetMyPirate(p)))
            {
                Bot.Game.SetSail(pirate, Bot.Game.GetDirections(pirate, this.Target.GetLocation()).First());
            }


            //update heading
            /* Heading += newDir;

            //sort pirates by the new heading
            Pirates.Sort((p1, p2) => Heading.ComparePirateByDirection(p2, p1));
            Bot.Game.SetSail(Bot.Game.GetMyPirate(Pirates[0]), newDir);

            //if there are others, move them after him
            if (Pirates.Count > 1)
            {
                /* this foreach lambda explained:
                 * we take the list of the group's pirates which is actually the list of the pirates IDs
                 * We skip the first pirate because he is the leader and we have already moved it at line 130
                 * We convert back the collection (which the Skip() method returned) of IDs with the first pirate ID taken out to a normal list<int>
                 * We convert this list of ints to pirates via the ConvertAll() method, with p being a pirate's ID 
                 * Then we iterate over each pirate in this list.
                 * Voilà!
                 * 
                 * P.S unlike the previous version of this method, although my lines are not the shortest,
                 * it is readable. Seriously, try reading it.
                 
                foreach (Pirate pete in this.Pirates.Skip(1).ToList().ConvertAll(p => Bot.Game.GetMyPirate(p)))
                {
                    Direction order = Bot.Game.GetDirections(pete, Bot.Game.GetMyPirate(Pirates[0])).First();
                    Bot.Game.SetSail(pete, order);
                }
            }*/
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

        private List<Location> GetMLocList(Location loc)
        {
            List<Location> list = new List<Location>();

            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                    list.Add(new Location(loc.Row + i, loc.Col + j));

            return list;
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

        #endregion
    }
}