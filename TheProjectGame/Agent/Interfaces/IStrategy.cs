using Agent.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agent
{
    interface IStrategy
    {
        ActionResult MakeDecision(Agent agent);
        ActionResult MakeForcedDecision(Agent agent, SpecificActionType action, int argument = -1);
    }
}
