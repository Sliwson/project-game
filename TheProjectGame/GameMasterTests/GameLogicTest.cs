﻿using NUnit.Framework;
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

        #region Process message (error handling)

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

#endregion

        #region Check sham

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

        #endregion

        #region Destroy piece
        
        [Test]
        public void ProcessMessage_DestroyPiece_ShouldReturnUndefinedErrorWhenAgentNotHolding()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));

            gameMaster.AddAgent(agent);

            var message = GetBaseMessage(new DestroyPieceRequest(), 666);
            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.UndefinedError, response.MessageId);

            var payload = response.Payload as UndefinedError;
            Assert.AreEqual(new Point(3, 3), payload.Position);
            Assert.AreEqual(configuration.DestroyPiecePenalty.TotalSeconds, agent.Timeout);
        }

        [Test]
        public void ProcessMessage_DestroyPiece_ShouldRemovePiece()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));
            var piece = new Piece(false);
            agent.PickUpPiece(piece);

            gameMaster.AddAgent(agent);

            var message = GetBaseMessage(new DestroyPieceRequest(), 666);
            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.DestroyPieceResponse, response.MessageId);
            Assert.IsNull(agent.Piece);
            Assert.AreEqual(configuration.DestroyPiecePenalty.TotalSeconds, agent.Timeout);
        }

        #endregion

        #region Discover

        [Test]
        public void ProcessMessage_Discover_ShouldReturnFilledArrayInResponse()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));
            var discoverArray = gameMaster.BoardLogic.GetDiscoverArray(agent.Position);

            gameMaster.AddAgent(agent);

            var message = GetBaseMessage(new DiscoverRequest(), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.DiscoverResponse, response.MessageId);

            var payload = response.Payload as DiscoverResponse;
            Assert.AreEqual(discoverArray, payload.Distances);
            Assert.AreEqual(configuration.DiscoveryPenalty.TotalSeconds, agent.Timeout);
        }

        #endregion

        #region Exchange information (request)

        [Test]
        public void ProcessMessage_ExchangeInformationRequest_ShouldReturnUndefinedErrorMessageWhenReceipentNotConnected()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));

            gameMaster.AddAgent(agent);

            var message = GetBaseMessage(new ExchangeInformationRequest(333), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.UndefinedError, response.MessageId);
            Assert.AreEqual(configuration.AskPenalty.TotalSeconds, agent.Timeout);
        }

        [Test]
        public void ProcessMessage_ExchangeInformationRequest_ShouldReturnExchangeInformationMessage()
        {
            var sender = new Agent(666, TeamId.Blue, new Point(3, 3));
            var receipent = new Agent(333, TeamId.Blue, new Point(3, 3));

            gameMaster.AddAgent(sender);
            gameMaster.AddAgent(receipent);

            var message = GetBaseMessage(new ExchangeInformationRequest(333), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.ExchangeInformationMessage, response.MessageId);

            var payload = response.Payload as ExchangeInformationPayload;
            Assert.AreEqual(666, payload.AskingAgentId);
            Assert.AreEqual(TeamId.Blue, payload.TeamId);
            Assert.AreEqual(configuration.AskPenalty.TotalSeconds, sender.Timeout);
        }

        [Test]
        public void ProcessMessage_ExchangeInformationRequest_ShouldForceReplyIfLeaderAsking()
        {
            var sender = new Agent(666, TeamId.Blue, new Point(3, 3), true);
            var receipent = new Agent(333, TeamId.Blue, new Point(3, 3));

            gameMaster.AddAgent(sender);
            gameMaster.AddAgent(receipent);

            var message = GetBaseMessage(new ExchangeInformationRequest(333), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.ExchangeInformationMessage, response.MessageId);

            var payload = response.Payload as ExchangeInformationPayload;
            Assert.AreEqual(666, payload.AskingAgentId);
            Assert.AreEqual(TeamId.Blue, payload.TeamId);
            Assert.IsTrue(payload.Leader);
            Assert.IsTrue(receipent.HaveToExchange());
            Assert.AreEqual(configuration.AskPenalty.TotalSeconds, sender.Timeout);
        }

        #endregion


        #region Exchange information (response)

        [Test]
        public void ProcessMessage_ExchangeInformationResponse_ShouldReturnUndefinedErrorMessageWhenAgentNotAsked()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));

            gameMaster.AddAgent(agent);

            var message = GetBaseMessage(new ExchangeInformationResponse(333, null, null, null), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.UndefinedError, response.MessageId);
            Assert.AreEqual(configuration.ResponsePenalty.TotalSeconds, agent.Timeout);
        }

        [Test]
        public void ProcessMessage_ExchangeInformationResponse_OnlyAgentIdShouldBeChanged()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3), true);
            agent.InformationExchangeRequested(false);

            gameMaster.AddAgent(agent);

            var payload = new ExchangeInformationResponse(
                          333,
                          new int[,] { { 1, 2 }, { 3, 4 } },
                          new GoalInformation[,] { { GoalInformation.Goal, GoalInformation.NoGoal }, { GoalInformation.NoInformation, GoalInformation.NoInformation } },
                          new GoalInformation[,] { { GoalInformation.Goal, GoalInformation.NoGoal }, { GoalInformation.NoInformation, GoalInformation.NoInformation } });

            var message = GetBaseMessage(payload, 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(333, response.AgentId);
            Assert.AreEqual(payload, response.Payload as ExchangeInformationResponse);
            Assert.AreEqual(configuration.ResponsePenalty.TotalSeconds, agent.Timeout);
        }

        #endregion

        // This method simulates normal situation where messages are stored in IEnumerable<BaseMessage>
        private BaseMessage GetBaseMessage<T>(T payload, int agentFromId) where T:IPayload
        {
            return MessageFactory.GetMessage(payload, agentFromId);
        }
    }
}
