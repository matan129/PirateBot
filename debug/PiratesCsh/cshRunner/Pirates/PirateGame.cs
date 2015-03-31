// Decompiled with JetBrains decompiler
// Type: Pirates.PirateGame
// Assembly: PiratesCsh, Version=1.0.5561.34964, Culture=neutral, PublicKeyToken=null
// MVID: F3CB7840-9EB5-484E-B3E0-5A16B16AF427
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
    private List<int> Scores;
    private List<int> CloakCooldowns;
    private List<int> LastTurnPoints;

    private int Rows { get; set; }

    private int Cols { get; set; }

    private int[,] Map { get; set; }

    private List<Island> AllIslands { get; set; }

    private List<Pirate> AllPirates { get; set; }

    private int TurnTime { get; set; }

    private int LoadTime { get; set; }

    private DateTime TurnStartTime { get; set; }

    private int NumPlayers { get; set; }

    private bool[,] Vision { get; set; }

    private List<Location> VisionOffsets2 { get; set; }

    private int ViewRadius2 { get; set; }

    private int AttackRadius2 { get; set; }

    private int SpawnRadius2 { get; set; }

    private int GhostCooldown { get; set; }

    private int SpawnTurns { get; set; }

    private int MaxPoints { get; set; }

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

    public PirateGame(string gameData)
    {
      this.Cols = 0;
      this.Rows = 0;
      this.Map = new int[0, 0];
      this.AllIslands = new List<Island>();
      this.AllPirates = new List<Pirate>();
      this.Scores = new List<int>();
      this.CloakCooldowns = new List<int>();
      this.LastTurnPoints = new List<int>();
      this.TurnTime = 0;
      this.LoadTime = 0;
      this.TurnStartTime = DateTime.Now;
      this.NumPlayers = 0;
      this.Vision = (bool[,]) null;
      this.VisionOffsets2 = (List<Location>) null;
      this.ViewRadius2 = 0;
      this.AttackRadius2 = 0;
      this.SpawnRadius2 = 0;
      this.GhostCooldown = 0;
      this.SpawnTurns = 0;
      this.MaxPoints = 0;
      this.MaxTurns = 0;
      this.Turn = 0;
      this.Cyclic = true;
      this.Orders = (IDictionary<Location, Direction>) new Dictionary<Location, Direction>();
      this.Loc2Pirate = (IDictionary<Location, Pirate>) new Dictionary<Location, Pirate>();
      this.ME = 0;
      this.ENEMY = 1;
      this.NEUTRAL = -1;
      using (StringReader stringReader = new StringReader(gameData))
      {
        string str1;
        while ((str1 = stringReader.ReadLine()) != null)
        {
          string str2 = str1.Trim().ToLower();
          if (str2.Length != 0)
          {
            string[] strArray = str2.Split();
            string str3 = strArray[0];
            if (strArray.Length > 1)
            {
              int Seed = int.Parse(strArray[1]);
              switch (str3)
              {
                case "cols":
                  this.Cols = Seed;
                  continue;
                case "rows":
                  this.Rows = Seed;
                  continue;
                case "player_seed":
                  this.Random = new Random(Seed);
                  continue;
                case "cyclic":
                  this.Cyclic = Seed != 0;
                  continue;
                case "spawnturns":
                  this.SpawnTurns = Seed;
                  continue;
                case "turntime":
                  this.TurnTime = Seed;
                  continue;
                case "loadtime":
                  this.LoadTime = Seed;
                  continue;
                case "viewradius2":
                  this.ViewRadius2 = Seed;
                  continue;
                case "attackradius2":
                  this.AttackRadius2 = Seed;
                  continue;
                case "spawnradius2":
                  this.SpawnRadius2 = Seed;
                  continue;
                case "ghost_cooldown":
                  this.GhostCooldown = Seed;
                  continue;
                case "maxpoints":
                  this.MaxPoints = Seed;
                  continue;
                case "max_turns":
                  this.MaxTurns = Seed;
                  continue;
                case "start_turn":
                  this.Turn = Seed;
                  continue;
                case "numplayers":
                  this.NumPlayers = Seed;
                  continue;
                default:
                  continue;
              }
            }
          }
        }
      }
      this.Map = new int[this.Rows, this.Cols];
      for (int index1 = 0; index1 < this.Rows; ++index1)
      {
        for (int index2 = 0; index2 < this.Cols; ++index2)
          this.Map[index1, index2] = -2;
      }
      this.Scores = Enumerable.ToList<int>(Enumerable.Repeat<int>(0, this.NumPlayers));
      this.CloakCooldowns = Enumerable.ToList<int>(Enumerable.Repeat<int>(0, this.NumPlayers));
      this.LastTurnPoints = Enumerable.ToList<int>(Enumerable.Repeat<int>(0, this.NumPlayers));
    }

    private Location parseLocationFrom(string row, string col)
    {
      return new Location(int.Parse(row), int.Parse(col));
    }

    private void parsePirateData(string[] tokens)
    {
      int Id = int.Parse(tokens[1]);
      Location location = this.parseLocationFrom(tokens[2], tokens[3]);
      int Owner = int.Parse(tokens[4]);
      Location InitialLocation = this.parseLocationFrom(tokens[5], tokens[6]);
      Pirate pirate;
      if (tokens[0] == "a")
      {
        pirate = new Pirate(Id, Owner, location, InitialLocation);
        bool flag = int.Parse(tokens[7]) != 0;
        pirate.IsCloaked = flag;
        this.Loc2Pirate.Add(location, pirate);
      }
      else
      {
        int num = int.Parse(tokens[7]);
        pirate = new Pirate(Id, Owner, location, InitialLocation);
        pirate.TurnsToRevive = num;
        pirate.IsLost = true;
      }
      this.AllPirates.Add(pirate);
    }

    private void parseIslandData(string[] tokens)
    {
      int Id = int.Parse(tokens[1]);
      Location Loc = this.parseLocationFrom(tokens[2], tokens[3]);
      int result1;
      if (!int.TryParse(tokens[4], out result1))
        result1 = -1;
      int result2;
      if (!int.TryParse(tokens[5], out result2))
        result2 = -1;
      int TurnsBeingCaptured = int.Parse(tokens[6]);
      int CaptureTurns = int.Parse(tokens[7]);
      int num = int.Parse(tokens[8]);
      this.AllIslands.Add(new Island(Id, Loc, result1, result2, TurnsBeingCaptured, CaptureTurns, num));
      this.Map[Loc.Row, Loc.Col] = -1;
    }

    private void parseMapUpdateData(string mapData)
    {
      using (StringReader stringReader = new StringReader(mapData))
      {
        string str1;
        while ((str1 = stringReader.ReadLine()) != null)
        {
          string str2 = str1.Trim().ToLower();
          if (str2.Length != 0)
          {
            string[] tokens = str2.Split();
            if (tokens[0] == "g")
            {
              List<int> list = Enumerable.ToList<int>(Enumerable.Repeat<int>(0, tokens.Length - 2));
              for (int index = 0; index < list.Count; ++index)
                list[index] = int.Parse(tokens[index + 2]);
              if (tokens[1] == "s")
                this.Scores = list;
              else if (tokens[1] == "c")
                this.CloakCooldowns = list;
              else if (tokens[1] == "p")
                this.LastTurnPoints = list;
            }
            if (tokens[0] == "w")
            {
              Location location = this.parseLocationFrom(tokens[1], tokens[2]);
              this.Map[location.Row, location.Col] = -4;
            }
            else if (tokens.Length >= 5)
            {
              if (tokens[0] == "f")
                this.parseIslandData(tokens);
              else if (tokens[0] == "a" || tokens[0] == "d")
                this.parsePirateData(tokens);
            }
          }
        }
      }
    }

    public void Update(string mapData)
    {
      this.TurnStartTime = DateTime.Now;
      this.Vision = (bool[,]) null;
      this.AllPirates = new List<Pirate>();
      this.AllIslands = new List<Island>();
      this.Loc2Pirate = (IDictionary<Location, Pirate>) new Dictionary<Location, Pirate>();
      this.SortedMyPirates = new List<Pirate>();
      this.SortedEnemyPirates = new List<Pirate>();
      ++this.Turn;
      this.parseMapUpdateData(mapData);
      this.SortedMyPirates = Enumerable.ToList<Pirate>((IEnumerable<Pirate>) Enumerable.OrderBy<Pirate, int>(Enumerable.Where<Pirate>((IEnumerable<Pirate>) this.AllPirates, (Func<Pirate, bool>) (pirate => pirate.Owner == 0)), (Func<Pirate, int>) (pirate => pirate.Id)));
      this.SortedEnemyPirates = Enumerable.ToList<Pirate>((IEnumerable<Pirate>) Enumerable.OrderBy<Pirate, int>(Enumerable.Where<Pirate>((IEnumerable<Pirate>) this.AllPirates, (Func<Pirate, bool>) (pirate => pirate.Owner != 0)), (Func<Pirate, int>) (pirate => pirate.Id)));
      this.AllIslands = Enumerable.ToList<Island>((IEnumerable<Island>) Enumerable.OrderBy<Island, int>((IEnumerable<Island>) this.AllIslands, (Func<Island, int>) (island => island.Id)));
    }

    public void FinishTurn()
    {
      foreach (KeyValuePair<Location, Direction> keyValuePair in (IEnumerable<KeyValuePair<Location, Direction>>) this.Orders)
        Console.Out.WriteLine(string.Format("o {0} {1} {2}", (object) keyValuePair.Key.Row, (object) keyValuePair.Key.Col, (object) (char) keyValuePair.Value));
      this.Orders.Clear();
      Console.Out.WriteLine("go");
      Console.Out.Flush();
    }

    public void SetSail(Pirate pirate, Direction direction)
    {
      if (direction == Direction.NOTHING)
        return;
      this.Orders[pirate.Loc] = direction;
    }

    public void Cloak(Pirate pirate)
    {
      this.Orders[pirate.Loc] = Direction.CLOAK;
    }

    public void Reveal(Pirate pirate)
    {
      this.Orders[pirate.Loc] = Direction.REVEAL;
    }

    public void CancelCollisions()
    {
      int num1 = 0;
      int num2 = Enumerable.Count<Pirate>((IEnumerable<Pirate>) this.AllMyPirates());
      for (int index1 = 0; index1 < num2; ++index1)
      {
        List<Tuple<Location, List<Location>>> list1 = this.ValidateCollisions();
        if (list1.Count > 0)
        {
          foreach (Tuple<Location, List<Location>> tuple in list1)
          {
            Location location = tuple.Item1;
            List<Location> list2 = tuple.Item2;
            for (int index2 = 1; index2 < list2.Count; ++index2)
            {
              ++num1;
              this.CancelOrder(list2[index2]);
            }
          }
        }
        else
          break;
      }
      if (num1 <= 0)
        return;
      this.Debug("WARNING: auto-canceling collisions for {0} pirates", (object) num1);
    }

    public int TimeRemaining()
    {
      return this.TurnTime - (int) (DateTime.Now - this.TurnStartTime).TotalSeconds;
    }

    public Direction StepTowards(Pirate pirate, Location target)
    {
      List<Direction> directions = this.GetDirections(pirate.Loc, target);
      if (directions.Count <= 0)
        return Direction.NOTHING;
      int index = this.Random.Next(0, directions.Count);
      this.SetSail(pirate, directions[index]);
      return directions[index];
    }

    public void CancelOrder(Pirate pirate)
    {
      this.CancelOrder(pirate.Loc);
    }

    public void CancelOrder(Location location)
    {
      this.Orders.Remove(location);
    }

    public int GetTurn()
    {
      return this.Turn;
    }

    public int GetMaxTurns()
    {
      return this.MaxTurns;
    }

    public int GetSpawnTurns()
    {
      return this.SpawnTurns;
    }

    public int GetMaxCloakCooldown()
    {
      return this.GhostCooldown;
    }

    public int GetAttackRadius()
    {
      return this.AttackRadius2;
    }

    public void Debug(string message)
    {
      Console.Out.WriteLine(string.Format("m {0}", (object) PirateGame.Base64Encode(message)));
      Console.Out.Flush();
    }

    public void Debug(string format, params object[] messages)
    {
      this.Debug(string.Format(format, messages));
    }

    private static string Base64Encode(string plainText)
    {
      return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
    }

    private List<Tuple<Location, List<Location>>> ValidateCollisions()
    {
      Dictionary<Location, List<Location>> dictionary = new Dictionary<Location, List<Location>>();
      foreach (Tuple<Location, bool> tuple in Enumerable.ToList<Tuple<Location, bool>>((IEnumerable<Tuple<Location, bool>>) Enumerable.OrderBy<Tuple<Location, bool>, bool>((IEnumerable<Tuple<Location, bool>>) Enumerable.ToList<Tuple<Location, bool>>(Enumerable.Select<Location, Tuple<Location, bool>>((IEnumerable<Location>) Enumerable.ToList<Location>(Enumerable.Select<Pirate, Location>((IEnumerable<Pirate>) this.MyPirates(), (Func<Pirate, Location>) (pir => pir.Loc))), (Func<Location, Tuple<Location, bool>>) (loc => Tuple.Create<Location, bool>(loc, this.Orders.ContainsKey(loc))))), (Func<Tuple<Location, bool>, bool>) (tup => tup.Item2))))
      {
        Location loc = tuple.Item1;
        Location key = tuple.Item2 ? this.Destination(loc, this.Orders[loc]) : loc;
        if (!dictionary.ContainsKey(key))
          dictionary[key] = new List<Location>();
        dictionary[key].Add(loc);
      }
      List<Tuple<Location, List<Location>>> list1 = new List<Tuple<Location, List<Location>>>();
      foreach (KeyValuePair<Location, List<Location>> keyValuePair in dictionary)
      {
        Location key = keyValuePair.Key;
        List<Location> list2 = keyValuePair.Value;
        if (list2.Count > 1)
          list1.Add(Tuple.Create<Location, List<Location>>(key, list2));
      }
      return list1;
    }

    public List<Island> Islands()
    {
      return new List<Island>((IEnumerable<Island>) this.AllIslands);
    }

    public List<Island> MyIslands()
    {
      return Enumerable.ToList<Island>(Enumerable.Where<Island>((IEnumerable<Island>) this.AllIslands, (Func<Island, bool>) (island => island.Owner == this.ME)));
    }

    public List<Island> EnemyIslands()
    {
      return Enumerable.ToList<Island>(Enumerable.Where<Island>((IEnumerable<Island>) this.AllIslands, (Func<Island, bool>) (island =>
      {
        if (island.Owner != this.ME)
          return island.Owner != -1;
        return false;
      })));
    }

    public List<Island> NeutralIslands()
    {
      return Enumerable.ToList<Island>(Enumerable.Where<Island>((IEnumerable<Island>) this.AllIslands, (Func<Island, bool>) (island => island.Owner == -1)));
    }

    public List<Island> NotMyIslands()
    {
      return Enumerable.ToList<Island>(Enumerable.Where<Island>((IEnumerable<Island>) this.AllIslands, (Func<Island, bool>) (island => island.Owner != this.ME)));
    }

    public List<Island> MyIslandsInDanger()
    {
      return Enumerable.ToList<Island>(Enumerable.Where<Island>((IEnumerable<Island>) this.AllIslands, (Func<Island, bool>) (island =>
      {
        if (island.Owner == this.ME)
          return island.TeamCapturing != -1;
        return false;
      })));
    }

    public List<Island> IslandsBeingCapturedByMe()
    {
      return Enumerable.ToList<Island>(Enumerable.Where<Island>((IEnumerable<Island>) this.AllIslands, (Func<Island, bool>) (island => island.TeamCapturing == this.ME)));
    }

    public Island GetIsland(int id)
    {
      return Enumerable.FirstOrDefault<Island>((IEnumerable<Island>) this.AllIslands, (Func<Island, bool>) (island => island.Id == id));
    }

    public List<Pirate> AllMyPirates()
    {
      return new List<Pirate>((IEnumerable<Pirate>) this.SortedMyPirates);
    }

    public List<Pirate> MyPirates()
    {
      return Enumerable.ToList<Pirate>(Enumerable.Where<Pirate>((IEnumerable<Pirate>) this.AllMyPirates(), (Func<Pirate, bool>) (pirate => !pirate.IsLost)));
    }

    private List<Location> MyPiratesLocations()
    {
      return Enumerable.ToList<Location>(Enumerable.Select<Pirate, Location>((IEnumerable<Pirate>) this.SortedMyPirates, (Func<Pirate, Location>) (pirate => pirate.Loc)));
    }

    public List<Pirate> MyLostPirates()
    {
      return Enumerable.ToList<Pirate>(Enumerable.Where<Pirate>((IEnumerable<Pirate>) this.AllMyPirates(), (Func<Pirate, bool>) (pirate => pirate.IsLost)));
    }

    public List<Pirate> AllEnemyPirates()
    {
      return new List<Pirate>((IEnumerable<Pirate>) this.SortedEnemyPirates);
    }

    public List<Pirate> EnemyPirates()
    {
      return Enumerable.ToList<Pirate>(Enumerable.Where<Pirate>((IEnumerable<Pirate>) this.SortedEnemyPirates, (Func<Pirate, bool>) (pirate =>
      {
        if (!pirate.IsLost)
          return !pirate.IsCloaked;
        return false;
      })));
    }

    public List<Pirate> EnemyLostPirates()
    {
      return Enumerable.ToList<Pirate>(Enumerable.Where<Pirate>((IEnumerable<Pirate>) this.SortedEnemyPirates, (Func<Pirate, bool>) (pirate => pirate.IsLost)));
    }

    public Pirate GetMyPirate(int id)
    {
      if (id < 0 || id >= this.SortedMyPirates.Count)
        return (Pirate) null;
      return this.SortedMyPirates[id];
    }

    public Pirate GetEnemyPirate(int id)
    {
      if (id < 0 || id >= this.SortedEnemyPirates.Count)
        return (Pirate) null;
      return this.SortedEnemyPirates[id];
    }

    public Pirate GetPirateOn(Island island)
    {
      return this.GetPirateFromLocation(island.Loc);
    }

    public Pirate GetPirateOn(Location location)
    {
      return this.GetPirateFromLocation(location);
    }

    public Pirate GetPirateFromLocation(Location location)
    {
      if (this.Loc2Pirate.ContainsKey(location))
        return this.Loc2Pirate[location];
      return (Pirate) null;
    }

    public bool isCapturing(Pirate pirate)
    {
      return Enumerable.Any<Island>(Enumerable.Where<Island>((IEnumerable<Island>) this.AllIslands, (Func<Island, bool>) (island => island.Owner != pirate.Owner)), (Func<Island, bool>) (island => island.Loc.Equals(pirate.Loc)));
    }

    public bool IsPassable(Location location)
    {
      if (location.Row >= 0 && location.Col >= 0 && (this.Map[location.Row, location.Col] != -4 && location.Row < this.Rows))
        return location.Col < this.Cols;
      return false;
    }

    public Location Destination(Pirate a, Direction d)
    {
      return this.Destination(a.Loc, d);
    }

    public Location Destination(Location loc, Direction d)
    {
      Location location = InnerConsts.AIM[d];
      return new Location(loc.Row + location.Row % this.Rows, loc.Col + location.Col % this.Cols);
    }

    public int Distance(Location loc1, Location loc2)
    {
      int val1_1 = Math.Abs(loc1.Row - loc2.Row);
      int val1_2 = Math.Abs(loc1.Col - loc2.Col);
      if (!this.Cyclic)
        return val1_1 + val1_2;
      return Math.Min(val1_2, this.Cols - val1_2) + Math.Min(val1_1, this.Rows - val1_1);
    }

    public int Distance(Pirate pirate1, Pirate pirate2)
    {
      return this.Distance(pirate1.Loc, pirate2.Loc);
    }

    public int Distance(Pirate pirate, Island island)
    {
      return this.Distance(pirate.Loc, island.Loc);
    }

    public int Distance(Island island, Pirate pirate)
    {
      return this.Distance(pirate.Loc, island.Loc);
    }

    public int Distance(Island island1, Island island2)
    {
      return this.Distance(island1.Loc, island2.Loc);
    }

    public List<Direction> GetDirections(Location loc1, Location loc2)
    {
      int num1 = this.Rows / 2;
      int num2 = this.Cols / 2;
      List<Direction> list = new List<Direction>();
      if (loc1.Equals(loc2))
      {
        list.Add(Direction.NOTHING);
      }
      else
      {
        if (loc1.Row < loc2.Row)
        {
          if (loc2.Row - loc1.Row >= num1 && this.Cyclic)
            list.Add(Direction.NORTH);
          else if (loc2.Row - loc1.Row <= num1 || !this.Cyclic)
            list.Add(Direction.SOUTH);
        }
        else if (loc1.Row > loc2.Row)
        {
          if (loc1.Row - loc2.Row >= num1 && this.Cyclic)
            list.Add(Direction.SOUTH);
          else if (loc1.Row - loc2.Row <= num1 || !this.Cyclic)
            list.Add(Direction.NORTH);
        }
        if (loc1.Col < loc2.Col)
        {
          if (loc2.Col - loc1.Col >= num2 && this.Cyclic)
            list.Add(Direction.WEST);
          else if (loc2.Col - loc1.Col <= num2 || !this.Cyclic)
            list.Add(Direction.EAST);
        }
        else if (loc1.Col > loc2.Col)
        {
          if (loc1.Col - loc2.Col >= num2 && this.Cyclic)
            list.Add(Direction.EAST);
          else if (loc1.Col - loc2.Col <= num2 || !this.Cyclic)
            list.Add(Direction.WEST);
        }
      }
      return list;
    }

    public List<Direction> GetDirections(Pirate pirate, Island island)
    {
      return this.GetDirections(pirate.Loc, island.Loc);
    }

    public List<Direction> GetDirections(Pirate pirate1, Pirate pirate2)
    {
      return this.GetDirections(pirate1.Loc, pirate2.Loc);
    }

    public List<Direction> GetDirections(Pirate pirate, Location location)
    {
      return this.GetDirections(pirate.Loc, location);
    }

    public List<int> GetScores()
    {
      return this.Scores;
    }

    public int GetMyScore()
    {
      return this.Scores[0];
    }

    public int GetEnemyScore()
    {
      return this.Scores[1];
    }

    public List<int> GetLastTurnPoints()
    {
      return this.LastTurnPoints;
    }

    public int GetMaxPoints()
    {
      return this.MaxPoints;
    }

    public Pirate GetMyCloaked()
    {
      return Enumerable.FirstOrDefault<Pirate>((IEnumerable<Pirate>) this.MyPirates(), (Func<Pirate, bool>) (pir => pir.IsCloaked));
    }

    public bool CanCloak()
    {
      return this.CloakCooldowns[0] == 0;
    }

    public List<int> GetCloakCooldowns()
    {
      return this.CloakCooldowns;
    }

    public bool InRange(Pirate a1, Pirate a2)
    {
      return this.InRange(a1.Loc, a2.Loc);
    }

    public bool InRange(Location l1, Pirate a2)
    {
      return this.InRange(l1, a2.Loc);
    }

    public bool InRange(Pirate a1, Location loc2)
    {
      return this.InRange(a1.Loc, loc2);
    }

    public bool InRange(Location loc1, Location loc2)
    {
      double num = Math.Pow((double) (loc1.Row - loc2.Row), 2.0) + Math.Pow((double) (loc1.Col - loc2.Col), 2.0);
      return 0.0 < num && num <= (double) this.AttackRadius2;
    }

    public bool IsOccupied(Location loc)
    {
      return Enumerable.Any<Pirate>(Enumerable.Where<Pirate>((IEnumerable<Pirate>) this.AllPirates, (Func<Pirate, bool>) (pirate => !pirate.IsLost)), (Func<Pirate, bool>) (pirate => pirate.Loc.Equals(loc)));
    }

    public Dictionary<Pirate, List<Direction>> InDanger(Pirate anAnt, List<Pirate> possibleAttackers = null)
    {
      if (possibleAttackers == null)
        possibleAttackers = this.EnemyPirates();
      Dictionary<Pirate, List<Direction>> dictionary = new Dictionary<Pirate, List<Direction>>();
      foreach (Pirate a in possibleAttackers)
      {
        List<Direction> list = new List<Direction>();
        foreach (Direction d in list)
        {
          Location loc2 = this.Destination(a, d);
          if (this.InRange(anAnt, loc2))
            list.Add(d);
        }
        if (list.Count > 0)
          dictionary[a] = list;
      }
      return dictionary;
    }

    public bool IsEmpty(Location otherLocation)
    {
      return Enumerable.All<Location>(Enumerable.Select<KeyValuePair<Location, Direction>, Location>((IEnumerable<KeyValuePair<Location, Direction>>) this.Orders, (Func<KeyValuePair<Location, Direction>, Location>) (kv => this.Destination(kv.Key, kv.Value))), (Func<Location, bool>) (location => location != otherLocation));
    }

    public int GetRows()
    {
      return this.Rows;
    }

    public int GetCols()
    {
      return this.Cols;
    }

    public bool isVisible(Location loc)
    {
      if (this.Vision == null)
      {
        if (this.VisionOffsets2 == null)
        {
          this.VisionOffsets2 = new List<Location>();
          int num = (int) Math.Sqrt((double) this.ViewRadius2);
          for (int index1 = -num; index1 <= num; ++index1)
          {
            for (int index2 = -num; index2 <= num; ++index2)
            {
              if ((int) (Math.Pow((double) index1, 2.0) + Math.Pow((double) index2, 2.0)) <= this.ViewRadius2)
                this.VisionOffsets2.Add(new Location(index1 % this.Rows - this.Rows, index2 % this.Cols - this.Cols));
            }
          }
        }
        this.Vision = new bool[this.Rows, this.Cols];
        for (int index1 = 0; index1 < this.Rows; ++index1)
        {
          for (int index2 = 0; index2 < this.Cols; ++index2)
            this.Vision[index1, index2] = false;
        }
        foreach (Pirate pirate in this.MyPirates())
        {
          Location loc1 = pirate.Loc;
          foreach (Location location in this.VisionOffsets2)
            this.Vision[loc1.Row + location.Row, loc1.Col + location.Col] = true;
        }
      }
      return this.Vision[loc.Row, loc.Col];
    }
  }
}
