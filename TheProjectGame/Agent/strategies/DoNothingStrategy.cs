using System;
using System.Collections.Generic;
using System.Text;

namespace Agent.strategies
{
    public class DoNothingStrategy : IStrategy
    {
        public bool MakeDecision(Agent agent)
        {
            return false;
        }
    }
}
