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
            var csThread = IntegrationTestsHelper.CreateCsThread();
            var gmThread = IntegrationTestsHelper.CreateGmThread(gameMaster, gameMasterSleepMs);

            csThread.Start();
            gmThread.Start();

            gameMaster.ApplyConfiguration();

            var agents = IntegrationTestsHelper.CreateAgents(agentsInTeam);
            foreach (var agent in agents)
            {
                var agentThread = new Thread(() => IntegrationTestsHelper.RunAgent(agent, agentSleepMs));
                agentThread.IsBackground = true; //background threads for termination at test exit
                agentThread.Start();
            }

            gmThread.Join();

            List<GameMaster.Agent> lobby = gameMaster.ConnectionLogic.FlushLobby();

            Assert.AreEqual(agentsInTeam * 2, lobby.Count);
            Assert.AreEqual(2, lobby.Where(agent => agent.IsTeamLeader).Count());
            Assert.AreEqual(agentsInTeam, lobby.Where(agent => agent.Team == TeamId.Blue).Count());
            Assert.AreEqual(agentsInTeam, lobby.Where(agent => agent.Team == TeamId.Red).Count());
        }
    }
}