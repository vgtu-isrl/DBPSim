using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.Model
{
    public class EndAction : ModelElement
    {

        public EndAction(string title)
            : base(title)
        {
        }


        public override string ToString()
        {
            return "(EndAction: " + this.Title + ")";
        }

    }
}
