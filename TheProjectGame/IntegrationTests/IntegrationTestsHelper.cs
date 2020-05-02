using Agent;
using GameMaster;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace IntegrationTests
{
    public static class IntegrationTestsHelper
    {
        public static Thread CreateGmThread(GameMaster.GameMaster gameMaster, int gameMasterSleepMs)
        {
            var gmThread = new Thread(() =>
            {
                RunGameMaster(gameMaster, gameMasterSleepMs);
            });

            return gmThread;
        }

        public static Thread CreateCsThread()
        {
            var csThread = new Thread(() =>
            {
                CommunicationServer.CommunicationServer server = new CommunicationServer.CommunicationServer();
                server.Run();
            });

            csThread.IsBackground = true;
            return csThread;
        }

        public static List<Agent.Agent> CreateAgents(int agentsInTeam)
        {
            var agents = new List<Agent.Agent>();
            for (int i = 0; i < agentsInTeam * 2; i++)
            {
                var agent = new Agent.Agent(new AgentConfiguration
                {
                    CsIP = "127.0.0.1",
                    CsPort = 54321,
                    TeamID = i < agentsInTeam ? "red" : "blue",
                    WantsToBeTeamLeader = i % agentsInTeam == 0
                });

                agents.Add(agent);
            }

            return agents;
        }

        public static void RunAgent(Agent.Agent agent, int agentSleepMs)
        {
            agent.ConnectToCommunicationServer();

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
                Thread.Sleep(agentSleepMs);
            }

            agent.OnDestroy();
        }

        public static void RunGameMaster(GameMaster.GameMaster gameMaster, int gameMasterSleepMs)
        {
            for (int i = 0; i < 200; i++)
            {
                gameMaster.Update(gameMasterSleepMs / 1000.0);
                Thread.Sleep(gameMasterSleepMs);
            }
        }
    }
}
