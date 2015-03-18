using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pirates;

namespace WeekOneBot
{
    public class Commander
    {
        private List<Fleet> fleets;
        private int[] config;

        private static Commander instance;

        private Commander()
        {
            
        }

        public static Commander Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Commander();
                }
                return instance;
            }
        }

        public void Distribute(IEnumerable<int> c)
        {
            this.config = c.ToArray();
            if (this.config != null || this.config.Sum() <= Bot.Game.AllMyPirates().Count)
            {
                this.fleets = new List<Fleet>();
                int sum = 0;
                List<int> friends = new List<int>();

                for (int i = 0; i < this.config.Length; i++)
                {
                    int f = this.config[i];

                    Bot.Game.Debug("Initing fleet {0} with {1} ships",i,f);

                    this.fleets.Add(new Fleet(sum, f, friends));

                    Bot.Game.Debug("Adding island # {0} to friends' targets list",this.fleets[i].Target);
                    friends.Add(this.fleets[i].Target);
                    sum += f;
                }
            }
        }

        private IEnumerable<int> GetFriends(int index)
        {
            for (int i = 0; i < this.fleets.Count; i++)
            {
                if(i != index)
                    yield return this.fleets[i].Target;
            }
        }

        public void Play()
        {
            for (int i = 0; i < this.fleets.Count; i++)
            {
                Fleet f = this.fleets[i];
                if (f.Status())
                {
                    f.ReSort(this.GetFriends(i).ToList());
                }
                f.SailAll();
            }
        }
    }
 
}
