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
            var initialDelay = TimeSpan.FromSeconds(10.0);

            gameMaster.Agents.Add(agent);

            var message = GetBaseMessage(new PutDownPieceRequest(), 666);
            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.IgnoredDelayError, response.MessageId);

            var payload = response.Payload as IgnoredDelayError;
            var timeDiff = initialDelay - payload.RemainingDelay;
            Assert.AreEqual(0, (int)timeDiff.TotalSeconds);
        }

        [Test]
        public void ProcessMessage_ShouldReturnUndefinedErrorMessageWhenForcedAgentNotAnswered()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));
            agent.InformationExchangeRequested(true);

            gameMaster.Agents.Add(agent);

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

            gameMaster.Agents.Add(agent);

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

            gameMaster.Agents.Add(agent);

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

            gameMaster.Agents.Add(agent);

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

            gameMaster.Agents.Add(agent);

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

            gameMaster.Agents.Add(agent);

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

            gameMaster.Agents.Add(agent);

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

            gameMaster.Agents.Add(agent);

            var message = GetBaseMessage(new ExchangeInformationRequest(333), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.UndefinedError, response.MessageId);
            Assert.AreEqual(configuration.InformationExchangePenalty.TotalSeconds, agent.Timeout);
        }

        [Test]
        public void ProcessMessage_ExchangeInformationRequest_ShouldReturnExchangeInformationMessage()
        {
            var sender = new Agent(666, TeamId.Blue, new Point(3, 3));
            var receipent = new Agent(333, TeamId.Blue, new Point(3, 3));

            gameMaster.Agents.Add(sender);
            gameMaster.Agents.Add(receipent);

            var message = GetBaseMessage(new ExchangeInformationRequest(333), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.ExchangeInformationMessage, response.MessageId);

            var payload = response.Payload as ExchangeInformationPayload;
            Assert.AreEqual(666, payload.AskingAgentId);
            Assert.AreEqual(TeamId.Blue, payload.TeamId);
            Assert.AreEqual(configuration.InformationExchangePenalty.TotalSeconds, sender.Timeout);
        }

        [Test]
        public void ProcessMessage_ExchangeInformationRequest_ShouldForceReplyIfLeaderAsking()
        {
            var sender = new Agent(666, TeamId.Blue, new Point(3, 3), true);
            var receipent = new Agent(333, TeamId.Blue, new Point(3, 3));

            gameMaster.Agents.Add(sender);
            gameMaster.Agents.Add(receipent);

            var message = GetBaseMessage(new ExchangeInformationRequest(333), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.ExchangeInformationMessage, response.MessageId);

            var payload = response.Payload as ExchangeInformationPayload;
            Assert.AreEqual(666, payload.AskingAgentId);
            Assert.AreEqual(TeamId.Blue, payload.TeamId);
            Assert.IsTrue(payload.Leader);
            Assert.IsTrue(receipent.HaveToExchange());
            Assert.AreEqual(configuration.InformationExchangePenalty.TotalSeconds, sender.Timeout);
        }

        #endregion

        #region Exchange information (response)

        [Test]
        public void ProcessMessage_ExchangeInformationResponse_ShouldReturnUndefinedErrorMessageWhenAgentNotAsked()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));

            gameMaster.Agents.Add(agent);

            var message = GetBaseMessage(new ExchangeInformationResponse(333, null, null, null), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.UndefinedError, response.MessageId);
            Assert.AreEqual(configuration.InformationExchangePenalty.TotalSeconds, agent.Timeout);
        }

        [Test]
        public void ProcessMessage_ExchangeInformationResponse_OnlyAgentIdShouldBeChanged()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3), true);
            agent.InformationExchangeRequested(false);

            gameMaster.Agents.Add(agent);

            var payload = new ExchangeInformationResponse(
                          333,
                          new int[,] { { 1, 2 }, { 3, 4 } },
                          new GoalInformation[,] { { GoalInformation.Goal, GoalInformation.NoGoal }, { GoalInformation.NoInformation, GoalInformation.NoInformation } },
                          new GoalInformation[,] { { GoalInformation.Goal, GoalInformation.NoGoal }, { GoalInformation.NoInformation, GoalInformation.NoInformation } });

            var message = GetBaseMessage(payload, 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(333, response.AgentId);
            Assert.IsTrue(response.Payload is ExchangeInformationResponsePayload);

            var responsePayload = response.Payload as ExchangeInformationResponsePayload;
            Assert.AreEqual(payload.RespondToId, responsePayload.RespondingId);
            Assert.AreEqual(payload.Distances, responsePayload.Distances);
            Assert.AreEqual(payload.BlueTeamGoalAreaInformation, responsePayload.BlueTeamGoalAreaInformation);
            Assert.AreEqual(payload.RedTeamGoalAreaInformation, responsePayload.RedTeamGoalAreaInformation);
            Assert.AreEqual(configuration.InformationExchangePenalty.TotalSeconds, agent.Timeout);
        }

        #endregion

        #region Join

        [Test]
        public void ProcessMessage_JoinRequest_NewAgentShouldNotBeAcceptedDuringGame()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3), true);

            gameMaster.Agents.Add(agent);

            var message = GetBaseMessage(new JoinRequest(TeamId.Blue, false), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.JoinResponse, response.MessageId);

            var payload = response.Payload as JoinResponse;
            Assert.AreEqual(666, payload.AgentId);
            Assert.AreEqual(payload.AgentId, response.AgentId);
            Assert.IsFalse(payload.Accepted);
        }

        [Test]
        public void ProcessMessage_JoinRequest_OldAgentShouldNotBeAcceptedDuringGame()
        {
            var message = GetBaseMessage(new JoinRequest(TeamId.Blue, false), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.JoinResponse, response.MessageId);

            var payload = response.Payload as JoinResponse;
            Assert.AreEqual(666, payload.AgentId);
            Assert.AreEqual(payload.AgentId, response.AgentId);
            Assert.IsFalse(payload.Accepted);
        }

        #endregion

        #region Move

        [Test]
        public void ProcessMessage_MoveRequest_ShouldChangeAgentPositionIfAllowed()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));
            var field = gameMaster.BoardLogic.GetField(new Point(3, 3));

            gameMaster.Agents.Add(agent);
            field.Agent = agent;

            var message = GetBaseMessage(new MoveRequest(Direction.North), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.MoveResponse, response.MessageId);

            var payload = response.Payload as MoveResponse;
            Assert.IsTrue(payload.MadeMove);
            Assert.AreEqual(new Point(3, 4), payload.CurrentPosition);
            Assert.AreEqual(configuration.MovePenalty.TotalSeconds, agent.Timeout);
        }

        [Test]
        public void ProcessMessage_MoveRequest_ShouldNotChangeAgentPositionIfNotAllowed()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(0, 0));
            var field = gameMaster.BoardLogic.GetField(new Point(0, 0));

            gameMaster.Agents.Add(agent);
            field.Agent = agent;

            var message = GetBaseMessage(new MoveRequest(Direction.South), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.MoveResponse, response.MessageId);

            var payload = response.Payload as MoveResponse;
            Assert.IsFalse(payload.MadeMove);
            Assert.AreEqual(new Point(0, 0), payload.CurrentPosition);
            Assert.AreEqual(configuration.MovePenalty.TotalSeconds, agent.Timeout);
        }

        #endregion

        #region Pick up piece

        [Test]
        public void ProcessMessage_PickUpPieceRequest_ShouldReturnErrorIfNoPiece()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));
            var field = gameMaster.BoardLogic.GetField(new Point(3, 3));

            gameMaster.Agents.Add(agent);
            field.Agent = agent;

            var message = GetBaseMessage(new PickUpPieceRequest(), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.PickUpPieceError, response.MessageId);

            var payload = response.Payload as PickUpPieceError;
            Assert.AreEqual(PickUpPieceErrorSubtype.NothingThere, payload.ErrorSubtype);
        }

        [Test]
        public void ProcessMessage_PickUpPieceRequest_ShouldReturnErrorIfAlreadyHolding()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));
            var field = gameMaster.BoardLogic.GetField(new Point(3, 3));
            var piece = new Piece(false);

            gameMaster.Agents.Add(agent);
            agent.PickUpPiece(piece);
            field.Agent = agent;

            var message = GetBaseMessage(new PickUpPieceRequest(), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.PickUpPieceError, response.MessageId);

            var payload = response.Payload as PickUpPieceError;
            Assert.AreEqual(PickUpPieceErrorSubtype.Other, payload.ErrorSubtype);
        }

        [Test]
        public void ProcessMessage_PickUpPieceRequest_ShouldAssignPieceIfAllowed()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));
            var field = gameMaster.BoardLogic.GetField(new Point(3, 3));
            var piece = new Piece(false);

            gameMaster.Agents.Add(agent);
            field.Pieces.Push(piece);
            field.Agent = agent;

            var message = GetBaseMessage(new PickUpPieceRequest(), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.PickUpPieceResponse, response.MessageId);
            Assert.AreEqual(piece, agent.Piece);
            Assert.AreEqual(0, field.Pieces.Count);
        }

        #endregion

        #region Put down piece

        [Test]
        public void ProcessMessage_PutDownPieceRequest_ShouldReturnErrorIfNoPiece()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(3, 3));
            var field = gameMaster.BoardLogic.GetField(new Point(3, 3));
            var initialScore = gameMaster.ScoreComponent.GetScore(agent.Team);

            gameMaster.Agents.Add(agent);
            field.Agent = agent;

            var message = GetBaseMessage(new PutDownPieceRequest(), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.PutDownPieceError, response.MessageId);

            var payload = response.Payload as PutDownPieceError;
            Assert.AreEqual(PutDownPieceErrorSubtype.AgentNotHolding, payload.ErrorSubtype);
            Assert.AreEqual(initialScore, gameMaster.ScoreComponent.GetScore(agent.Team));
            Assert.AreEqual(configuration.PutPenalty.TotalSeconds, agent.Timeout);
        }

        [Test]
        public void ProcessMessage_PutDownPieceRequest_ShouldLeavePieceInTaskArea()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(10, 10));
            var field = gameMaster.BoardLogic.GetField(agent.Position);
            // Make sure the field is in task area - if test fails here, change position / configuration
            Assert.IsTrue(gameMaster.BoardLogic.IsFieldInTaskArea(agent.Position));
            var piece = new Piece(false);
            var initialScore = gameMaster.ScoreComponent.GetScore(agent.Team);

            gameMaster.Agents.Add(agent);
            agent.PickUpPiece(piece);
            field.Agent = agent;

            var message = GetBaseMessage(new PutDownPieceRequest(), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.PutDownPieceResponse, response.MessageId);

            var payload = response.Payload as PutDownPieceResponse;

            Assert.AreEqual(PutDownPieceResult.TaskField, payload.Result);
            Assert.IsNull(agent.Piece);
            Assert.AreEqual(1, field.Pieces.Count);
            Assert.AreEqual(piece, field.Pieces.Peek());
            Assert.AreEqual(initialScore, gameMaster.ScoreComponent.GetScore(agent.Team));
            Assert.AreEqual(configuration.PutPenalty.TotalSeconds, agent.Timeout);
        }

        [Test]
        public void ProcessMessage_PutDownPieceRequest_ShouldDestroyNormalPieceInGoalArea()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(0, 0));
            var field = gameMaster.BoardLogic.GetField(agent.Position);
            // Make sure the field is in goal area - if test fails here, change position / configuration
            Assert.IsTrue(gameMaster.BoardLogic.IsFieldInGoalArea(agent.Position));
            var piece = new Piece(false);
            var initialScore = gameMaster.ScoreComponent.GetScore(agent.Team);

            gameMaster.Agents.Add(agent);
            agent.PickUpPiece(piece);
            field.Agent = agent;

            var message = GetBaseMessage(new PutDownPieceRequest(), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.PutDownPieceResponse, response.MessageId);

            var payload = response.Payload as PutDownPieceResponse;

            Assert.AreEqual(PutDownPieceResult.NormalOnNonGoalField, payload.Result);
            Assert.IsNull(agent.Piece);
            Assert.AreEqual(0, field.Pieces.Count);
            Assert.AreEqual(initialScore, gameMaster.ScoreComponent.GetScore(agent.Team));
            Assert.AreEqual(configuration.PutPenalty.TotalSeconds, agent.Timeout);
        }

        [Test]
        public void ProcessMessage_PutDownPieceRequest_ShouldDestroyShamInGoalArea()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(0, 0));
            var field = gameMaster.BoardLogic.GetField(agent.Position);
            // Make sure the field is in goal area - if test fails here, change position / configuration
            Assert.IsTrue(gameMaster.BoardLogic.IsFieldInGoalArea(agent.Position));
            var piece = new Piece(true);
            var initialScore = gameMaster.ScoreComponent.GetScore(agent.Team);

            gameMaster.Agents.Add(agent);
            agent.PickUpPiece(piece);
            field.Agent = agent;

            var message = GetBaseMessage(new PutDownPieceRequest(), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.PutDownPieceResponse, response.MessageId);

            var payload = response.Payload as PutDownPieceResponse;

            Assert.AreEqual(PutDownPieceResult.ShamOnGoalArea, payload.Result);
            Assert.IsNull(agent.Piece);
            Assert.AreEqual(0, field.Pieces.Count);
            Assert.AreEqual(initialScore, gameMaster.ScoreComponent.GetScore(agent.Team));
            Assert.AreEqual(configuration.PutPenalty.TotalSeconds, agent.Timeout);
        }

        [Test]
        public void ProcessMessage_PutDownPieceRequest_ShouldCompleteGoalIfPieceIsNormal()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(0, 0));
            var field = gameMaster.BoardLogic.GetField(agent.Position);
            // Make sure the field is in goal area - if test fails here, change position / configuration
            Assert.IsTrue(gameMaster.BoardLogic.IsFieldInGoalArea(agent.Position));
            field.State = FieldState.Goal;
            var piece = new Piece(false);
            var initialScore = gameMaster.ScoreComponent.GetScore(agent.Team);

            gameMaster.Agents.Add(agent);
            agent.PickUpPiece(piece);
            field.Agent = agent;

            var message = GetBaseMessage(new PutDownPieceRequest(), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.PutDownPieceResponse, response.MessageId);

            var payload = response.Payload as PutDownPieceResponse;

            Assert.AreEqual(PutDownPieceResult.NormalOnGoalField, payload.Result);
            Assert.IsNull(agent.Piece);
            Assert.AreEqual(0, field.Pieces.Count);
            Assert.AreEqual(1, gameMaster.ScoreComponent.GetScore(agent.Team) - initialScore);
            Assert.AreEqual(FieldState.CompletedGoal, field.State);
            Assert.AreEqual(configuration.PutPenalty.TotalSeconds, agent.Timeout);
        }

        [Test]
        public void ProcessMessage_PutDownPieceRequest_ShouldNotCompleteGoalIfPieceIsSham()
        {
            var agent = new Agent(666, TeamId.Blue, new Point(0, 0));
            var field = gameMaster.BoardLogic.GetField(agent.Position);
            // Make sure the field is in goal area - if test fails here, change position / configuration
            Assert.IsTrue(gameMaster.BoardLogic.IsFieldInGoalArea(agent.Position));
            field.State = FieldState.Goal;
            var piece = new Piece(true);
            var initialScore = gameMaster.ScoreComponent.GetScore(agent.Team);

            gameMaster.Agents.Add(agent);
            agent.PickUpPiece(piece);
            field.Agent = agent;

            var message = GetBaseMessage(new PutDownPieceRequest(), 666);

            dynamic response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.PutDownPieceResponse, response.MessageId);

            var payload = response.Payload as PutDownPieceResponse;

            Assert.AreEqual(PutDownPieceResult.ShamOnGoalArea, payload.Result);
            Assert.IsNull(agent.Piece);
            Assert.AreEqual(0, field.Pieces.Count);
            Assert.AreEqual(initialScore, gameMaster.ScoreComponent.GetScore(agent.Team));
            Assert.AreEqual(FieldState.Goal, field.State);
            Assert.AreEqual(configuration.PutPenalty.TotalSeconds, agent.Timeout);
        }

        #endregion

        // This method simulates normal situation where messages are stored in IEnumerable<BaseMessage>
        private BaseMessage GetBaseMessage<T>(T payload, int agentFromId) where T:IPayload
        {
            return MessageFactory.GetMessage(payload, agentFromId);
        }
    }
}
