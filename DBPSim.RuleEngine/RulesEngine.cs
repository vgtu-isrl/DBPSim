using DBPSim.RuleEngine.Collision;
using DBPSim.RuleEngine.DataSource;
using DBPSim.RuleEngine.Execution;
using DBPSim.RuleEngine.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using DBPSim.RuleEngine.Template;

namespace DBPSim.RuleEngine
{
    public class RulesEngine
    {

        // Engine
        private DataSourceProvider _dataSource = null;
        private RuleCollection _rules = null;
        private WorkingMemory _workingMemory = null;
        private CollisionSolverBase _collisionSolver = null;

        private List<RuleTemplateBase> _unfinishedRules = new List<RuleTemplateBase>();

        public Action RuleChanged = null;
        public Action<int, RuleTemplateBase, Action> Subscribe = null;
        public Func<RuleCollection,bool> StepInTime = null;


        // Rule execution queue
        private RuleCollection _ruleExecutionQueue = null;

        // Executed rules
        private List<RuleCollection> _executedRules = new List<RuleCollection>();


        private Guid _instanceID = Guid.NewGuid();
        private string simStart = DateTime.Now.ToString("yyyyMMddhhmmss");


        public void InitEngine()
        {
            _ruleExecutionQueue = null;
            if(_executedRules.Count > 0)
            {
                if (!System.IO.Directory.Exists(Environment.CurrentDirectory+@"\execlogs"))
                {
                    System.IO.Directory.CreateDirectory(Environment.CurrentDirectory + @"\execlogs");
                }
                using (var sw = new System.IO.StreamWriter(Environment.CurrentDirectory + @"\execlogs\history_" + simStart + ".txt", true))
                {
                    foreach (var rls in _executedRules)
                    {
                        foreach (var rl in rls)
                        {
                            sw.WriteLine(_instanceID + "\t" + rl.Title);
                        }
                    }
                }
            }
            _executedRules = new List<RuleCollection>();
            _ruleExecutionQueue = null;
            _instanceID = Guid.NewGuid();
        }
        public int ExecutedSteps
        {
            get { return _executedRules.Count; }
        }

        public RulesEngine()
        {
            this._rules = new RuleCollection();            
        }


        public RulesEngine(DataSourceProvider dataSource)
        {
            if (dataSource == null)
            {
                throw new ArgumentNullException("dataSource");
            }
            this._dataSource = dataSource;
            this._rules = RuleCollection.Load(dataSource);
        }


        public DataSourceProvider DataSource
        {
            get
            {
                return this._dataSource;
            }
            set
            {
                this._dataSource = value;
            }
        }


        public RuleCollection Rules
        {
            get
            {
                return this._rules;
            }
            set
            {
                this._rules = value;
                if (RuleChanged != null)
                {
                    RuleChanged();
                }
            }
        }


        public RuleCollection RuleExecutionQueue
        {
            get
            {
                if (this._ruleExecutionQueue == null)
                {
                    this._ruleExecutionQueue = new RuleCollection();
                }
                return this._ruleExecutionQueue;
            }
        }


        public WorkingMemory WorkingMemory
        {
            get
            {
                if (this._workingMemory == null)
                {
                    // Default is empty memory
                    this._workingMemory = new WorkingMemory();
                }
                return this._workingMemory;
            }
            set
            {
                if (value == null)
                {
                    // throw exception?
                }
                this._workingMemory = value;
            }
        }


        public CollisionSolverBase CollisionSolver
        {
            get
            {
                if (this._collisionSolver == null)
                {
                    // Default solver is priority solver 
                    this._collisionSolver = new CollisionSolverPriority();
                }
                return this._collisionSolver;
            }
            set
            {
                this._collisionSolver = value;
            }
        }

        public void FireAllRules()
        {
            // Clear queue
            this.RuleExecutionQueue.Clear();

            // Get all rules to execute     

            this.FilterEnabledRules().ToList().ForEach(rule=>//.AsParallel().ForAll(rule =>
            {
                bool ruleConditionExecutionResult = RuleExecutor.ExecuteRuleCondition(this, rule, this.WorkingMemory);
                if (ruleConditionExecutionResult)
                {
                    lock (this.RuleExecutionQueue)
                    {
                        this.RuleExecutionQueue.Add(rule);
                    }
                }
            });

            // Determine rule execution order by solver
            //this.CollisionSolver.Solve(this.RuleExecutionQueue);

            RuleCollection parallelEx = new RuleCollection();
            this._executedRules.Add(parallelEx);
            
            

            // Execute rules by new order
            foreach (RuleBase rule in this.RuleExecutionQueue)
            {
                rule.RulesEngine = this;
                var executionResult = RuleExecutor.ExecuteRule(this, rule, this.WorkingMemory);
                var exrule = (RuleTemplateBase)(executionResult.Result.ResultParameters["activity"]);
                if (exrule.State == ExecutionStateAcitivity.ExecutionState.running)
                {
                    _unfinishedRules.Add((RuleTemplateBase)(executionResult.Result.ResultParameters["activity"]));
                }
                parallelEx.Add(rule);
            }
            if (parallelEx.Count == 0)
            {
                StepInTime(parallelEx);
            }
        }


        public void FireAllRuleOneStep()
        {
            this.FireAllRules();
        }


        public bool FireAllRules(out List<RuleBase> executedRule)
        {
            executedRule = null;

            // Clear queue
            this.RuleExecutionQueue.Clear();

            // Get all rules to execute    

            var filteredRules = this.FilterEnabledRules().OrderByDescending(x=>x.Priority).ToList();

            

            filteredRules.ForEach(rule =>
            {
                bool ruleConditionExecutionResult = RuleExecutor.ExecuteRuleCondition(this, rule, this.WorkingMemory);
                
                if (ruleConditionExecutionResult)
                {
                    lock (this.RuleExecutionQueue)
                    {
                        this.RuleExecutionQueue.Add(rule);
                    }
                }
            });

            RuleCollection parallelEx = new RuleCollection();
            this._executedRules.Add(parallelEx);
            
            //var nonFinishedActivities = from i in this._executedRules from j in i where 
            // Determine rule execution order by solver
            //this.CollisionSolver.Solve(this.RuleExecutionQueue);


            // Execute rules by new order
            bool result = false;
            foreach (RuleBase rule in this.RuleExecutionQueue)
            {
                rule.RulesEngine = this;
                var executionResult = RuleExecutor.ExecuteRule(this, rule, this.WorkingMemory);
                var exrule = (RuleTemplateBase)(executionResult.Result.ResultParameters["activity"]);
                if (exrule.State == ExecutionStateAcitivity.ExecutionState.running)
                {
                    _unfinishedRules.Add((RuleTemplateBase)(executionResult.Result.ResultParameters["activity"]));
                }
                parallelEx.Add(rule);

                result= true;
            }
            executedRule = parallelEx;
            var step = false;
            if (parallelEx.Count == 0)
            {
                
                step = StepInTime(parallelEx);
            }

            return result || step;
        }


        public bool FireAllRulesOneStep(out List<RuleBase> executedRule)
        {
            return FireAllRules(out executedRule);
        }


        private List<RuleBase> FilterEnabledRules()
        {
            List<RuleBase> filteredRules = new List<RuleBase>();
            foreach (RuleBase rule in this.Rules)
            {
                if (rule.Enabled && (_unfinishedRules.Where(x=>x.Rule==rule).FirstOrDefault() == null ))
                {
                    filteredRules.Add(rule);
                }
            }
            return filteredRules;
        }

        private Dictionary<string,object> _plugins = new Dictionary<string, object>();

        public object GetPlugin(string name)
        {
            object plugin = null;
            _plugins.TryGetValue(name, out plugin);
            return plugin;
        }
        public void AddPlugin(string name, object plugin)
        {
            if (!_plugins.ContainsKey(name))
            {
                _plugins.Add(name, plugin);
            }
        }
        public void RemovePlugin(string name)
        {
            _plugins.Remove(name);
        }



        //private BBNGine _bbnEngine;

        //public BBNGine BbnEngine
        //{
        //    get { return _bbnEngine; }
        //}
        

        //public void LoadBBNGModel(string filename) {
        //    _bbnEngine = new BBNGine();
        //    _bbnEngine.LoadXesDocument(filename);
        //}
        

    }

    public class ExecutionStateAcitivity
    {
        public enum ExecutionState
        {
            init,
            started,
            running,
            finished
        }

        public  int ActionsToBeTaken = 0;
        public ExecutionState _state;

        public ExecutionState State
        {
            set
            {
                if (value == ExecutionState.finished && ActionsToBeTaken == 0)
                {
                    _state = ExecutionState.finished;
                }
            }
            get
            {
                return _state;
            }
        }

        public void ActionTaken()
        {
            ActionsToBeTaken--;
            State = ExecutionState.finished;
        }
    }
}
