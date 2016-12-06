using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.Model
{
    public class StartAction : ModelElement
    {

        private ModelAction _modelAction;


        public StartAction(string title)
            : base(title)
        {            
        }


        public ModelAction ModelAction
        {
            get
            {
                return this._modelAction;
            }
            set                
            {
                this._modelAction = value;
            }
        }


        public override string ToString()
        {
            return "(StartAction: " + this.Title + ")";
        }

    }
}
