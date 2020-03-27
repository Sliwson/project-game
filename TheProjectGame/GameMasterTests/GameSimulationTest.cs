using NUnit.Framework;
using GameMaster;
using System;
using System.Collections.Generic;
using System.Text;
using Messaging.Contracts;
using Messaging.Contracts.GameMaster;
using Messaging.Contracts.Agent;
using Messaging.Contracts.Errors;
using Messaging.Implementation;
using Messaging.Enumerators;
using System.Drawing;
using System.Linq;

namespace GameMasterTests
{
    public class GameplaySimulationTest
    {
        private GameMaster.GameMaster gameMaster;
        private GameMasterConfiguration config;

        [SetUp]
        public void Setup()
        {
            gameMaster = new GameMaster.GameMaster();
            config = gameMaster.Configuration;
        }

        [Test]
        public void Simulation()
        {
            gameMaster.ApplyConfiguration();
            CreateAndConnectAgents(); 
        }

        private void CreateAndConnectAgents()
        {
            int limit = config.AgentsLimit;
            var agentMessages = new List<BaseMessage>();

            for (int i = 0; i < limit * 2; i++)
                agentMessages.Add(MessageFactory.GetMessage(new JoinRequest(
                    i < limit ? TeamId.Blue : TeamId.Red,
                    i % limit == 0
                    ), i));

            foreach (var m in agentMessages)
                gameMaster.InjectMessage(m);

            gameMaster.Update(0);
            gameMaster.StartGame();
            var agents = gameMaster.Agents;

            Assert.AreEqual(2 * limit, agents.Count);
            Assert.AreEqual(2, agents.Where(a => a.IsTeamLeader == true).Count());
            Assert.AreEqual(limit, agents.Where(a => a.Team == TeamId.Blue).Count());

            for (int i = 0; i < 2 * limit; i++)
                Assert.AreEqual(i, agents[i].Id);
        }
    }
}


