using System;
using System.Collections.Generic;
using System.Text;

namespace Agent.strategies
{
    public class DoNothingStrategy : IStrategy
    {
        public ActionResult MakeDecision(Agent agent)
        {
            return ActionResult.Continue;
        }
    }
}
