using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Britbot
{
    public class EnemyGroup : ITarget
    {
        public List<int> EnemyPirates
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public HeadingVector Heading
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }
    
        public Score GetScore(Group origin)
        {
            throw new NotImplementedException();
        }

        public Pirates.Location GetLocation()
        {
            throw new NotImplementedException();
        }
    }
}
