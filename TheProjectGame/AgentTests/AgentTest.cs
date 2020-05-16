using Agent;
using Agent.strategies;
using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.Errors;
using Messaging.Contracts.GameMaster;
using Messaging.Enumerators;
using Messaging.Implementation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace AgentTests
{
    public class AgentTest
    {
        private Agent.Agent agent; //agent initialized by default values in each run
        private DateTime startTime; //start time of each run
        private AgentConfiguration defaultConfiguration;

        //props set for test run
        private Point TestBoardSize => new Point(5, 6);
        private Point TestInitialPosition => new Point(0, 0);

        [SetUp]
        public void Setup()
        {
            agent = GetDefaultAgent();
            startTime = DateTime.Now;
            defaultConfiguration = AgentConfiguration.GetDefault();
        }

        [Test]
        public void Initialize_ShouldSetBoardSize()
        {
            Assert.AreEqual(agent.BoardLogicComponent.BoardSize.X, TestBoardSize.X);
            Assert.AreEqual(agent.BoardLogicComponent.BoardSize.Y, TestBoardSize.Y);
        }

        [Test]
        public void Initialize_ShouldSetAgentPosition()
        {
            Assert.AreEqual(agent.BoardLogicComponent.Position.X, TestInitialPosition.X);
            Assert.AreEqual(agent.BoardLogicComponent.Position.Y, TestInitialPosition.Y);
        }

        [Test]
        public void Agent_ShouldBeProperlyInitialized()
        {
            Assert.IsNotNull(agent.WaitingPlayers);
            Assert.IsNull(agent.Piece);

            foreach (var field in agent.BoardLogicComponent.Board)
            {
                Assert.AreEqual(field.deniedMove, DateTime.MinValue);
                Assert.AreEqual(field.distLearned, DateTime.MinValue);
            }
        }

        [Test]
        public void Agent_ShouldHaveBoardInitialized()
        {
            Assert.AreEqual(agent.BoardLogicComponent.BoardSize.X, agent.BoardLogicComponent.Board.GetLength(1));
            Assert.AreEqual(agent.BoardLogicComponent.BoardSize.Y, agent.BoardLogicComponent.Board.GetLength(0));
            Assert.True(agent.BoardLogicComponent.Position.X >= 0 && agent.BoardLogicComponent.Position.Y >= 0 && agent.BoardLogicComponent.Position.X < agent.BoardLogicComponent.BoardSize.X && agent.BoardLogicComponent.Position.Y < agent.BoardLogicComponent.BoardSize.Y);
        }

        #region Join

        [Test]
        public void ProcessMessage_JoinRequestWhenAcceptedShouldSetAgentId()
        {
            Assert.AreEqual(agent.Id, 0);
            agent.AcceptMessage(GetBaseMessage(new JoinResponse(true, 1), 1));
            Assert.AreEqual(agent.Id, 1);
        }

        [Test]
        public void ProcessMessage_JoinRequestWhenNotAcceptedShouldNotSetAgentId()
        {
            Assert.AreEqual(agent.Id, 0);
            agent.AcceptMessage(GetBaseMessage(new JoinResponse(false, 1), 1));
            Assert.AreEqual(agent.Id, 0);
        }

        #endregion

        #region Check Sham

        [Test]
        public void ProcessMessage_CheckShamResponseIfShamAgentShouldDestroyPiece()
        {
            agent.AgentState = AgentState.InGame;
            agent.Piece = new Piece();
            agent.AcceptMessage(GetBaseMessage(new CheckShamResponse(true), 1));
            agent.AcceptMessage(GetBaseMessage(new DestroyPieceResponse(), 1));

            Assert.IsNull(agent.Piece);
        }

        [Test]
        public void ProcessMessage_CheckShamResponseIfNotShamAgentPieceShouldBeDiscovered()
        {
            agent.AgentState = AgentState.InGame;
            agent.Piece = new Piece();

            Assert.IsFalse(agent.Piece.isDiscovered);
            agent.AcceptMessage(GetBaseMessage(new CheckShamResponse(false), 1));
            Assert.IsTrue(agent.Piece.isDiscovered);
        }

        #endregion

        #region Discover

        [Test]
        public void ProcessMessage_DiscoverResponseShouldUpdateAgentBoardState()
        {
            agent.AgentState = AgentState.InGame;

            agent.AcceptMessage(GetBaseMessage(new DiscoverResponse(new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } }), 1));
            var position = agent.BoardLogicComponent.Position;

            Assert.AreEqual(agent.BoardLogicComponent.Board[position.Y, position.X].distToPiece, 5);
            if (position.Y + 1 < agent.BoardLogicComponent.BoardSize.Y)
            {
                Assert.AreEqual(agent.BoardLogicComponent.Board[position.Y + 1, position.X].distToPiece, 8);
                if (position.X + 1 < agent.BoardLogicComponent.BoardSize.X)
                {
                    Assert.AreEqual(agent.BoardLogicComponent.Board[position.Y + 1, position.X + 1].distToPiece, 9);
                }
                if (position.X - 1 >= 0)
                {
                    Assert.AreEqual(agent.BoardLogicComponent.Board[position.Y + 1, position.X - 1].distToPiece, 7);
                }
            }
            if (position.X - 1 >= 0)
            {
                Assert.AreEqual(agent.BoardLogicComponent.Board[position.Y, position.X - 1].distToPiece, 4);
                if (position.Y - 1 >= 0)
                {
                    Assert.AreEqual(agent.BoardLogicComponent.Board[position.Y - 1, position.X - 1].distToPiece, 1);
                }
            }
            if (position.Y - 1 >= 0)
            {
                Assert.AreEqual(agent.BoardLogicComponent.Board[position.Y - 1, position.X].distToPiece, 2);
                if (position.X + 1 < agent.BoardLogicComponent.BoardSize.X)
                {
                    Assert.AreEqual(agent.BoardLogicComponent.Board[position.Y - 1, position.X + 1].distToPiece, 3);
                }
            }
            if (position.X + 1 < agent.BoardLogicComponent.BoardSize.X)
            {
                Assert.AreEqual(agent.BoardLogicComponent.Board[position.Y, position.X + 1].distToPiece, 6);
            }
        }

        #endregion

        #region Exchange information

        [Test]
        public void ProcessMessage_ExchangeInformationPayload_IfNotTeamLeaderAsking_ShouldBeAddedToWaitingList()
        {
            agent.AgentState = AgentState.InGame;
            agent.StartGameComponent.Initialize(new StartGamePayload(0, new int[] { 1, 2, 3 }, 1, null, TeamId.Blue, new Point(), 0, 3, 0, 0, 0, new System.Collections.Generic.Dictionary<ActionType, TimeSpan>(), 0.0f, new Point()));

            agent.AcceptMessage(GetBaseMessage(new ExchangeInformationRequestForward(2, false, Messaging.Enumerators.TeamId.Blue), 1));

            Assert.AreEqual(agent.WaitingPlayers.Count, 1);
            Assert.AreEqual(agent.WaitingPlayers[0], 2);
        }


        [Test]
        public void ProcessMessage_ExchangeInformationResponsePayload_ShouldUpdateAgentBoardState()
        {
            agent.AgentState = AgentState.InGame;

            var blueGoalAreaInformation = new GoalInformation[,] { { GoalInformation.Goal, GoalInformation.NoGoal, GoalInformation.NoGoal, GoalInformation.NoGoal, GoalInformation.NoInformation } };
            var redGoalAreaInformation = new GoalInformation[,] { { GoalInformation.NoInformation, GoalInformation.NoGoal, GoalInformation.NoGoal, GoalInformation.Goal, GoalInformation.NoInformation } };
            var distances = new int[,] {{ 1, 2, 3, 1, 4 }, { 2, 2, 2, 1, 3 }, { 3, 0, 2, 1, 2 }, { 2, 2, 2, 1, 1 }, { 3, 0, 2, 1, 2 }, { 2, 1, 3, 4, 1 } };

            agent.AcceptMessage(GetBaseMessage(new ExchangeInformationResponseForward(2, distances, redGoalAreaInformation, blueGoalAreaInformation ), 1));

            //distances are currently being ignored
            //for(int i = 0; i < agent.BoardLogicComponent.BoardSize.Y; i++)
            //{
            //    for(int j = 0; j < agent.BoardLogicComponent.BoardSize.X; j++)
            //    {
            //        Assert.AreEqual(agent.BoardLogicComponent.Board[i, j].distToPiece, distances[i, j]);
            //    }
            //}

            for (int i = 0; i < agent.BoardLogicComponent.GoalAreaSize; i++)
            {
                for (int j = 0; j < agent.BoardLogicComponent.BoardSize.X; j++)
                {
                    Assert.AreEqual(agent.BoardLogicComponent.Board[i, j].goalInfo, blueGoalAreaInformation[i, j]);
                }
            }

            for (int i = agent.BoardLogicComponent.BoardSize.Y - agent.BoardLogicComponent.GoalAreaSize + 1; i < agent.BoardLogicComponent.BoardSize.Y; i++)
            {
                for (int j = 0; j < agent.BoardLogicComponent.BoardSize.X; j++)
                {
                    Assert.AreEqual(agent.BoardLogicComponent.Board[i, j].goalInfo, redGoalAreaInformation[i, j]);
                }
            }
        }

        #endregion

        #region Move

        [Test]
        public void ProcessMessage_MoveResponse_WhenMoveMadeAgentPositionShouldChangeAndDistToPieceShouldUpdate()
        {
            agent.AgentState = AgentState.InGame;
            agent.AcceptMessage(GetBaseMessage(new MoveResponse(true, new Point(1, 0), 2), 1));

            Assert.AreEqual(agent.BoardLogicComponent.Position, new Point(1, 0));
            Assert.AreEqual(agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece, 2);
        }

        [Test]
        public void ProcessMessage_MoveResponse_WhenDistToPieceEqualZero_AgentShouldPickUpPiece()
        {
            agent.AgentState = AgentState.InGame;
            agent.AcceptMessage(GetBaseMessage(new MoveResponse(true, new Point(1, 0), 0), 1));
            agent.AcceptMessage(GetBaseMessage(new PickUpPieceResponse(), 1));

            Assert.AreEqual(agent.BoardLogicComponent.Position, new Point(1, 0));
            Assert.AreEqual(agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece, 0);
            Assert.IsNotNull(agent.Piece);
        }

        [Test]
        public void ProcessMessage_MoveResponse_WhenMoveDenied_AgentShouldUpdatePositionAndBoardState()
        {
            agent.AgentState = AgentState.InGame;
            agent.BoardLogicComponent.Position = new Point(0, 0);

            agent.AcceptMessage(GetBaseMessage(new MoveResponse(false, new Point(1, 0), 2), 1));

            Assert.AreEqual(agent.BoardLogicComponent.Position, new Point(1, 0));
            Assert.AreNotEqual(agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].deniedMove, startTime);
        }

        #endregion

        #region PickUp

        [Test]
        public void ProcessMessage_PickUpPieceResponse_WhenDistToPieceIsZero_AgentShouldPickUpPiece()
        {
            agent.AgentState = AgentState.InGame;
            agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece = 0;

            Assert.IsNull(agent.Piece);
            agent.AcceptMessage(GetBaseMessage(new PickUpPieceResponse(), 1));
            Assert.IsNotNull(agent.Piece);
        }

        [Test]
        public void ProcessMessage_PickUpPieceResponse_WhenDistToPieceIsNotZero_AgentShouldNotPickUpPiece()
        {
            agent.AgentState = AgentState.InGame;
            agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece = 1;

            Assert.IsNull(agent.Piece);
            agent.AcceptMessage(GetBaseMessage(new PickUpPieceResponse(), 1));
            Assert.IsNull(agent.Piece);
        }

        [Test]
        public void ProcessMessage_PickUpPieceResponse_WhenPickUpPieceError_DistToPieceShouldBeSetToDefault()
        {
            agent.AgentState = AgentState.InGame;
            agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece = 0;

            Assert.IsNull(agent.Piece);

            agent.AcceptMessage(GetBaseMessage(new PickUpPieceResponse(), 1));
            agent.AcceptMessage(GetBaseMessage(new PickUpPieceError(PickUpPieceErrorSubtype.NothingThere), 1));

            Assert.AreEqual(agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece, int.MaxValue);
        }

        #endregion

        #region PutDown

        [Test]
        public void ProcessMessage_PutDownPieceResponse_AgentShouldNotHavePiece()
        {
            agent.AgentState = AgentState.InGame;

            agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece = 1;
            agent.Piece = new Piece();

            Assert.IsNotNull(agent.Piece);
            agent.AcceptMessage(GetBaseMessage(new PutDownPieceResponse(PutDownPieceResult.TaskField), 1));
            Assert.IsNull(agent.Piece);
        }

        [Test]
        public void ProcessMessage_PutDownPieceResponse_WhenPutDownPieceErrorAgentNotHolding_AgentShouldNotHavePiece()
        {
            agent.AgentState = AgentState.InGame;
            agent.Piece = new Piece();
            Assert.IsNotNull(agent.Piece);

            agent.AcceptMessage(GetBaseMessage(new PutDownPieceResponse(PutDownPieceResult.TaskField), 1));
            agent.AcceptMessage(GetBaseMessage(new PutDownPieceError(PutDownPieceErrorSubtype.AgentNotHolding), 1));

            Assert.IsNull(agent.Piece);
        }

        #endregion

        #region Agent state

        [Test]
        public void AcceptMessage_ShouldJoinWhenAccepted()
        {
            var config = AgentConfiguration.GetDefault();
            agent = new Agent.Agent(config);
            agent.AgentState = AgentState.WaitingForJoin;
            agent.AcceptMessage(GetBaseMessage(new JoinResponse(true, 1), 1));
            Assert.AreEqual(agent.AgentState, AgentState.WaitingForStart);
        }

        [Test]
        public void AcceptMessage_ShouldNotJoinWnehRejected()
        {
            var config = AgentConfiguration.GetDefault();
            agent = new Agent.Agent(config);
            agent.AgentState = AgentState.WaitingForJoin;
            agent.AcceptMessage(GetBaseMessage(new JoinResponse(false, 1), 1));
            Assert.AreEqual(agent.AgentState, AgentState.WaitingForJoin);
        }

        #endregion

        // This method simulates normal situation where messages are stored in IEnumerable<BaseMessage>
        private BaseMessage GetBaseMessage<T>(T payload, int agentFromId) where T : IPayload
        {
            return MessageFactory.GetMessage(payload, agentFromId);
        }

        private Agent.Agent GetDefaultAgent()
        {
            var config = AgentConfiguration.GetDefault();
            return GetInitializedAgent(config);
        }

        private Agent.Agent GetInitializedAgent(AgentConfiguration config)
        {
            agent = new Agent.Agent(config);

            var startGamePayload = GetDefaultStartGamePayload();
            agent.StartGameComponent.Initialize(startGamePayload);
            agent.SetDoNothingStrategy();
            return agent;
        }

        private StartGamePayload GetDefaultStartGamePayload()
        {
            var teamMates = new int[3] { 2, 3, 4 };
            var enemiesIds = new int[3] { 5, 7, 6 };
            return new StartGamePayload(1, teamMates, 1, enemiesIds, TeamId.Blue, TestBoardSize, 1, 3, 3, 4, 4, new Dictionary<ActionType, TimeSpan>(), 0.5f, TestInitialPosition);
        }
    }
}