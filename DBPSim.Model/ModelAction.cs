using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.Model
{

    public class ModelAction : ModelElement
    {

        ModelElementCollection _elements;


        public ModelAction(string title)
            : base(title)
        {
            this._elements = new ModelElementCollection();
        }


        public ModelElementCollection Elements
        {
            get
            {
                return this._elements;
            }
        }


        public override string ToString()
        {
            return "(Action: " + this.Title + ")";
        }

    }

}
