using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace WeekOneBot
{
    class EnemyPirate
    {
        public int Id {  get; private set; }

        private List<Location> path;

        public EnemyPirate(int id)
        {
            this.Id = id;
        }

        public void Update()
        {
            
        }
    }
}
