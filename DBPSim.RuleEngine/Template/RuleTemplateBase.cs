using DBPSim.RuleEngine.Execution;
using DBPSim.RuleEngine.Memory;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using DBPSim.Resources;
using System.Xml.Linq;
using System.Threading;
using System.Globalization;

namespace DBPSim.RuleEngine.Template
{
    public class RuleTemplateBase : ExecutionStateAcitivity
    {
        //tadas 
        public int ActivityDuration;
        public int ID = -1;
        //DateTime StartDate=new DateTime();
        //tadas
        protected bool _eventFiringEnabled = true;
        protected WorkingMemory _workingMemory;
        protected RuleBase _rule;
        protected RulesEngine _rulesEngine;

        protected RuleExecutionResult _ruleResult;

        public RuleTemplateBase(RulesEngine rulesEngine, WorkingMemory workingMemory, RuleBase rule)
        {
            this._workingMemory = workingMemory;
            this._rule = rule;
            this._rulesEngine = rulesEngine;
            this._ruleResult = new RuleExecutionResult(rule);
        }


        public RuleBase Rule
        {
            get { return this._rule; }
        }


        public WorkingMemory Memory
        {
            get
            {
                return this._workingMemory;
            }
        }


        public int? Duration
        {
            set
            {
                this._rule.Duration = value;
            }
        }


        public virtual RuleExecutionResult Execute()
        {
            return this._ruleResult;
        }


        // Rule body commands
        protected void PUSH(string parameterName, object result)
        {
            lock (_ruleResult.ResultParameters)
            {
                this._ruleResult.ResultParameters.Add(parameterName, result);
            }
        }
        protected object POP(string parameterName, object result)
        {
            lock (this._ruleResult.ResultParameters)
            {
                return this._ruleResult.ResultParameters.Where(item => item.Key == parameterName);
            }
        }
        
        
        
        //Tadas Version
        protected void RES_ALLOCATE(string title, int amount, int duration, string activityName= null)
        {
            var al = new DBPSim.Resources.ManageResource();
            ActivityDuration += duration;
            var a = al.ResourceAllocation(title, amount, ActivityDuration, activityName?? this.Rule.Title);
            if (a == -1)
            {
                throw new ForceExecutionStopException("Unable to allocate resource " + title + ". No resourcle like this");
            }
            else
            {
                ID = a;
            }
            //CREATE("ResourceWasAllocated");
        }
        protected void RES_CANCEL(string title, int raID, int amount, int duration)
        {
            //if (ID == -1 )
            //{
            //    throw new ForceExecutionStopException("No ID");
            //}
            //else
            //{
            var al = new DBPSim.Resources.ManageResource();
            ActivityDuration += duration;
            al.Cancel(title, raID, amount, duration);
            //}
        }
        protected void RES_FILL(string title, int amount, int duration)
        {
            var al = new DBPSim.Resources.ManageResource();
            ActivityDuration += duration;
            var a = al.AddResources(title, "Amount", amount);
            if (a == false)
            {
                throw new ForceExecutionStopException("Unable update resources " + title + ". No resourcle like this");
            }
        }
        protected int RES_FINDAMOUNT(string title, string propertie, int duration)
        {
            var fa = new DBPSim.Resources.ManageResource();
            ActivityDuration += duration;
            var amount = fa.Amount(title, propertie);
            return amount;
        }
        protected void RES_RELEASE(string title, int duration)
        {
            var rel = new DBPSim.Resources.ManageResource();
            ActivityDuration += duration;
            rel.ResourceRelease(title, ActivityDuration);
        }

        protected void CHANGE(ref object variable, object NewVal, int duration)
        {
            variable = NewVal;
            ActivityDuration += duration;
        }
        //Tadas Version
        protected void CREATE(string parameterName)
        {
            dynamic value = new ExpandoObject();
            bool insertSucceeded = false;
            lock (this._workingMemory)
            {
                insertSucceeded = this._workingMemory.Insert(parameterName, value);
            }
            if (insertSucceeded)
            {
                RuleEngineEvents.ExecuteEvent(RuleEngineEvents.OnFactInsert, this, RuleEventArgs.Load(false, parameterName + ": Fact was inserted to working memory.", null));
            }
            else
            {
                RuleEngineEvents.ExecuteEvent(RuleEngineEvents.OnFactInsert, this, RuleEventArgs.Load(false, parameterName + ": Fact was overwritten.", null));
                //throw new ForceExecutionStopException("Unable to insert fact " + parameterName + " because it already exists");
            }
        }
        protected void CREATE(string parameterName, int duration)
        {
            dynamic value = new ExpandoObject();
            bool insertSucceeded = false;
            lock (this._workingMemory)
            {
                insertSucceeded = this._workingMemory.Insert(parameterName, value);
            }
            if (insertSucceeded)
            {
                Interlocked.Add(ref ActivityDuration, duration);
                RuleEngineEvents.ExecuteEvent(RuleEngineEvents.OnFactInsert, this, RuleEventArgs.Load(false, parameterName + ": Fact was inserted to working memory.", null));
            }
            else
            {
                RuleEngineEvents.ExecuteEvent(RuleEngineEvents.OnFactInsert, this, RuleEventArgs.Load(false, parameterName + ": Fact already exists.", null));
                //throw new ForceExecutionStopException("Unable to insert fact " + parameterName + " because it already exists");
            }
        }

        protected void INSERT(string parameterName, object value)
        {
            bool insertSucceeded = false;

            lock (this._workingMemory)
            {
                insertSucceeded = this._workingMemory.Insert(parameterName, value);
            }
            if (insertSucceeded)
            {
                RuleEngineEvents.ExecuteEvent(RuleEngineEvents.OnFactInsert, this, RuleEventArgs.Load(false, parameterName + ": Fact was inserted to working memory.", null));
            }
            else
            {
                RuleEngineEvents.ExecuteEvent(RuleEngineEvents.OnFactInsert, this, RuleEventArgs.Load(false, parameterName + ": Fact was overwritten.", null));
                //throw new ForceExecutionStopException("Unable to insert fact " + parameterName + " because it already exists");
            }
        }


        protected void RETRACT(string parameterName, int duration)
        {

            bool retractSucceeded = false;

            lock (this._workingMemory)
            {
                retractSucceeded = this._workingMemory.Retract(parameterName);
            }
            Interlocked.Add(ref ActivityDuration, duration);
            //if (retractSucceeded)
            //{
            //    RuleEngineEvents.ExecuteEvent(RuleEngineEvents.OnFactRetract, this, RuleEventArgs.Load(false, parameterName + ": Fact was retracted from working memory", null));
            //}
            //else
            //{
            //    throw new ForceExecutionStopException("Unable to retract fact " + parameterName + " because it already removed or does not exists");
            //}
        }


        protected void REMOVE(string parameterName)
        {
            bool retractSucceeded = false;

            lock (this._workingMemory)
            {
                retractSucceeded = this._workingMemory.Retract(parameterName);
            }
            //if (retractSucceeded)
            //{
            //    RuleEngineEvents.ExecuteEvent(RuleEngineEvents.OnFactRetract, this, RuleEventArgs.Load(false, parameterName + ": Fact was retracted from working memory", null));
            //}
            //else
            //{
            //    throw new ForceExecutionStopException("Unable to retract fact " + parameterName + " because it already removed or does not exists");
            //}
        }


        protected T MEM<T>(string parameterName)
        {
            lock (this._workingMemory)
            {
                return (T)this._workingMemory.ParamWorkingMemory<T>(parameterName);
            }
        }


        protected object MEM(string parameterName)
        {
            lock (this._workingMemory)
            {
                return this._workingMemory.ParamWorkingMemory<object>(parameterName);
            }
        }


        protected object POINTS(string parameterName)
        {
            IDictionary<string, object> points = (IDictionary<string, object>)WatchPoints.Instance;
            if (points.ContainsKey(parameterName))
            {
                return ((IDictionary<string, object>)WatchPoints.Instance)[parameterName];
            }
            throw new InvalidOperationException("Watch point " + parameterName + " does not exists.");
        }


        protected void POINTS(string parameterName, object value)
        {
            lock (WatchPoints.Instance)
            {
                IDictionary<string, object> points = (IDictionary<string, object>)WatchPoints.Instance;
                if (points.ContainsKey(parameterName))
                {
                    points[parameterName] = value;
                }
                else
                {
                    throw new InvalidOperationException("Watch point " + parameterName + " does not exists.");
                }
            }
        }

        protected void ACT(int timestamp, Action action)
        {
            this._rulesEngine.Subscribe(timestamp, this, action);
            ActionsToBeTaken++;
        }

        
        protected bool RESERVE(ref int num)
        {
            if (num > 0)
            {
                num -= 1;
                return true;
            }
            else if (num == 0)
            {
                return false;
            }
            else
            {
                throw new ArgumentException("Reservation for negative value");
            }
        }

        protected void CLEARMEMORY()
        {
            lock (this._workingMemory)
            {
                this._workingMemory.Select(x => x.Key).ToList().ForEach(x => this._workingMemory.Retract(x));
            }
        }

        protected void BBNG_TRYLOADMODEL(string filename)
        {

            var engine = (BBNGs.Engine.BBNGine)this.Rule.RulesEngine.GetPlugin("bbngs");
            if (engine == null)
            {
                engine = new BBNGs.Engine.BBNGine();
                engine.LoadXesDocument(filename);
                this.Rule.RulesEngine.AddPlugin("bbngs", engine);
                engine.ExtractDistinctEvents();
                engine.ExtractTree();
                engine.CreateCPT();
            }


            XElement rules = new XElement("RuleCollection");
            int counter = 1;
            foreach (var node in engine.traceTree.distinctNodes)
            {
                XElement rule = new XElement("Rule",                    
                    new XElement("Id",counter++),
                    new XElement("Enabled","True"),
                    new XElement("Title",node.Key),
                    new XElement("Priority","10"),
                    new XElement("Condition","Fact.Exists(\"started\") And BBNG_ISNEXTEVENT(\""+node.Key+"\")"),
                    new XElement("Body","BBNG_OBSERVEVALUE(\""+node.Key+ "\",\"occured\",\"1\")\r\nBBNG_OBSERVEGENERATEDDATA(\"" + node.Key+ "\")\r\nBBNG_OBSERVEVALUE(\"" + node.Key + "\",\"previous\",\"1\") ")// "Create(\"Fired "+node.Key+ "\")\r\n")
                    );
                rules.Add(rule);
            }
            DataSource.XmlDataSourceProvider prov = new DataSource.XmlDataSourceProvider(rules.ToString());
            RuleCollection rs = prov.Load();

            var newRules = new RuleCollection();

            foreach (var rule in rs)
            {
                bool found = false;
                foreach (var r in this._rulesEngine.Rules)
                {
                    if (r.Title == rule.Title)
                    {
                        found = true;
                        newRules.Add(r);
                        break;
                    }
                }
                if (!found)
                {
                    newRules.Add(rule);
                }
            }
            foreach( var r in this._rulesEngine.Rules)
            {
                if(newRules.Where(x=>x.Title==r.Title).FirstOrDefault()== null)
                {
                    newRules.Add(r);
                }
            }

            //foreach (var rule in this._rulesEngine.Rules)
            //{
            //    bool found = false;
            //    foreach (var exrule in rs)
            //    {
            //        if (rule.Title == exrule.Title)
            //        {
            //            found = true;
            //            break;
            //        }
            //    }
            //    if (!found)
            //    {
            //        rs.Add(rule);
            //    }
            //}
            this._rulesEngine.Rules = newRules;
        }

        protected void BBNG_OBSERVEVALUE(string node, string fact,string value)
        {
            var engine = (BBNGs.Engine.BBNGine)this._rulesEngine.GetPlugin("bbngs");
            engine.probs.ObserveVariable(node, fact, value);
        }

        protected bool BBNG_ISNEXTEVENT(string node)
        {
            var engine = (BBNGs.Engine.BBNGine)this._rulesEngine.GetPlugin("bbngs");
           var bestNextChoice = engine.probs.GetMostProbableNextChoice();
            return bestNextChoice == node;
        }

        protected string BBNG_NEXTEVENT()
        {
            var engine = (BBNGs.Engine.BBNGine)this._rulesEngine.GetPlugin("bbngs");
            if(engine == null)
            {
                return null;
            }
            var next = engine.probs.GetMostProbableNextChoice();
            return next;
        }

        protected void BBNG_RESETOBSERVATIONS()
        {
            var engine = (BBNGs.Engine.BBNGine)this._rulesEngine.GetPlugin("bbngs");
            engine.probs.ClearPredictions();
        }

        protected Dictionary<string, string> BBNG_GENERATEDATA(string node)
        {
            var engine = (BBNGs.Engine.BBNGine)this._rulesEngine.GetPlugin("bbngs");
            var data = engine.probs.GenerateData(node);
            return data != null ? data.data : null;
        }

        protected void BBNG_OBSERVEGENERATEDDATA(string node)
        {

            var engine = (BBNGs.Engine.BBNGine)this._rulesEngine.GetPlugin("bbngs");
            var data = engine.probs.GenerateData(node);
            if(data != null)
            {
                foreach (var x in data.data)
                {
                    if (x.Key.StartsWith(node))
                    {
                        engine.probs.ObserveVariable(node, x.Key.Remove(0, x.Key.LastIndexOf('_') + 1), x.Value);
                        INSERT(x.Key, new Dictionary<string, object>() { { "value", x.Value } });
                    }
                }
            }
        }


        // TODO: Need to have all rules in this context because one rule can fire another rules
        protected void FireAllRules()
        {

        }

    }

    public class ProbabilityDistribution
    {
        class Probability
        {
            public double Chance { get; set; }
            public double Min { get; set; }
            public double Max { get; set; }
        }

        private List<Probability> probs = new List<Probability>();
        private Func<double, object> mapFunc = null;
        private Random r = System.Diagnostics.Debugger.IsAttached ? new Random(1) : new Random();
        private double _probSum { get; set; }

        public ProbabilityDistribution(string val, Func<double,object> mapFunc)
        {
            this.mapFunc = mapFunc;
            string[] probs = val.Split(';');
            
            foreach(var prob in probs)
            {
                var probDef = prob.Split(':');
                var chance = double.Parse(probDef[0].Replace(',', '.'), CultureInfo.InvariantCulture);
                var minmax = probDef[1].Split('/');
                var min = double.Parse(minmax[0].Replace(',', '.'), CultureInfo.InvariantCulture);
                var max = double.Parse(minmax[1].Replace(',', '.'), CultureInfo.InvariantCulture);
                this.probs.Add(new Probability() { Chance = chance, Min = min, Max = max });                
            }
            _probSum = this.probs.Sum(x => x.Chance);

        }

        public double NextDouble()
        {
            var val = r.Next(0, _probSum);
            double tmp = 0;
            if(val < probs[0].Chance)
            {
                return r.Next(probs[0].Min, probs[0].Max);
            }
            for( int i = 0; i < probs.Count; i++)
            {
                if(probs[i].Chance+tmp <= val)
                {
                    return r.Next(probs[i].Min, probs[i].Max);
                }
                tmp = probs[i].Chance;
            }
            throw new ArgumentException("Failed to generate probability");
        }
        
        public object Next()
        {
            return mapFunc(NextDouble());
        }

    }

}
