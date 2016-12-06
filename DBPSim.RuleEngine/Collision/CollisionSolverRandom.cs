using System;

namespace DBPSim.RuleEngine.Collision
{
    public class CollisionSolverRandom : CollisionSolverBase
    {

        public override void Solve(RuleCollection rules)
        {
            Random rng = new Random();
            int n = rules.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                RuleBase value = rules[k];
                rules[k] = rules[n];
                rules[n] = value;
            }
        }

    }
}
