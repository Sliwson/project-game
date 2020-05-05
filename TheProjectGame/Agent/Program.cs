﻿using Messaging.Enumerators;
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
            if (args.Length == 0)
            {
                var agents = new Agent[] {
                    CreateAgentWithConfiguration()
                };

                Run(agents);
            }
            else if (args.Length == 2)
            {
                int blue = 0, red = 0;
                try
                {
                    blue = int.Parse(args[0]);
                    red = int.Parse(args[1]);

                    if (blue < 0 || red < 0 || (blue == 0 && red == 0))
                        throw new ArgumentException();
                }
                catch
                {
                    Console.WriteLine($"Usage: ./Agent.exe (blueCount, redCount)");
                    Console.WriteLine("Creating one blue agent with default configuration");
                    blue = 1;
                    red = 0;
                }

                var agents = CreateDefaultAgents(blue, red);
                Run(agents);
            }
            else
            {
                Console.WriteLine($"Usage: ./Agent.exe (blueCount, redCount)");
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
            var agents = new Agent[blue + red];
            for (int i = 0; i < blue; i++)
            {
                var config = AgentConfiguration.GetDefault();
                config.TeamID = "blue";
                agents[i] = new Agent(config);
            }

            for (int i = 0; i < red; i++)
            {
                var config = AgentConfiguration.GetDefault();
                config.TeamID = "red";
                agents[blue + i] = new Agent(config);
            }

            return agents;
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

                try
                {
                    agents[i].ConnectToCommunicationServer();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Agent {i}: {ex.Message}");
                    shouldUpdate[i] = false;
                }
            }

            while (shouldUpdate.Any(b => b == true))
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
                    ActionResult actionResult = ActionResult.Finish;

                    try
                    {
                        actionResult = agents[i].Update(timeElapsed);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Agent {i}: {ex.Message}");
                    }

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
