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
        private const int updateInterval = 10;

        public static AgentConfiguration Configuration { get; private set; }
        public static Agent Agent { get; set; }

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
            Agent = new Agent(Configuration);
            Stopwatch stopwatch = new Stopwatch();
            double timeElapsed = 0.0;
            ActionResult actionResult = ActionResult.Continue;
            while (actionResult == ActionResult.Continue)
            {
                stopwatch.Start();
                Thread.Sleep(updateInterval);
                actionResult = Agent.Update(timeElapsed);
                stopwatch.Stop();
                timeElapsed = stopwatch.Elapsed.TotalSeconds;
            }

            Agent.OnDestroy();
        }
    }
}
