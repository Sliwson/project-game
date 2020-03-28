using Agent.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agent
{
    class MockConfigurationProvider : IConfigurationProvider
    {
        public AgentConfiguration GetConfiguration()
        {
            return new AgentConfiguration
            {
                MovePenalty = new TimeSpan(1500),
                AskPenalty = new TimeSpan(1000),
                DiscoveryPenalty = new TimeSpan(700),
                PutPenalty = new TimeSpan(500),
                CheckForShamPenalty = new TimeSpan(1000),
                ResponsePenalty = new TimeSpan(1000),
                BoardX = 40,
                BoardY = 40,
                GoalAreaHeight = 5,
                AgentsLimit = 5,
                DestroyPiecePenalty = new TimeSpan(700)
            };
        }
    }
}
