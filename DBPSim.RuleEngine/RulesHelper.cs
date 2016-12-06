using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.RuleEngine
{
    public class RulesHelper
    {

        private static string _ruleTemplateVB = null;
        private static string _ruleTemplateCSharp = null;

        private const string RULETEMPLATENAMESPACEVB = "DBPSim.RuleEngine.Template.RuleTemplateVB.txt";
        private const string RULETEMPLATENAMESPACECSHARP = "DBPSim.RuleEngine.Template.RuleTemplateCSharp.txt";


        public static int LogSize
        {
            get { return 100; }
        }


        public static string RuleTemplateVB
        {
            get
            {
                if (RulesHelper._ruleTemplateVB == null)
                {
                    Stream ruleTemplateStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RULETEMPLATENAMESPACEVB);
                    using (StreamReader sr = new StreamReader(ruleTemplateStream))
                    {
                        RulesHelper._ruleTemplateVB = sr.ReadToEnd();
                    }
                }
                return RulesHelper._ruleTemplateVB;
            }
        }


        public static string RuleTemplateCSharp
        {
            get
            {
                if (RulesHelper._ruleTemplateCSharp == null)
                {
                    Stream ruleTemplateStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RULETEMPLATENAMESPACECSHARP);
                    using (StreamReader sr = new StreamReader(ruleTemplateStream))
                    {
                        RulesHelper._ruleTemplateCSharp = sr.ReadToEnd();
                    }
                }
                return RulesHelper._ruleTemplateCSharp;
            }
        }

    }
}
