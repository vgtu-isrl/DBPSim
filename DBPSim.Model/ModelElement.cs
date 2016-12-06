using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.Model
{
    public abstract class ModelElement
    {

        private string _title;                   

        public ModelElement(string title) 
        {
            this._title = title;
        }


        public string Title
        {
            get
            {
                return this._title;
            }
        }
      
    }
}
