using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.Model
{
    public class Gateway : ModelElement
    {

        public Gateway(string title)
            : base(title)
        {
        }


        public override string ToString()
        {
            return "(Gateway: " + this.Title + ")";
        }

    }
}
