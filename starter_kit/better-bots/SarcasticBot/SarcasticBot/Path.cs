using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pirates;

namespace SarcasticBot
{
    public class Path
    {
        private Location CurrentLocation;
        private Location End;
        private Queue<Location> Steps;
        public ScoreStruct Score;

        public Path()
        {
            throw new System.NotImplementedException();
        }

        public Path CalculatePathToStationaryTarget(Group origin, Location end)
        {
            throw new System.NotImplementedException();
        }

        public Path CalculatePathFastToStationaryTarget(Group origin, Path oldPath, Location end)
        {
            throw new System.NotImplementedException();
        }

        public Location GetNextMove()
        {
            throw new System.NotImplementedException();
        }

        public Path CalculatePathToMovingTarget(SarcasticBot.EnemyGroup eGroup, Group origin)
        {
            throw new System.NotImplementedException();
        }
    }
}
