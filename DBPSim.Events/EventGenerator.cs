using DBPSim.RuleEngine;
using DBPSim.RuleEngine.Execution;
using DBPSim.RuleEngine.Memory;
using DBPSim.RuleEngine.Translator;
using System;
using System.Dynamic;

namespace DBPSim.Events
{
    public class EventGenerator
    {

        private static bool _translatorsProvided = false;


        public static Event GenerateEvent(string name, string configuration, DateTime? date)
        {
            //ProvideTranslators();            
            RuleNonConditional rule = new RuleNonConditional();
            rule.Id = name;
            rule.Title = name;
            rule.Body = configuration;

            EventRuleTranslator ruleTranslator = new EventRuleTranslator();
            ruleTranslator.Translate(rule, null);

            ExecutionResult executionResult = RuleExecutor.ExecuteRule(null,rule, new WorkingMemory());

            Event generatedEvent = null;

            if (rule.ExecutionResultCollection[0].SuccessfullyExecuted)
            {
                generatedEvent = new Event(name, configuration, date, (ExpandoObject)executionResult.Result.ResultParameters["event"]);
            }

            return generatedEvent;
        }


        public static DateTime GetRandomDate(DateTime dateFrom, DateTime dateTo, int hourFrom, int hourTo)
        {
            Random rnd = new Random();

            var range = dateTo - dateFrom;

            var randTimeSpan = new TimeSpan((long)(rnd.NextDouble() * range.Ticks));

            var hourDiff = hourTo - hourFrom -1;

            var randomHour = hourFrom + rnd.Next(hourDiff);

            var randomDate = dateFrom + randTimeSpan;

            return new DateTime(randomDate.Year, randomDate.Month, randomDate.Day, randomHour, randomDate.Minute, randomDate.Second);
        }


        internal static void ProvideTranslators()
        {
            if (!_translatorsProvided)
            {
                RuleTranslator.AddAdditionalRuleTranslator(new EventRuleTranslator());
                _translatorsProvided = true;
            }
        }

    }
}
