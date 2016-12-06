using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.Model
{
    public class SimulationModel
    {

        // Model start/end
        public StartAction _startAction;
        public EndAction _endAction;

        // Current element
        public ModelElement _currentElement;


        public StartAction StartAction
        {
            get
            {
                return this._startAction;
            }
            set
            {
                this._startAction = value;
            }
        }


        public EndAction EndAction
        {
            get
            {
                return this._endAction;
            }
            set
            {
                this._endAction = value;
            }
        }


        public ModelElement CurrentElement
        {
            get
            {
                if (this._currentElement == null)
                {
                    return this._startAction;
                }
                return this._currentElement;
            }
            set
            {
                this._currentElement = value;
            }
        }


        public ModelElement GetNextModelElement()
        {
            //if (this.CurrentElement is Gateway)
            //{
            //    throw new InvalidOperationException("This operation allowed only when current element is gateway.");
            //}
            //if (this.CurrentElement == null)
            //{
            //    throw new ArgumentNullException("CurrentElement");
            //}
            //ModelElement nextModelElement = this.CurrentElement.ConnectedElements.FirstOrDefault();
            //this.CurrentElement = nextModelElement;
            //return nextModelElement;

            throw new NotImplementedException();
        }


        public ModelElement GetNextModelElement(bool gatewayResult)
        {
            //if (!(this.CurrentElement is Gateway))
            //{
            //    throw new InvalidOperationException("This operation allowed only when current element is gateway");
            //}
            //if (this.CurrentElement == null)
            //{
            //    throw new ArgumentNullException("CurrentElement");
            //}
            //ModelElement nextModelElement = this.CurrentElement.ConnectedElements.Find(item => item.Flag == gatewayResult);
            //this.CurrentElement = nextModelElement;
            //return nextModelElement;

            throw new NotImplementedException();
        }

    }
}
