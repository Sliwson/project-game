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
        public static AgentConfiguration Configuration { get; private set; }
        public static Agent agent { get; set; }
        private const int updateInterval = 10;
        static void Main(string[] args)
        {
            CreateAgent();
        }

        private static void LoadDefaultConfiguration()
        {
            Configuration = AgentConfiguration.GetConfiguration();
        }
     
        private static void CreateAgent()
        {
            LoadDefaultConfiguration();
            agent = new Agent(Configuration.teamID.ToLower() == "red" ? TeamId.Red : TeamId.Blue, Configuration.wantsToBeTeamLeader);
            agent.AgentConfiguration = Configuration;
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
