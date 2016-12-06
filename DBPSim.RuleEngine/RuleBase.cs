using DBPSim.RuleEngine.Execution;
using DBPSim.RuleEngine.Translator;
using System;
using System.Reflection;

namespace DBPSim.RuleEngine
{

    public abstract class RuleBase
    {

        // Rule properties
        private string _id;
        private int? _priority;
        private bool _enabled;
        private string _title;
        private string _condition;
        private string _body;

        // Duration (mins)
        private int? _duration;
        // Rules engine
        private RulesEngine _rulesEngine;

        // Rule change indicator
        private bool _changed = true;

        // Assembly
        private Assembly _assembly;

        // Execution log
        private ExecutionResultCollection _resultCollection;


        public RuleBase()
        {
            this._resultCollection = new ExecutionResultCollection(RulesHelper.LogSize);
        }


        public RuleBase(string ruleId, int? priority, string ruleTitle, string ruleCondition, string ruleBody)
            : this()
        {
            this._id = ruleId;
            this._priority = priority;
            this._title = ruleTitle;
            this._condition = ruleCondition;
            this._body = ruleBody;
        }


        public virtual int? Priority
        {
            get
            {
                return this._priority;
            }
            set
            {
                this._priority = value;
            }
        }


        public bool Enabled
        {
            get
            {
                return true;
            }
            set
            {
                this._enabled = value;
            }
        }


        public virtual string Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }


        public virtual string Title
        {
            get
            {
                return this._title;
            }
            set
            {
                this._title = value;
            }
        }


        public virtual string Condition
        {
            get
            {
                return this._condition;
            }
            set
            {
                if (this._condition != null && !this._condition.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    this._changed = true;
                    RuleEngineEvents.ExecuteEvent(RuleEngineEvents.OnRuleChanged, this, RuleEventArgs.Load(false, "Rule condition was changed", null));
                }
                this._condition = value;
            }
        }


        public string Body
        {
            get
            {
                return this._body;
            }
            set
            {
                if (this._body != null && !this._body.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    this._changed = true;
                    RuleEngineEvents.ExecuteEvent(RuleEngineEvents.OnRuleChanged, this, RuleEventArgs.Load(false, "Rule body was changed", null));
                }
                this._body = value;
            }
        }


        public int? Duration
        {
            get { return this._duration; }
            set { this._duration = value; }
        }


        public virtual string FullRule
        {
            get
            {
                return RuleTranslator.Translate(this);
            }
        }


        public RulesEngine RulesEngine
        {
            get
            {
                return this._rulesEngine;
            }
            set
            {
                this._rulesEngine = value;
            }
        }


        public Assembly Assembly
        {
            get
            {
                return this._assembly;
            }
            set
            {
                this._assembly = value;
            }
        }


        public bool Changed
        {
            get
            {
                return this._changed;
            }
            set
            {
                this._changed = value;
            }
        }


        public virtual ExecutionResultCollection ExecutionResultCollection
        {
            get
            {
                return this._resultCollection;
            }
        }


        public override string ToString()
        {
            if (this._id != null && this._title != null)
            {
                return this._id + "_" + this._title;
            }
            return base.ToString();
        }

    }
}
