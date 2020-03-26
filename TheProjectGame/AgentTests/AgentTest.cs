using Agent;
using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.GameMaster;
using NUnit.Framework;
using System;

namespace AgentTests
{
    public class AgentTest
    {
        private Agent.Agent agent;

        [SetUp]
        public void Setup()
        {
            agent = new Agent.Agent(); //Initialize(...)
        }

        [Test]
        public void IsInitialized()
        {
            Assert.IsNotNull(agent.waitingPlayers);
            Assert.IsNull(agent.piece);
            foreach (var field in agent.board)
            {
                Assert.AreEqual(field.deniedMove, DateTime.MinValue);
                Assert.AreEqual(field.distLearned, DateTime.MinValue);
            }
        }

        [Test]
        public void IsReadyToPlay()
        {
            //agent.Join...(...);
            Assert.AreEqual(agent.boardSize.X, agent.board.GetLength(0));
            Assert.AreEqual(agent.boardSize.Y, agent.board.GetLength(1));
            Assert.True(agent.position.X >= 0 && agent.position.Y >= 0 && agent.position.X < agent.boardSize.X && agent.position.Y < agent.boardSize.Y);
        }

        [Test]
        public void PicksUpIfDistanceZero()
        {
            var message = new Message<IPayload>(0, 0, new MoveResponse(false, new System.Drawing.Point(0, 0), 0));
            agent.AcceptMessage(message);
            Assert.IsInstanceOf<PickUpPieceRequest>(agent.lastRequest.Payload);
        }

        [Test]
        public void DoesNotCarryPieceAfterPut()
        {
            agent.Put();
            Assert.IsNull(agent.piece);
        }

        [Test]
        public void DestroysPieceIfSham()
        {
            var message = new Message<IPayload>(0, 0, new CheckShamResponse(true));
            agent.AcceptMessage(message);
            Assert.IsInstanceOf<DestroyPieceRequest>(agent.lastRequest.Payload);
        }
    }
}