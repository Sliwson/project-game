using System;
using System.Collections.Generic;
using System.Text;

namespace Agent
{
    interface IStrategy
    {
        ActionResult MakeDecision(Agent agent);
    }
}
