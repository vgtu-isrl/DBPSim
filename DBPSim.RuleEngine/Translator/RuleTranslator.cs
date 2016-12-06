using System;
using System.Collections.Generic;

namespace DBPSim.RuleEngine.Translator
{

    public class RuleTranslator : List<IRuleTranslator>
    {

        private static RuleTranslator _instance;


        private RuleTranslator()
        {            
            // Translation rules
            this.Add(new RuleConditionTranslator());
            this.Add(new RuleBodyTranslator());
            this.Add(new RuleMemoryVariablesTranslator());
            this.Add(new RuleVariablesTranslator());
            this.Add(new RuleEventFiringTranslator());
            this.Add(new RuleForceStopTranslator());
            this.Add(new RuleEndTranslator());
            this.Add(new WatchPointTranslator());
            this.Add(new FactExistsTranslator());
        }


        public static string Translate(RuleBase rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }

            string fullRule = RulesHelper.RuleTemplateVB;
            foreach (IRuleTranslator translator in RuleTranslator.Instance)
            {
                fullRule = translator.Translate(rule, fullRule);
            }
            return fullRule;
        }


        public static void AddAdditionalRuleTranslator(IRuleTranslator translator)
        {
            if (translator == null)
            {
                throw new ArgumentNullException("translator");
            }
            //RuleTranslator.Instance.Insert(0, translator);
            RuleTranslator.Instance.Add(translator);
        }


        private static RuleTranslator Instance
        {
            get
            {
                if (RuleTranslator._instance == null)
                {
                    RuleTranslator._instance = new RuleTranslator();
                }
                return RuleTranslator._instance;
            }
        }

    }
}
