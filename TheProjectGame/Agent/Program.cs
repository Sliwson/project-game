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
            stopwatch.Start();

            ActionResult actionResult = ActionResult.Continue;
            while (actionResult == ActionResult.Continue)
            {
                stopwatch.Stop();
                var timeElapsed = stopwatch.Elapsed.TotalSeconds;
                stopwatch.Reset();
                stopwatch.Start();

                actionResult = Agent.Update(timeElapsed);
                Thread.Sleep(updateInterval);
            }

            Agent.OnDestroy();
        }
    }
}
