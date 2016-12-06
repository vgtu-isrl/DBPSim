using Microsoft.VisualBasic;
using DBPSim.RuleEngine.Memory;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace DBPSim.RuleEngine.Execution
{
    public class RuleExecutor
    {

        private static string _typeName = "RuleTemplate";
        private static Dictionary<string, string> _providerOptions = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };


        private static ExecutionResult Execute(RulesEngine rulesEngine, RuleBase rule, WorkingMemory workingMemory, Assembly assembly)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            if (workingMemory == null)
            {
                throw new ArgumentNullException("workingMemory");
            }
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            ExecutionResult executionResult = new ExecutionResult();
            // Create instance
            Type type = assembly.GetType(RuleExecutor._typeName);

            object obj = Activator.CreateInstance(type, rulesEngine,workingMemory, rule);

            // Get method to execute
            MethodInfo methodInfo = type.GetMethod("Execute");

            try
            {
                executionResult.Result = (RuleExecutionResult)methodInfo.Invoke(obj, null);
            }
            catch (Exception ex)
            {
                // Remember exception reference
                executionResult.ExternalException = ex.InnerException;

                // Force stop check and stop if exception is force stop type.
                // Exceptions from assembly always throws error "Exception has been thrown by the target of an invocation."
                // Inner exception is actual exception from assembly execution
                if (ex.InnerException is ForceExecutionStopException)
                {
                    throw ex.InnerException;
                }
                if (ex.InnerException is EndExecutionException)
                {
                    // Do nothing
                }
            }
            return executionResult;
        }


        private static bool ExecuteCondition(RulesEngine rulesEngine, RuleBase rule, WorkingMemory workingMemory, Assembly assembly)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            if (workingMemory == null)
            {
                throw new ArgumentNullException("workingMemory");
            }
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            bool executionResult = false;

            // Create instance
            Type type = assembly.GetType(RuleExecutor._typeName);            

            object obj = Activator.CreateInstance(type,rulesEngine, workingMemory, rule);

            // Get method to execute
            MethodInfo methodInfo = type.GetMethod("ExecuteCondition");

            try
            {
                executionResult = (bool)methodInfo.Invoke(obj, null);
            }
            catch (Exception ex)
            {
                // Remember exception reference
                //executionResult.ExternalException = ex.InnerException;

                // Force stop check and stop if exception is force stop type.
                // Exceptions from assembly always throws error "Exception has been thrown by the target of an invocation."
                // Inner exception is actual exception from assembly execution
                if (ex.InnerException is ForceExecutionStopException)
                {
                    throw ex.InnerException;
                }
            }
            return executionResult;
        }


        public static ExecutionResult CompileAndExecute(RulesEngine rulesEngine, RuleBase rule, WorkingMemory workingMemory, out Assembly compiledAssembly)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            if (workingMemory == null)
            {
                throw new ArgumentNullException("workingMemory");
            }

            compiledAssembly = null;
            ExecutionResult executionResult = new ExecutionResult();

            CompilerResults compilerResult = RuleExecutor.CompileToAssembly(rule.FullRule);

            executionResult.Errors = compilerResult.Errors;

            // When Assembly was not compiled CompilerResults object's CompiledAssembly property throws FileNotFoundException
            if (executionResult.Errors.Count == 0)
            {
                executionResult = RuleExecutor.Execute(rulesEngine, rule, workingMemory, compilerResult.CompiledAssembly);
            }

            return executionResult;
        }


        public static CompilerResults CompileToAssembly(string fullRule)
        {
            if (string.IsNullOrEmpty(fullRule))
            {
                throw new ArgumentNullException("fullRule");
            }

            CompilerResults compilerResults = null;
            using (VBCodeProvider provider = new VBCodeProvider(RuleExecutor._providerOptions))
            {
                // Compiler params
                CompilerParameters compilerParams = new CompilerParameters
                {
                    GenerateInMemory = true,
                    GenerateExecutable = false,
                };

                // References
                compilerParams.ReferencedAssemblies.Add("mscorlib.dll");
                compilerParams.ReferencedAssemblies.Add("System.dll");
                compilerParams.ReferencedAssemblies.Add("System.Core.dll");
                compilerParams.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
                // Must add reference to self
                compilerParams.ReferencedAssemblies.Add("DBPSim.RuleEngine.dll");
                compilerParams.ReferencedAssemblies.Add(Assembly.GetEntryAssembly().ManifestModule.Name);

                compilerParams.IncludeDebugInformation = true;
                compilerParams.GenerateInMemory = false; //default
                compilerParams.TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true);

                compilerResults = provider.CompileAssemblyFromSource(compilerParams, fullRule);
            }
            return compilerResults;
        }


        public static bool ExecuteRuleCondition(RulesEngine rulesEngine, RuleBase rule, WorkingMemory memory)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            if (memory == null)
            {
                throw new ArgumentNullException("memory");
            }
            bool conditionResult = false;
            if (rule.Assembly == null || rule.Changed)
            {                
                CompilerResults compilerResult = RuleExecutor.CompileToAssembly(rule.FullRule);
                if (compilerResult.Errors.Count == 0)
                {
                    rule.Assembly = compilerResult.CompiledAssembly;
                }
                else
                {
                    throw new Exception("Rule " + rule.Id + " '" + rule.Title + "' can't compile.");
                }
                conditionResult = RuleExecutor.ExecuteCondition(rulesEngine, rule, memory, rule.Assembly);
                rule.Changed = false;
            }
            else
            {
                conditionResult = RuleExecutor.ExecuteCondition(rulesEngine, rule, memory, rule.Assembly);
            }
            return conditionResult; ;
        }


        public static ExecutionResult ExecuteRule(RulesEngine rulesEngine, RuleBase rule, WorkingMemory memory)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            if (memory == null)
            {
                throw new ArgumentNullException("memory");
            }
            ExecutionResult result = null;
            if (rule.Assembly == null || rule.Changed)
            {
                Assembly assembly = null;
                result = RuleExecutor.CompileAndExecute(rulesEngine, rule, memory, out assembly);
                rule.Assembly = assembly;
                rule.Changed = false;
            }
            else
            {
                result = RuleExecutor.Execute(rulesEngine, rule, memory, rule.Assembly);
            }
            if (result != null)
            {
                rule.ExecutionResultCollection.Add(result);
            }
            return result;
        }

    }
}
