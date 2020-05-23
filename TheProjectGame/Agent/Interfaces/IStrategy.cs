using Agent.Enums;

namespace Agent
{
    interface IStrategy
    {
        ActionResult MakeDecision(Agent agent);
        ActionResult MakeForcedDecision(Agent agent, SpecificActionType action, int argument = -1);
    }
}
