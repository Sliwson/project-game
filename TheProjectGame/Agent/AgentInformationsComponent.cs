using Messaging.Contracts;
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
        public BaseMessage LastMessage { get; set; }
        public double LastPenalty { get; set; }
        public bool DeniedLastMove { get; set; }
        public bool DeniedLastRequest { get; set; }
        public TimeSpan SkipTime { get; set; }
        public double RemainingPenalty { get; set; }

        public AgentInformationsComponent(Agent agent)
        {
            this.agent = agent;
            LastAskedTeammate = 0;
            LastPenalty = 0.0;
            DeniedLastMove = false;
            DeniedLastRequest = false;
            SkipTime = TimeSpan.Zero;
            RemainingPenalty = 0.0;
        }
    }
}
