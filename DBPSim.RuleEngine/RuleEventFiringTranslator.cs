using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron.RuleEngine.Translator
{
    public class RuleEventFiringTranslator : IRuleTranslator
    {

        public RuleEventFiringTranslator()
        {
        }


        public string Translate(RuleBase rule, string input)
        {
            // Replace &EventFiringEnabled = True
            input = input.Replace("&EventFiringEnabled = True", " Me._eventFiringEnabled = True ");
            input = input.Replace("&EventFiringEnabled=True", " Me._eventFiringEnabled = True ");
            input = input.Replace("&EventFiringEnabled= True", " Me._eventFiringEnabled = True ");
            input = input.Replace("&EventFiringEnabled =True", " Me._eventFiringEnabled = True ");
            // Replace &EventFiringEnabled = False
            input = input.Replace("&EventFiringEnabled = False", " Me._eventFiringEnabled = False ");
            input = input.Replace("&EventFiringEnabled=False", " Me._eventFiringEnabled = False ");
            input = input.Replace("&EventFiringEnabled= False", " Me._eventFiringEnabled = False ");
            input = input.Replace("&EventFiringEnabled =False", " Me._eventFiringEnabled = False ");
            return input;
        }

    }
}

