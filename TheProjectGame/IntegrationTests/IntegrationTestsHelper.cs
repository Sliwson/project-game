using Agent;
using CommunicationServer;
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

        public static void RunGameMaster(GameMaster.GameMaster gameMaster, int gameMasterSleepMs)
        {
            for (int i = 0; i < 300; i++)
            {
                gameMaster.Update(gameMasterSleepMs / 1000.0);
                Thread.Sleep(gameMasterSleepMs);
            }
        }

        public static void RunGameMaster(object state)
        {
            var gameMasterState = (GameMasterTaskState)state;
            RunGameMaster(gameMasterState.GameMaster, gameMasterState.GameMasterSleepMs);
        }

        public static Thread CreateCsThread()
        {
            var csThread = new Thread(() =>
            {
                RunCommunicationServer(null);
            });

            csThread.IsBackground = true;
            return csThread;
        }

        public static void RunCommunicationServer(object state)
        {
            var config = state == null ? CommunicationServerConfiguration.GetDefault() : (CommunicationServerConfiguration)state;
            CommunicationServer.CommunicationServer server = new CommunicationServer.CommunicationServer(config);

            try
            {
                server.Run();
            }
            catch
            {
                server.OnDestroy();
                throw;
            }
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

        public static void RunAgent(object state)
        {
            var agentState = (AgentTaskState)state;
            RunAgent(agentState.Agent, agentState.AgentSleepMs);
        }

        public class GameMasterTaskState
        {
            public GameMaster.GameMaster GameMaster { get; set; }
            public int GameMasterSleepMs { get; set; }

            public GameMasterTaskState(GameMaster.GameMaster gameMaster, int gameMasterSleepMs)
            {
                GameMaster = gameMaster;
                GameMasterSleepMs = gameMasterSleepMs;
            }
        }

        public class AgentTaskState
        {
            public Agent.Agent Agent { get; set; }
            public int AgentSleepMs { get; set; }

            public AgentTaskState(Agent.Agent agent, int agentSleepMs)
            {
                Agent = agent;
                AgentSleepMs = agentSleepMs;
            }
        }
    }
}
