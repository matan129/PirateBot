using System.Collections.Generic;

namespace Britbot
{
    //TODO all of this
    public class Group
    {
        //Direction of the group to make navigation more precise
        private HeadingVector heading;

        public Group(int ID)
        {
            throw new System.NotImplementedException();
        }

        public List<int> Pirates { get; private set; }
        public ITarget Target { get; set; }

        public Group Role
        {
            get { throw new System.NotImplementedException(); }
            set { }
        }

        public int Id
        {
            get { throw new System.NotImplementedException(); }
            set { }
        }

        public List<Score> priorities
        {
            get { throw new System.NotImplementedException(); }
            set { }
        }

        public System.Threading.Thread CalcThread
        {
            get { throw new System.NotImplementedException(); }
            set { }
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