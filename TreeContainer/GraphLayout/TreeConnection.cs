using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphLayout
{
    public struct TreeConnection
    {
        public ITreeNode IgnParent { get; private set; }
        public ITreeNode IgnChild { get; private set; }
        //public List<DPoint> LstPt { get; private set; }
        public bool Active;
        public List<DPoint> LstPt
        {
            get
            {
                return new List<DPoint>() {
                new DPoint(IgnParent.PosX+IgnParent.TreeWidth/2,IgnParent.PosY+IgnParent.TreeHeight),
                new DPoint(IgnChild.PosX+IgnChild.TreeWidth/2,IgnChild.PosY)
            };
            }
        }

        public TreeConnection(ITreeNode ignParent, ITreeNode ignChild, List<DPoint> lstPt)
            : this()
        {
            IgnChild = ignChild;
            IgnParent = ignParent;
            Active = false;
            //LstPt = lstPt;
        }
    }
}
