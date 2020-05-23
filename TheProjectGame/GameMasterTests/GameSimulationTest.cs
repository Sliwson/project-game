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
using Moq;
using Messaging.Communication;
using System.Threading;

namespace GameMasterTests
{
    public class GameplaySimulationTest
    {
        private Random random = new Random();

        private GameMasterConfiguration config;
        private GameMaster.GameMaster gameMaster;
        private Mock<INetworkComponent> mockedNetworkComponent;

        private bool isBatchSent = false;

        [SetUp]
        public void Setup()
        {
            config = GameMasterConfiguration.GetDefault();
            gameMaster = new GameMaster.GameMaster(config);

            mockedNetworkComponent = new Mock<INetworkComponent>();
            mockedNetworkComponent.Setup(m => m.Connect(It.IsAny<ClientType>())).Returns(true);
            mockedNetworkComponent.Setup(m => m.GetIncomingMessages()).Returns( GetMockedMessages() );
        }

        [Test]
        public void SimpleSimulationTest_GameMasterShouldConnectAgentsAndProcessMoveRequestsAfterGameStart()
        {
            gameMaster.ConnectToCommunicationServer(mockedNetworkComponent.Object);
            CreateAndConnectAgents();
            Play();
        }

        private void CreateAndConnectAgents()
        {
            var agentMessages = new List<BaseMessage>();
            int limit = config.TeamSize;

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

                gameMaster.Update(1.0);

                for (int j = 0; j < agents.Count; j++)
                    if (previousPositions[j] != agents[j].Position)
                        hasMoved[j] = true;

                isBatchSent = false;
            }

            Assert.IsTrue(hasMoved.All(b => b == true));
        }

        private IEnumerable<BaseMessage> GetMockedMessages()
        {
            int limit = config.TeamSize;
            var agents = gameMaster.Agents;

            if (gameMaster.state == GameMasterState.ConnectingAgents)
            {
                for (int i = 0; i < limit * 2; i++)
                {
                    yield return MessageFactory.GetMessage(new JoinRequest(i < limit ? TeamId.Blue : TeamId.Red), i);
                }
            }

            else if (gameMaster.state == GameMasterState.InGame)
            {
                while(!isBatchSent)
                {
                    foreach (var a in agents)
                    {
                        yield return CreateRandomMoveMessage(a);
                    }
                    isBatchSent = true;
                }
            }
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