using System;
using DBPSim.RuleEngine;
using DBPSim.RuleEngine.Translator;
using DBPSim.RuleEngine.Validation;
using System.CodeDom.Compiler;
using System.Text;
using System.Windows;
using DBPSim.Simulation;

namespace DBPSim.SimulationGUI.Windows
{
    /// <summary>
    /// Interaction logic for RuleEditor.xaml
    /// </summary>
    public partial class RuleEditor : Window
    {

        private RuleConditional _editingRule = null;


        public RuleEditor()
        {
            InitializeComponent();
        }


        public RuleEditor(RuleConditional rule)
        {
            InitializeComponent();

            this._editingRule = rule;

            this.TextBoxRuleId.Text = rule.Id;
            this.TextBoxRuleName.Text = rule.Title;
            this.TextBoxRulePriority.Text = rule.Priority != null ? rule.Priority.Value.ToString() : null;
            this.TextBoxRuleCondition.Text = rule.Condition;
            this.TextBoxRuleBody.Text = rule.Body;
        }


        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.TextBoxRuleId.Text.Trim()))
            {
                MessageBox.Show("Rule id must be not empty.");
                return;
            }

            if (string.IsNullOrEmpty(this.TextBoxRulePriority.Text.Trim()))
            {
                MessageBox.Show("Rule priority must be not empty.");
                return;
            }

            if (string.IsNullOrEmpty(this.TextBoxRuleName.Text.Trim()))
            {
                MessageBox.Show("Rule title must be not empty.");
                return;
            }

            if (string.IsNullOrEmpty(this.TextBoxRuleCondition.Text.Trim()))
            {
                MessageBox.Show("Rule condition must be not empty.");
                return;
            }

            if (string.IsNullOrEmpty(this.TextBoxRuleBody.Text.Trim()))
            {
                MessageBox.Show("Rule body must be not empty.");
                return;
            }

            RuleConditional rule = this.GetRule();

            CompilerErrorCollection compilerErrors;
            if (!Validator.ValidateRuleSyntax(rule, out compilerErrors))
            {
                MessageBox.Show("Wrong rule syntax! Please validate before adding to rule database.");
                return;
            }

            if (this._editingRule == null)
            {
                if (Context.Rules.Exists(item => item.Id == this.TextBoxRuleId.Text.Trim()))
                {
                    MessageBox.Show(string.Format("Rule with id '{0}' already exists.", rule.Id));
                    return;
                }
                Context.Rules.Add(rule);
            }
            else
            {
                if (!this._editingRule.Id.Equals(this.TextBoxRuleId.Text.Trim(), StringComparison.InvariantCultureIgnoreCase) && Context.Rules.Exists(item => item.Id == this.TextBoxRuleId.Text.Trim()))
                {
                    MessageBox.Show(string.Format("Rule with id '{0}' already exists.", rule.Id));
                    return;
                }
                this._editingRule.Id = this.TextBoxRuleId.Text;
                this._editingRule.Title = this.TextBoxRuleName.Text;
                this._editingRule.Priority = int.Parse(this.TextBoxRulePriority.Text);
                this._editingRule.Condition = this.TextBoxRuleCondition.Text;
                this._editingRule.Body = this.TextBoxRuleBody.Text;
            }

            this.Close();
        }


        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void ButtonValidate_Click(object sender, RoutedEventArgs e)
        {
            CompilerErrorCollection compilerErrors;
            if (!Validator.ValidateRuleSyntax(this.GetRule(), out compilerErrors))
            {
                StringBuilder sb = new StringBuilder();
                foreach (CompilerError compilerError in compilerErrors)
                {
                    sb.AppendFormat("Error number: {0}", compilerError.ErrorNumber);
                    sb.AppendLine();
                    sb.AppendFormat("Line: {0}", compilerError.Line);
                    sb.AppendLine();
                    sb.AppendFormat("Text: {0}", compilerError.ErrorText);
                    sb.AppendLine();
                    sb.Append("---------------------------------------------------------");
                    sb.AppendLine();
                }
                this.TextBlockValidationResult.Text = sb.ToString();
            }
            else
            {
                this.TextBlockValidationResult.Text = "All validation rules PASSED.";
            }
        }


        private void ButtonShowTranslation_Click(object sender, RoutedEventArgs e)
        {
            RuleConditional rule = this.GetRule();
            string translatedRule = RuleTranslator.Translate(rule);

            FullRule dialog = new FullRule(translatedRule);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.ShowDialog();
        }


        private RuleConditional GetRule()
        {
            RuleConditional rule = new RuleConditional();

            rule.Id = this.TextBoxRuleId.Text;
            rule.Title = this.TextBoxRuleName.Text;
            rule.Condition = this.TextBoxRuleCondition.Text;
            rule.Body = this.TextBoxRuleBody.Text;
            int rulePriority;
            if (int.TryParse(this.TextBoxRulePriority.Text, out rulePriority))
            {
                rule.Priority = int.Parse(this.TextBoxRulePriority.Text);
            }
            return rule;
        }


        private void TextBoxRulePriority_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            int result;
            if (!int.TryParse(e.Text, out result))
            {
                e.Handled = true;
            }
        }

    }
}
