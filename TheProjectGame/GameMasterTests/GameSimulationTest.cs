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
        private Random random = new Random();

        [SetUp]
        public void Setup()
        {
            gameMaster = new GameMaster.GameMaster();
            config = gameMaster.Configuration;
        }

        [Test]
        public void SimpleSimulationTest_GameMasterShouldConnectAgentsAndProcessMoveRequestsAfterGameStart()
        {
            gameMaster.ApplyConfiguration();
            CreateAndConnectAgents();
            Play();
        }

        private void CreateAndConnectAgents()
        {
            int limit = config.AgentsLimit;
            var agentMessages = new List<BaseMessage>();

            for (int i = 0; i < limit * 2; i++)
                gameMaster.InjectMessage(MessageFactory.GetMessage(new JoinRequest(
                    i < limit ? TeamId.Blue : TeamId.Red,
                    i % limit == 0
                    ), i));


            gameMaster.Update(0);
            gameMaster.StartGame();
            var agents = gameMaster.Agents;

            Assert.AreEqual(2 * limit, agents.Count);
            Assert.AreEqual(2, agents.Where(a => a.IsTeamLeader == true).Count());
            Assert.AreEqual(limit, agents.Where(a => a.Team == TeamId.Blue).Count());
            for (int i = 0; i < 2 * limit; i++)
                Assert.AreEqual(i, agents[i].Id);
        }

        private void Play()
        {
            var agents = gameMaster.Agents;
            bool[] hasMoved = new bool[agents.Count];
            Point[] previousPositions = new Point[agents.Count];

            //simulate 100 move requests for each agent
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < agents.Count; j++)
                    previousPositions[j] = agents[j].Position;

                foreach (var a in agents)
                    gameMaster.InjectMessage(CreateRandomMoveMessage(a));

                gameMaster.Update(1.0);

                for (int j = 0; j < agents.Count; j++)
                    if (previousPositions[j] != agents[j].Position)
                        hasMoved[j] = true;
            }

            Assert.IsTrue(hasMoved.All(b => b == true));
        }

        private BaseMessage CreateRandomMoveMessage(Agent agent)
        {
            return MessageFactory.GetMessage(new MoveRequest(GetRandomDirection()), agent.Id);
        }

        private Direction GetRandomDirection()
        {
            int number = random.Next(0, 4);
            return (Direction)number;
        }
    }
}


