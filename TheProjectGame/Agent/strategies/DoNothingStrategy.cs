using Agent.Enums;

namespace Agent.strategies
{
    public class DoNothingStrategy : IStrategy
    {
        public ActionResult MakeDecision(Agent agent)
        {
            return ActionResult.Continue;
        }

        public ActionResult MakeForcedDecision(Agent agent, SpecificActionType action, int argument = -1)
        {
            return ActionResult.Continue;
        }
    }
}
