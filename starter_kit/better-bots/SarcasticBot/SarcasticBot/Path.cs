using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }

        public Path CalculatePathToStationaryTarget(Group origin, Location end)
        {
            throw new NotImplementedException();
        }

        public Path CalculatePathFastToStationaryTarget(Group origin, Path oldPath, Location end)
        {
            throw new NotImplementedException();
        }

        public Direction GetNextMove()
        {
            throw new NotImplementedException();
        }

        public Path CalculatePathToMovingTarget(EnemyGroup eGroup, Group origin)
        {
            throw new NotImplementedException();
        }
    }
}