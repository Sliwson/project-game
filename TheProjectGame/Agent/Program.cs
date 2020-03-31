using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Agent
{
    class Program
    {
        public AgentConfiguration Configuration { get; private set; }
        public static Agent agent { get; set; }
        private int numberOfAgents { get; set; }
        static void Main(string[] args)
        {
            CreateAgent();
        }

        private static AgentConfiguration LoadDefaultConfiguration()
        {
            AgentConfiguration agentConfiguration = new AgentConfiguration();
            return agentConfiguration.GetConfiguration();
        }
     
        public static void CreateAgent()
        {
            agent = new Agent();
            AgentConfiguration agentConfiguration = LoadDefaultConfiguration();
            agent.CsIP = agentConfiguration.CsIP;
            agent.CsPort = agentConfiguration.CsPort;
            agent.team = agentConfiguration.teamID == "red" ? TeamId.Red : TeamId.Blue;
            agent.JoinTheGame();
        }
    }
}
