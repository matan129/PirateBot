using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pirates;

namespace Britbot
{
    public class SmartIsland : Island, ITarget
    {
        public SmartIsland(int Id, Location Loc, int Owner, int TeamCapturing, int TurnsBeingCaptured, int CaptureTurns,
            int Value) : base(Id, Loc, Owner, TeamCapturing, TurnsBeingCaptured, CaptureTurns, Value)
        {
        }

        public int ID { get; private set; }



        public int Value { get; private set; }
        
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public Score GetScore(Group origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Pirates.Location GetLocation()
        {
            throw new NotImplementedException();
        }
        
        public void EnemyNumber()
        {
            
            throw new System.NotImplementedException();
        }
    }
}
