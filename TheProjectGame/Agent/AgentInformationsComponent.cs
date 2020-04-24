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
        public bool Discovered { get; set; }
        public bool IsComingBack { get; set; }
        public int DidNotAskCount { get; set; }
        public Direction DirectionEastWest { get; set; }
        public int StayInLineCount { get; set; }
        public TimeSpan SkipTime { get; set; }
        public double RemainingPenalty { get; set; }

        public AgentInformationsComponent(Agent agent)
        {
            this.agent = agent;
            LastAskedTeammate = 0;
            LastPenalty = 0.0;
            DeniedLastMove = false;
            DeniedLastRequest = false;
            Discovered = false;
            IsComingBack = false;
            DidNotAskCount = 0;
            DirectionEastWest = Direction.East;
            StayInLineCount = 0;
            SkipTime = TimeSpan.Zero;
            RemainingPenalty = 0.0;
        }
    }
}
