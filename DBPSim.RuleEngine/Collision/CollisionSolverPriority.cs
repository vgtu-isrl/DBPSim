using System.Linq;

namespace DBPSim.RuleEngine.Collision
{
    public class CollisionSolverPriority : CollisionSolverBase
    {

        public override void Solve(RuleCollection rules)
        {
            rules.Sort(delegate(RuleBase rule1, RuleBase rule2)
            {
                if (rule1.Priority != null && rule2.Priority != null)
                {
                    return rule1.Priority.Value.CompareTo(rule2.Priority.Value);
                }
                return 0;
            });
        }

    }
}
