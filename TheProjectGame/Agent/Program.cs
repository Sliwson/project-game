using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Agent
{
    class Program
    {
        private const int updateInterval = 10;

        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                var agents = new Agent[] {
                    CreateAgentWithConfiguration()
                };

                Run(agents);
            }
            else if (args.Length == 3)
            {
                int blue = 1, red = 0;
                try
                {
                    blue = int.Parse(args[1]);
                    red = int.Parse(args[1]);
                }
                catch
                {
                    Console.WriteLine($"Usage: {args[0]} (blueCount, redCount)");
                    Console.WriteLine("Creating one blue agent with default configuration");
                }

                var agents = CreateDefaultAgents(blue, red);
                Run(agents);
            }
            else
            {
                Console.WriteLine($"Usage: {args[0]} (blueCount, redCount)");
            }
        }

        private static Agent CreateAgentWithConfiguration()
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

        private static Agent[] CreateDefaultAgents(int blue, int red)
        {
        }

        private static void Run(Agent[] agents)
        {
            var length = agents.Length;
            var shouldUpdate = new bool[length];
            var stopwatches = new Stopwatch[length];

            for (int i = 0; i < length; i++)
            {
                shouldUpdate[i] = true;
                stopwatches[i] = new Stopwatch();
                stopwatches[i].Start();
            }

            while (shouldUpdate.Any())
            {
                for (int i = 0; i < agents.Length; i++)
                {
                    if (!shouldUpdate[i])
                        continue;

                    var stopwatch = stopwatches[i];
                    stopwatch.Stop();
                    var timeElapsed = stopwatch.Elapsed.TotalSeconds;
                    stopwatch.Reset();
                    stopwatch.Start();

                    var actionResult = agents[i].Update(timeElapsed);
                    if (actionResult == ActionResult.Finish)
                    {
                        shouldUpdate[i] = false;
                        agents[i].OnDestroy();
                    }
                }
            }
        }
    }
}
