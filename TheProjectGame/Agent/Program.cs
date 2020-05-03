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

        static void Main(string[] args)
        {
            var agent = CreateAgent();
            Run(agent);
        }

        private static Agent CreateAgent()
        {
            var config = AgentConfiguration.GetDefault();

            Console.WriteLine("Do you want to load configuration from file? [Y]/[*]");
            ConsoleKey key = Console.ReadKey().Key;
            Console.WriteLine();

            if (key == ConsoleKey.Y)
            {
                Console.WriteLine("Enter path: ");
                var line = Console.ReadLine();
                var newConfig = AgentConfiguration.LoadFromFile(line);
                if (newConfig != null)
                    config = newConfig;
            }

            return new Agent(config);
        }

        private static void Run(Agent agent)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            ActionResult actionResult = ActionResult.Continue;
            while (actionResult == ActionResult.Continue)
            {
                stopwatch.Stop();
                var timeElapsed = stopwatch.Elapsed.TotalSeconds;
                stopwatch.Reset();
                stopwatch.Start();

                actionResult = agent.Update(timeElapsed);
                Thread.Sleep(updateInterval);
            }

            agent.OnDestroy();
        }
    }
}
