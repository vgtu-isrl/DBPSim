using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine.Translator
{
    
    public interface IRuleTranslator
    {

        string Translate(RuleBase rule, string input);

    }

}
