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

namespace GameMasterTests
{
    public class GameLogicTest
    {
        private GameMaster.GameMaster gameMaster;
        private GameLogicComponent gameLogicComponent;
        private GameMasterConfiguration configuration;

        [SetUp]
        public void Setup()
        {
            gameMaster = new GameMaster.GameMaster();
            gameLogicComponent = gameMaster.GameLogic;
            configuration = gameMaster.Configuration;
        }

        [Test]
        public void ProcessMessage_ShouldReturnUndefinedErrorMessageWhenAgentNotConnected()
        {
            var message = GetBaseMessage(new PutDownPieceRequest(), 666);
            var response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.UndefinedError, response.MessageId);
        }

        [Test]
        public void ProcessMessage_ShouldReturnIgnoredDelayErrorMessageWhenAgentHasTimeout()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));
            agent.AddTimeout(10.0);
            var startTime = DateTime.Now.AddSeconds(10);

            gameMaster.AddAgent(agent);

            var message = GetBaseMessage(new PutDownPieceRequest(), 666);
            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.IgnoredDelayError, response.MessageId);

            var payload = response.Payload as IgnoredDelayError;
            var timeDiff = startTime - payload.WaitUntil;
            Assert.AreEqual(0, (int)timeDiff.TotalSeconds);
        }

        [Test]
        public void ProcessMessage_ShouldReturnUndefinedErrorMessageWhenForcedAgentNotAnswered()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));
            agent.InformationExchangeRequested(true);

            gameMaster.AddAgent(agent);

            var message = GetBaseMessage(new PutDownPieceRequest(), 666);
            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.UndefinedError, response.MessageId);

            var payload = response.Payload as UndefinedError;
            Assert.AreEqual(new Point(3, 3), payload.Position);
        }

        [Test]
        public void ProcessMessage_CheckSham_ShouldReturnUndefinedErrorWhenAgentNotHolding()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));

            gameMaster.AddAgent(agent);

            var message = GetBaseMessage(new CheckShamRequest(), 666);
            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.UndefinedError, response.MessageId);

            var payload = response.Payload as UndefinedError;
            Assert.AreEqual(new Point(3, 3), payload.Position);
            Assert.AreEqual(configuration.CheckForShamPenalty.TotalSeconds, agent.Timeout);
        }

        [Test]
        public void ProcessMessage_CheckSham_ShouldSetTrueIfSham()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));
            var piece = new Piece(true);
            agent.PickUpPiece(piece);

            gameMaster.AddAgent(agent);

            var message = GetBaseMessage(new CheckShamRequest(), 666);
            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.CheckShamResponse, response.MessageId);

            var payload = response.Payload as CheckShamResponse;
            Assert.IsTrue(payload.Sham);
            Assert.AreEqual(configuration.CheckForShamPenalty.TotalSeconds, agent.Timeout);
        }

        [Test]
        public void ProcessMessage_CheckSham_ShouldSetFalseIfSham()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));
            var piece = new Piece(false);
            agent.PickUpPiece(piece);

            gameMaster.AddAgent(agent);

            var message = GetBaseMessage(new CheckShamRequest(), 666);
            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.CheckShamResponse, response.MessageId);

            var payload = response.Payload as CheckShamResponse;
            Assert.IsFalse(payload.Sham);
            Assert.AreEqual(configuration.CheckForShamPenalty.TotalSeconds, agent.Timeout);
        }



        // This method simulates normal situation where messages are stored in IEnumerable<BaseMessage>
        private BaseMessage GetBaseMessage<T>(T payload, int agentFromId) where T:IPayload
        {
            return MessageFactory.GetMessage(payload, agentFromId);
        }
    }
}
