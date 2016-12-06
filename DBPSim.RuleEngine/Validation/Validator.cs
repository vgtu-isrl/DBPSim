using DBPSim.RuleEngine.Execution;
using DBPSim.RuleEngine.Translator;
using System;
using System.CodeDom.Compiler;

namespace DBPSim.RuleEngine.Validation
{
    public class Validator
    {

        public static bool ValidateRuleSyntax(RuleBase rule, out CompilerErrorCollection errors)
        {
            bool isValid = true;
            errors = null;
            string fullRule = RuleTranslator.Translate(rule);

            try
            {
                CompilerResults compilerResults = RuleExecutor.CompileToAssembly(fullRule);
                if (compilerResults.Errors.Count > 0)
                {
                    isValid = false;
                    errors = compilerResults.Errors;
                }
            }
            catch (Exception)
            {
                isValid = false;
            }

            return isValid;
        }

    }
}
