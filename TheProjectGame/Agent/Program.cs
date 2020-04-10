using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Agent
{
    class Program
    {
        public AgentConfiguration Configuration { get; private set; }
        public static Agent agent { get; set; }
        private const int updateInterval = 10;
        static void Main(string[] args)
        {
            CreateAgent();
        }

        private static AgentConfiguration LoadDefaultConfiguration()
        {
            AgentConfiguration agentConfiguration = new AgentConfiguration();
            return agentConfiguration.GetConfiguration();
        }
     
        private static void CreateAgent()
        {
            agent = new Agent();
            AgentConfiguration agentConfiguration = LoadDefaultConfiguration();
            agent.CsIP = agentConfiguration.CsIP;
            agent.CsPort = agentConfiguration.CsPort;
            agent.team = agentConfiguration.teamID == "red" ? TeamId.Red : TeamId.Blue;
            Stopwatch stopwatch = new Stopwatch();
            double timeElapsed = 0.0;
            ActionResult actionResult = ActionResult.Continue;
            while (actionResult == ActionResult.Continue)
            {
                stopwatch.Start();
                Thread.Sleep(updateInterval);
                actionResult = agent.Update(timeElapsed);
                stopwatch.Stop();
                timeElapsed = stopwatch.Elapsed.TotalSeconds;
            }
        }
    }
}
