using Agent;
using CommunicationServer;
using GameMaster;
using Messaging.Contracts.Agent;
using Messaging.Enumerators;
using Messaging.Implementation;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Linq;

namespace IntegrationTests
{
    public class IntegrationTests
    {
        int agentsInTeam = 1;
        const int agentSleepMs = 16;
        const int gameMasterSleepMs = 16;
        private GameMaster.GameMaster gameMaster;

        [SetUp]
        public void Setup()
        {
            gameMaster = new GameMaster.GameMaster(GameMasterConfiguration.GetDefault());
            agentsInTeam = gameMaster.Configuration.TeamSize;
        }

        [Test]
        public void ConnectingAgentsState_ShouldConnectAgent()
        {
            var csThread = CreateCsThread();
            var gmThread = CreateGmThread();

            csThread.Start();
            gmThread.Start();

            gameMaster.ApplyConfiguration();

            var agents = CreateAgents();
            foreach (var agent in agents)
            {
                var agentThread = new Thread(RunAgent);
                agentThread.IsBackground = true; //background threads for termination at test exit
                agentThread.Start(agent);
            }

            gmThread.Join();

            List<GameMaster.Agent> lobby = gameMaster.ConnectionLogic.FlushLobby();

            Assert.AreEqual(agentsInTeam * 2, lobby.Count);
            Assert.AreEqual(2, lobby.Where(agent => agent.IsTeamLeader).Count());
            Assert.AreEqual(agentsInTeam, lobby.Where(agent => agent.Team == TeamId.Blue).Count());
            Assert.AreEqual(agentsInTeam, lobby.Where(agent => agent.Team == TeamId.Red).Count());
        }

        private Thread CreateGmThread()
        {
            var gmThread = new Thread(() =>
            {
                RunGameMaster();
            });

            return gmThread;
        }
        
        private Thread CreateCsThread()
        {
            var csThread = new Thread(() =>
            {
                var csConfig = CommunicationServerConfiguration.GetDefault();
                var server = new CommunicationServer.CommunicationServer(csConfig);
                server.Run();
            });

            csThread.IsBackground = true;
            return csThread;
        }

        private List<Agent.Agent> CreateAgents()
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

        private void RunAgent(object o)
        {
            var agent = o as Agent.Agent;
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

        private void RunGameMaster()
        {
            for (int i = 0; i < 200; i++)
            {
                gameMaster.Update(gameMasterSleepMs / 1000.0);
                Thread.Sleep(gameMasterSleepMs);
            }
        }
    }
}