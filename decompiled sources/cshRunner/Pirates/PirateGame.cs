// Decompiled with JetBrains decompiler
// Type: Pirates.PirateGame
// Assembly: PiratesCsh, Version=1.0.5548.25549, Culture=neutral, PublicKeyToken=null
// MVID: 932FE985-6866-4B4F-91C1-D0B41B499FF8
// Assembly location: C:\Users\Matan\Documents\Repositories\PirateBot\starter_kit\lib\cshRunner.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pirates
{
    public class PirateGame : IPirateGame
    {
        private List<int> CloakCooldowns;
        private List<int> LastTurnPoints;
        private List<int> Scores;

        public PirateGame(string gameData)
        {
            Cols = 0;
            Rows = 0;
            Map = new int[0, 0];
            AllIslands = new List<Island>();
            AllPirates = new List<Pirate>();
            Scores = new List<int>();
            CloakCooldowns = new List<int>();
            LastTurnPoints = new List<int>();
            TurnTime = 0;
            LoadTime = 0;
            TurnStartTime = DateTime.Now;
            NumPlayers = 0;
            Vision = null;
            VisionOffsets2 = null;
            ViewRadius2 = 0;
            AttackRadius2 = 0;
            SpawnRadius2 = 0;
            GhostCooldown = 0;
            SpawnTurns = 0;
            MaxTurns = 0;
            Turn = 0;
            Cyclic = true;
            Orders = new Dictionary<Location, Direction>();
            Loc2Pirate = new Dictionary<Location, Pirate>();
            ME = 0;
            ENEMY = 1;
            NEUTRAL = -1;
            using (var stringReader = new StringReader(gameData))
            {
                string str1;
                while ((str1 = stringReader.ReadLine()) != null)
                {
                    var str2 = str1.Trim().ToLower();
                    if (str2.Length != 0)
                    {
                        var strArray = str2.Split();
                        var str3 = strArray[0];
                        if (strArray.Length > 1)
                        {
                            var Seed = int.Parse(strArray[1]);
                            switch (str3)
                            {
                                case "cols":
                                    Cols = Seed;
                                    continue;
                                case "rows":
                                    Rows = Seed;
                                    continue;
                                case "player_seed":
                                    Random = new Random(Seed);
                                    continue;
                                case "cyclic":
                                    Cyclic = Seed != 0;
                                    continue;
                                case "spawnturns":
                                    SpawnTurns = Seed;
                                    continue;
                                case "turntime":
                                    TurnTime = Seed;
                                    continue;
                                case "loadtime":
                                    LoadTime = Seed;
                                    continue;
                                case "viewradius2":
                                    ViewRadius2 = Seed;
                                    continue;
                                case "attackradius2":
                                    AttackRadius2 = Seed;
                                    continue;
                                case "spawnradius2":
                                    SpawnRadius2 = Seed;
                                    continue;
                                case "ghost_cooldown":
                                    GhostCooldown = Seed;
                                    continue;
                                case "max_turns":
                                    MaxTurns = Seed;
                                    continue;
                                case "start_turn":
                                    Turn = Seed;
                                    continue;
                                case "numplayers":
                                    NumPlayers = Seed;
                                    continue;
                                default:
                                    continue;
                            }
                        }
                    }
                }
            }
            Map = new int[Rows, Cols];
            for (var index1 = 0; index1 < Rows; ++index1)
            {
                for (var index2 = 0; index2 < Cols; ++index2)
                    Map[index1, index2] = -2;
            }

            Scores = Enumerable.Repeat(0, NumPlayers).ToList();
            CloakCooldowns = Enumerable.Repeat(0, NumPlayers).ToList();
            LastTurnPoints = Enumerable.Repeat(0, NumPlayers).ToList();
        }

        private int Rows { get; set; }
        private int Cols { get; set; }
        private int[,] Map { get; set; }
        private List<Island> AllIslands { get; set; }
        private List<Pirate> AllPirates { get; set; }
        private int TurnTime { get; set; }
        private int LoadTime { get; set; }
        private DateTime TurnStartTime { get; set; }
        private int NumPlayers { get; set; }
        
        //
        private bool[,] Vision { get; set; }
        private List<Location> VisionOffsets2 { get; set; }
        private int ViewRadius2 { get; set; }
        //

        private int AttackRadius2 { get; set; }
        private int SpawnRadius2 { get; set; }
        private int GhostCooldown { get; set; }
        private int SpawnTurns { get; set; }
        private int MaxTurns { get; set; }
        private int Turn { get; set; }
        private bool Cyclic { get; set; }
        private IDictionary<Location, Direction> Orders { get; set; }
        private IDictionary<Location, Pirate> Loc2Pirate { get; set; }
        private List<Pirate> SortedMyPirates { get; set; }
        private List<Pirate> SortedEnemyPirates { get; set; }
        private Random Random { get; set; }
        private int ME { get; set; }
        private int ENEMY { get; set; }
        private int NEUTRAL { get; set; }

        public void SetSail(Pirate pirate, Direction direction)
        {
            if (direction == Direction.NOTHING)
                return;
            Orders[pirate.Loc] = direction;
        }

        public void Cloak(Pirate pirate)
        {
            Orders[pirate.Loc] = Direction.CLOAK;
        }

        public void Reveal(Pirate pirate)
        {
            Orders[pirate.Loc] = Direction.REVEAL;
        }

        public int TimeRemaining()
        {
            return TurnTime - (int) (DateTime.Now - TurnStartTime).TotalSeconds;
        }

        public int GetTurn()
        {
            return Turn;
        }

        public int GetMaxTurns()
        {
            return MaxTurns;
        }

        public int GetSpawnTurns()
        {
            return SpawnTurns;
        }

        public int GetMaxCloakCooldown()
        {
            return GhostCooldown;
        }

        public int GetAttackRadius()
        {
            return AttackRadius2;
        }

        public void Debug(string message)
        {
            Console.Out.WriteLine("m {0}", Base64Encode(message));
            Console.Out.Flush();
        }

        public void Debug(string format, params object[] messages)
        {
            Debug(string.Format(format, messages));
        }

        public List<Island> Islands()
        {
            return new List<Island>(AllIslands);
        }

        public List<Island> MyIslands()
        {
            return AllIslands.Where(island => island.Owner == ME).ToList();
        }

        public List<Island> EnemyIslands()
        {
            return AllIslands.Where(island =>
            {
                if (island.Owner != ME)
                    return island.Owner != -1;
                return false;
            }).ToList();
        }

        public List<Island> NeutralIslands()
        {
            return AllIslands.Where(island => island.Owner == -1).ToList();
        }

        public List<Island> NotMyIslands()
        {
            return AllIslands.Where(island => island.Owner != ME).ToList();
        }

        public Island GetIsland(int id)
        {
            return AllIslands.FirstOrDefault(island => island.Id == id);
        }

        public List<Pirate> AllMyPirates()
        {
            return new List<Pirate>(SortedMyPirates);
        }

        public List<Pirate> MyPirates()
        {
            return AllMyPirates().Where(pirate => !pirate.IsLost).ToList();
        }

        public List<Pirate> MyLostPirates()
        {
            return AllMyPirates().Where(pirate => pirate.IsLost).ToList();
        }

        public List<Pirate> AllEnemyPirates()
        {
            return new List<Pirate>(SortedEnemyPirates);
        }

        public List<Pirate> EnemyPirates()
        {
            return SortedEnemyPirates.Where(pirate =>
            {
                if (!pirate.IsLost)
                    return !pirate.IsCloaked;
                return false;
            }).ToList();
        }

        public List<Pirate> EnemyLostPirates()
        {
            return SortedEnemyPirates.Where(pirate => pirate.IsLost).ToList();
        }

        public Pirate GetMyPirate(int id)
        {
            if (id < 0 || id >= SortedMyPirates.Count)
                return null;
            return SortedMyPirates[id];
        }

        public Pirate GetEnemyPirate(int id)
        {
            if (id < 0 || id >= SortedEnemyPirates.Count)
                return null;
            return SortedEnemyPirates[id];
        }

        public Pirate GetPirateOn(Island island)
        {
            return GetPirateFromLocation(island.Loc);
        }

        public Pirate GetPirateOn(Location location)
        {
            return GetPirateFromLocation(location);
        }

        public bool isCapturing(Pirate pirate)
        {
            return AllIslands.Where(island => island.Owner != pirate.Owner).Any(island => island.Loc.Equals(pirate.Loc));
        }

        public bool IsPassable(Location location)
        {
            if (location.Row >= 0 && location.Col >= 0 && (Map[location.Row, location.Col] != -4 && location.Row < Rows))
                return location.Col < Cols;
            return false;
        }

        public Location Destination(Pirate a, Direction d)
        {
            return Destination(a.Loc, d);
        }

        public Location Destination(Location loc, Direction d)
        {
            var location = InnerConsts.AIM[d];
            return new Location(loc.Row + location.Row%Rows, loc.Col + location.Col%Cols);
        }

        public int Distance(Location loc1, Location loc2)
        {
            var val1_1 = Math.Abs(loc1.Row - loc2.Row);
            var val1_2 = Math.Abs(loc1.Col - loc2.Col);
            if (!Cyclic)
                return val1_1 + val1_2;
            return Math.Min(val1_2, Cols - val1_2) + Math.Min(val1_1, Rows - val1_1);
        }

        public int Distance(Pirate pirate1, Pirate pirate2)
        {
            return Distance(pirate1.Loc, pirate2.Loc);
        }

        public int Distance(Pirate pirate, Island island)
        {
            return Distance(pirate.Loc, island.Loc);
        }

        public int Distance(Island island, Pirate pirate)
        {
            return Distance(pirate.Loc, island.Loc);
        }

        public int Distance(Island island1, Island island2)
        {
            return Distance(island1.Loc, island2.Loc);
        }

        public List<Direction> GetDirections(Location loc1, Location loc2)
        {
            var num1 = Rows/2;
            var num2 = Cols/2;
            var list = new List<Direction>();
            if (loc1.Equals(loc2))
            {
                list.Add(Direction.NOTHING);
            }
            else
            {
                if (loc1.Row < loc2.Row)
                {
                    if (loc2.Row - loc1.Row >= num1 && Cyclic)
                        list.Add(Direction.NORTH);
                    else if (loc2.Row - loc1.Row <= num1 || !Cyclic)
                        list.Add(Direction.SOUTH);
                }
                else if (loc1.Row > loc2.Row)
                {
                    if (loc1.Row - loc2.Row >= num1 && Cyclic)
                        list.Add(Direction.SOUTH);
                    else if (loc1.Row - loc2.Row <= num1 || !Cyclic)
                        list.Add(Direction.NORTH);
                }
                if (loc1.Col < loc2.Col)
                {
                    if (loc2.Col - loc1.Col >= num2 && Cyclic)
                        list.Add(Direction.WEST);
                    else if (loc2.Col - loc1.Col <= num2 || !Cyclic)
                        list.Add(Direction.EAST);
                }
                else if (loc1.Col > loc2.Col)
                {
                    if (loc1.Col - loc2.Col >= num2 && Cyclic)
                        list.Add(Direction.EAST);
                    else if (loc1.Col - loc2.Col <= num2 || !Cyclic)
                        list.Add(Direction.WEST);
                }
            }
            return list;
        }

        public List<Direction> GetDirections(Pirate pirate, Island island)
        {
            return GetDirections(pirate.Loc, island.Loc);
        }

        public List<Direction> GetDirections(Pirate pirate1, Pirate pirate2)
        {
            return GetDirections(pirate1.Loc, pirate2.Loc);
        }

        public List<Direction> GetDirections(Pirate pirate, Location location)
        {
            return GetDirections(pirate.Loc, location);
        }

        public List<int> GetScores()
        {
            return Scores;
        }

        public int GetMyScore()
        {
            return Scores[0];
        }

        public int GetEnemyScore()
        {
            return Scores[1];
        }

        public List<int> GetLastTurnPoints()
        {
            return LastTurnPoints;
        }

        public Pirate GetMyCloaked()
        {
            return MyPirates().FirstOrDefault(pir => pir.IsCloaked);
        }

        public bool CanCloak()
        {
            return CloakCooldowns[0] == 0;
        }

        public List<int> GetCloakCooldowns()
        {
            return CloakCooldowns;
        }

        public bool InRange(Pirate a1, Pirate a2)
        {
            return InRange(a1.Loc, a2.Loc);
        }

        public bool InRange(Location l1, Pirate a2)
        {
            return InRange(l1, a2.Loc);
        }

        public bool InRange(Pirate a1, Location loc2)
        {
            return InRange(a1.Loc, loc2);
        }

        public bool InRange(Location loc1, Location loc2)
        {
            var num = Math.Pow(loc1.Row - loc2.Row, 2.0) + Math.Pow(loc1.Col - loc2.Col, 2.0);
            return 0.0 < num && num <= AttackRadius2;
        }

        public bool IsOccupied(Location loc)
        {
            return AllPirates.Where(pirate => !pirate.IsLost).Any(pirate => pirate.Loc.Equals(loc));
        }

        public int GetRows()
        {
            return Rows;
        }

        public int GetCols()
        {
            return Cols;
        }

        private Location parseLocationFrom(string row, string col)
        {
            return new Location(int.Parse(row), int.Parse(col));
        }

        private void parsePirateData(string[] tokens)
        {
            var Id = int.Parse(tokens[1]);
            var location = parseLocationFrom(tokens[2], tokens[3]);
            var Owner = int.Parse(tokens[4]);
            var InitialLocation = parseLocationFrom(tokens[5], tokens[6]);
            Pirate pirate;
            if (tokens[0] == "a")
            {
                pirate = new Pirate(Id, Owner, location, InitialLocation);
                var flag = int.Parse(tokens[7]) != 0;
                pirate.IsCloaked = flag;
                Loc2Pirate.Add(location, pirate);
            }
            else
            {
                var num = int.Parse(tokens[7]);
                pirate = new Pirate(Id, Owner, location, InitialLocation);
                pirate.TurnsToRevive = num;
                pirate.IsLost = true;
            }
            AllPirates.Add(pirate);
        }

        private void parseIslandData(string[] tokens)
        {
            var Id = int.Parse(tokens[1]);
            var Loc = parseLocationFrom(tokens[2], tokens[3]);
            int result1;
            if (!int.TryParse(tokens[4], out result1))
                result1 = -1;
            int result2;
            if (!int.TryParse(tokens[5], out result2))
                result2 = -1;
            var TurnsBeingCaptured = int.Parse(tokens[6]);
            var CaptureTurns = int.Parse(tokens[7]);
            var num = int.Parse(tokens[8]);
            AllIslands.Add(new Island(Id, Loc, result1, result2, TurnsBeingCaptured, CaptureTurns, num));
            Map[Loc.Row, Loc.Col] = -1;
        }

        private void parseMapUpdateData(string mapData)
        {
            using (var stringReader = new StringReader(mapData))
            {
                string str1;
                while ((str1 = stringReader.ReadLine()) != null)
                {
                    var str2 = str1.Trim().ToLower();
                    if (str2.Length != 0)
                    {
                        var tokens = str2.Split();
                        if (tokens[0] == "g")
                        {
                            var list = Enumerable.Repeat(0, tokens.Length - 2).ToList();
                            for (var index = 0; index < list.Count; ++index)
                                list[index] = int.Parse(tokens[index + 2]);
                            if (tokens[1] == "s")
                                Scores = list;
                            else if (tokens[1] == "c")
                                CloakCooldowns = list;
                            else if (tokens[1] == "p")
                                LastTurnPoints = list;
                        }
                        if (tokens[0] == "w")
                        {
                            var location = parseLocationFrom(tokens[1], tokens[2]);
                            Map[location.Row, location.Col] = -4;
                        }
                        else if (tokens.Length >= 5)
                        {
                            if (tokens[0] == "f")
                                parseIslandData(tokens);
                            else if (tokens[0] == "a" || tokens[0] == "d")
                                parsePirateData(tokens);
                        }
                    }
                }
            }
        }

        public void Update(string mapData)
        {
            TurnStartTime = DateTime.Now;
            Vision = null;
            AllPirates = new List<Pirate>();
            AllIslands = new List<Island>();
            Loc2Pirate = new Dictionary<Location, Pirate>();
            SortedMyPirates = new List<Pirate>();
            SortedEnemyPirates = new List<Pirate>();
            ++Turn;
            parseMapUpdateData(mapData);
            SortedMyPirates = AllPirates.Where(pirate => pirate.Owner == 0).OrderBy(pirate => pirate.Id).ToList();
            SortedEnemyPirates = AllPirates.Where(pirate => pirate.Owner != 0).OrderBy(pirate => pirate.Id).ToList();
            AllIslands = AllIslands.OrderBy(island => island.Id).ToList();
        }

        public void FinishTurn()
        {
            foreach (var keyValuePair in Orders)
                Console.Out.WriteLine("o {0} {1} {2}", keyValuePair.Key.Row, keyValuePair.Key.Col,
                    (char) keyValuePair.Value);
            Orders.Clear();
            Console.Out.WriteLine("go");
            Console.Out.Flush();
        }

        public void CancelCollisions()
        {
            var num1 = 0;
            var num2 = AllMyPirates().Count();
            for (var index1 = 0; index1 < num2; ++index1)
            {
                var list1 = ValidateCollisions();
                if (list1.Count > 0)
                {
                    foreach (var tuple in list1)
                    {
                        var location = tuple.Item1;
                        var list2 = tuple.Item2;
                        for (var index2 = 1; index2 < list2.Count; ++index2)
                        {
                            ++num1;
                            CancelOrder(list2[index2]);
                        }
                    }
                }
                else
                    break;
            }
            if (num1 <= 0)
                return;
            Debug("WARNING: auto-canceling collisions for {0} pirates", (object) num1);
        }

        public Direction StepTowards(Pirate pirate, Location target)
        {
            var directions = GetDirections(pirate.Loc, target);
            if (directions.Count <= 0)
                return Direction.NOTHING;
            var index = Random.Next(0, directions.Count);
            SetSail(pirate, directions[index]);
            return directions[index];
        }

        public void CancelOrder(Pirate pirate)
        {
            CancelOrder(pirate.Loc);
        }

        public void CancelOrder(Location location)
        {
            Orders.Remove(location);
        }

        private static string Base64Encode(string plainText)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }

        private List<Tuple<Location, List<Location>>> ValidateCollisions()
        {
            var dictionary = new Dictionary<Location, List<Location>>();
            foreach (
                var tuple in
                    MyPirates()
                        .Select(pir => pir.Loc)
                        .ToList()
                        .Select(loc => Tuple.Create(loc, Orders.ContainsKey(loc)))
                        .ToList()
                        .OrderBy(tup => tup.Item2)
                        .ToList())
            {
                var loc = tuple.Item1;
                var key = tuple.Item2 ? Destination(loc, Orders[loc]) : loc;
                if (!dictionary.ContainsKey(key))
                    dictionary[key] = new List<Location>();
                dictionary[key].Add(loc);
            }
            var list1 = new List<Tuple<Location, List<Location>>>();
            foreach (var keyValuePair in dictionary)
            {
                var key = keyValuePair.Key;
                var list2 = keyValuePair.Value;
                if (list2.Count > 1)
                    list1.Add(Tuple.Create(key, list2));
            }
            return list1;
        }

        public List<Island> MyIslandsInDanger()
        {
            return AllIslands.Where(island =>
            {
                if (island.Owner == ME)
                    return island.TeamCapturing != -1;
                return false;
            }).ToList();
        }

        public List<Island> IslandsBeingCapturedByMe()
        {
            return AllIslands.Where(island => island.TeamCapturing == ME).ToList();
        }

        private List<Location> MyPiratesLocations()
        {
            return SortedMyPirates.Select(pirate => pirate.Loc).ToList();
        }

        public Pirate GetPirateFromLocation(Location location)
        {
            if (Loc2Pirate.ContainsKey(location))
                return Loc2Pirate[location];
            return null;
        }

        public Dictionary<Pirate, List<Direction>> InDanger(Pirate anAnt, List<Pirate> possibleAttackers = null)
        {
            if (possibleAttackers == null)
                possibleAttackers = EnemyPirates();
            var dictionary = new Dictionary<Pirate, List<Direction>>();
            foreach (var a in possibleAttackers)
            {
                var list = new List<Direction>();
                foreach (var d in list)
                {
                    var loc2 = Destination(a, d);
                    if (InRange(anAnt, loc2))
                        list.Add(d);
                }
                if (list.Count > 0)
                    dictionary[a] = list;
            }
            return dictionary;
        }

        public bool IsEmpty(Location otherLocation)
        {
            return Orders.Select(kv => Destination(kv.Key, kv.Value)).All(location => location != otherLocation);
        }

        public bool isVisible(Location loc)
        {
            if (Vision == null)
            {
                if (VisionOffsets2 == null)
                {
                    VisionOffsets2 = new List<Location>();
                    var num = (int) Math.Sqrt(ViewRadius2);
                    for (var index1 = -num; index1 <= num; ++index1)
                    {
                        for (var index2 = -num; index2 <= num; ++index2)
                        {
                            if ((int) (Math.Pow(index1, 2.0) + Math.Pow(index2, 2.0)) <= ViewRadius2)
                                VisionOffsets2.Add(new Location(index1%Rows - Rows, index2%Cols - Cols));
                        }
                    }
                }
                Vision = new bool[Rows, Cols];
                for (var index1 = 0; index1 < Rows; ++index1)
                {
                    for (var index2 = 0; index2 < Cols; ++index2)
                        Vision[index1, index2] = false;
                }
                foreach (var pirate in MyPirates())
                {
                    var loc1 = pirate.Loc;
                    foreach (var location in VisionOffsets2)
                        Vision[loc1.Row + location.Row, loc1.Col + location.Col] = true;
                }
            }
            return Vision[loc.Row, loc.Col];
        }
    }
}