using System.Collections.Generic;
using Pirates;

namespace Britbot
{
    //TODO all of this
    public class Group
    {
        public Group(int ID)
        {
            throw new System.NotImplementedException();
        }
    
        public List<int> Pirates { get; private set; }
        public ITarget Target { get; set; }

        //Direction of the group to make navigation more precise
        HeadingVector heading;

        public Group Role
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public int Id
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public List<Score> priorities
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public System.Threading.Thread CalcThread
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
            }
        }

        public void Move()
        {
            throw new System.NotImplementedException();
        }

        public void CalcPriorities()
        {
            throw new System.NotImplementedException();
        }
    }
}
