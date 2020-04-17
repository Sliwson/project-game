using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agent
{
    public class AgentInformationsComponent
    {
        private Agent agent;

        public int LastAskedTeammate;
        public Direction LastDirection { get; set; }
        public bool DeniedLastMove { get; set; }
        public int SkipCount { get; set; }
        public double RemainingPenalty { get; set; }


        public AgentInformationsComponent(Agent agent)
        {
            this.agent = agent;
            LastAskedTeammate = 0;
            DeniedLastMove = false;
            SkipCount = 0;
            RemainingPenalty = 0.0;
        }
    }
}
