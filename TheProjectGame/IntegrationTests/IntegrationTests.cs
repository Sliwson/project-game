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

namespace IntegrationTests
{
    public class IntegrationTests
    {
        private static string csConfigFilePath = @"communicationServerConfig.json";
        const int agentsInTeam = 1;
        const int agentSleepMs = 16;
        private GameMaster.GameMaster gameMaster;
        private GameMasterConfiguration config;
        private ConnectionLogicComponent connectionLogic;

        [SetUp]
        public void Setup()
        {
            gameMaster = new GameMaster.GameMaster();
            config = gameMaster.Configuration;
            connectionLogic = gameMaster.ConnectionLogic;
        }


        [Test]
        public void IntegrationTest()
        {
            var csThread = CreateCsThread();
            var gmThread = CreateGmThread();

            csThread.Start();
            gmThread.Start();

            Thread.Sleep(3000); //time for connecting gm with cs

            var agents = CreateAgents();
            foreach (var agent in agents)
            {
                var agentThread = new Thread(RunAgent);
                agentThread.IsBackground = true; //background threads for termination at test exit
                agentThread.Start(agent);
            }

            gmThread.Join();
        }
        
        private Thread CreateGmThread()
        {
            var gmThread = new Thread(() =>
            {
                Play();
            });

            gmThread.SetApartmentState(ApartmentState.STA);
            return gmThread;
        }
        
        private Thread CreateCsThread()
        {
            var csThread = new Thread(() =>
            {
                CommunicationServer.CommunicationServer server = new CommunicationServer.CommunicationServer(csConfigFilePath);
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


        private void Play()
        {

            gameMaster.Update(1.0);

        }
    }
}