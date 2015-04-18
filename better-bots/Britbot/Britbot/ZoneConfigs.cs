using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Britbot
{
    internal class ZoneConfigs
    {
        public int Capacity { get; private set; }
        public List<int> Groups { get; private set; }

        public ZoneConfigs(int capacity)
        {
            this.Capacity = capacity;
            this.Groups = new List<int>();
        }

        public void AddGroup(int count)
        {
            if (this.Capacity >= count)
            {
                this.Capacity -= count;
                this.Groups.Add(count);
            }
            else
            {
                throw new Exception("Not enough space in the zone");
            }
        }
    }
}
