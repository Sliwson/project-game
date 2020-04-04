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
using System.Drawing;

namespace AgentTests
{
    public class AgentTest
    {
        private Agent.Agent agent;

        private DateTime startTime;

        [SetUp]
        public void Setup()
        {
            agent = new Agent.Agent(false);
            var teamMates = new int[3] { 2, 3, 4 };
            agent.Initialize(1, Messaging.Enumerators.TeamId.Blue, new System.Drawing.Point(5, 6), 1, new System.Drawing.Point(0, 0), teamMates, new System.Collections.Generic.Dictionary<ActionType, TimeSpan>(), 0.5f);
            agent.SetDoNothingStrategy();
            startTime = DateTime.Now;
        }

        [Test]
        public void Set_agent_TeamLeader()
        {
            agent = new Agent.Agent(false);
            agent.id = 1;
            agent.Initialize(1, Messaging.Enumerators.TeamId.Blue, new System.Drawing.Point(5, 5), 1, new System.Drawing.Point(0, 0), new int[3] { 2, 3, 4 }, new System.Collections.Generic.Dictionary<ActionType, TimeSpan>(), 0.5f);
            Assert.AreEqual(agent.isLeader, true);
        }

        [Test]
        public void Set_other_agent_TeamLeader()
        {
            var agent = new Agent.Agent(true);
            agent.id = 1;
            var teamMates = new int[3] { 2, 3, 4 };
            agent.Initialize(2, Messaging.Enumerators.TeamId.Blue, new System.Drawing.Point(5, 5), 1, new System.Drawing.Point(0, 0), teamMates, new System.Collections.Generic.Dictionary<ActionType, System.TimeSpan>(), 0.5f);
            Assert.AreEqual(agent.isLeader, false);
        }

        [Test]
        public void Set_agent_boardSize()
        {
            Assert.AreEqual(agent.boardSize.X, 5);
            Assert.AreEqual(agent.boardSize.Y, 6);
        }

        [Test]
        public void Set_agent_position()
        {
            Assert.AreEqual(agent.position.X, 0);
            Assert.AreEqual(agent.position.Y, 0);
        }

        [Test]
        public void Is_initialized()
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
        public void Has_correct_board_size()
        {
            Assert.AreEqual(agent.boardSize.X, agent.board.GetLength(1));
            Assert.AreEqual(agent.boardSize.Y, agent.board.GetLength(0));
            Assert.True(agent.position.X >= 0 && agent.position.Y >= 0 && agent.position.X < agent.boardSize.X && agent.position.Y < agent.boardSize.Y);
        }

        #region Join

        [Test]
        public void ProcessMessage_JoinRequest_When_Accepted_Should_Set_Agent_Id()
        {
            Assert.AreEqual(agent.id, 0);

            agent.JoinTheGame();

            agent.AcceptMessage(GetBaseMessage(new JoinResponse(true, 1), 1));

            Assert.AreEqual(agent.id, 1);
        }

        [Test]
        public void ProcessMessage_JoinRequest_When_Not_Accepted_Should_Not_Set_Agent_Id()
        {
            Assert.AreEqual(agent.id, 0);

            agent.AcceptMessage(GetBaseMessage(new JoinResponse(false, 1), 1));

            Assert.AreEqual(agent.id, 0);
        }

        #endregion

        #region Check Sham

        [Test]
        public void ProcessMessage_CheckShamResponse_If_Sham_Agent_Should_Destroy_Piece()
        {
            agent.agentState = AgentState.InGame;

            agent.piece = new Piece();

            agent.AcceptMessage(GetBaseMessage(new CheckShamResponse(true), 1));

            agent.AcceptMessage(GetBaseMessage(new DestroyPieceResponse(), 1));

            Assert.IsNull(agent.piece);
        }

        [Test]
        public void ProcessMessage_CheckShamResponse_If_Not_Sham_Agent_Piece_Should_Be_Discovered()
        {
            agent.agentState = AgentState.InGame;

            agent.piece = new Piece();

            Assert.IsFalse(agent.piece.isDiscovered);

            agent.AcceptMessage(GetBaseMessage(new CheckShamResponse(false), 1));

            Assert.IsTrue(agent.piece.isDiscovered);
        }

        #endregion

        #region Discover

        [Test]
        public void ProcessMessage_DiscoverResponse_Should_Update_Agent_Board_State()
        {
            agent.agentState = AgentState.InGame;

            agent.AcceptMessage(GetBaseMessage(new DiscoverResponse(new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } }), 1));
            var position = agent.position;

            Assert.AreEqual(agent.board[position.Y, position.X].distToPiece, 5);
            if (position.Y + 1 < agent.boardSize.Y)
            {
                Assert.AreEqual(agent.board[position.Y + 1, position.X].distToPiece, 8);
                if (position.X + 1 < agent.boardSize.X)
                {
                    Assert.AreEqual(agent.board[position.Y + 1, position.X + 1].distToPiece, 9);
                }
                if (position.X - 1 >= 0)
                {
                    Assert.AreEqual(agent.board[position.Y + 1, position.X - 1].distToPiece, 7);
                }
            }
            if (position.X - 1 >= 0)
            {
                Assert.AreEqual(agent.board[position.Y, position.X - 1].distToPiece, 4);
                if (position.Y - 1 >= 0)
                {
                    Assert.AreEqual(agent.board[position.Y - 1, position.X - 1].distToPiece, 1);
                }
            }
            if (position.Y - 1 >= 0)
            {
                Assert.AreEqual(agent.board[position.Y - 1, position.X].distToPiece, 2);
                if (position.X + 1 < agent.boardSize.X)
                {
                    Assert.AreEqual(agent.board[position.Y - 1, position.X + 1].distToPiece, 3);
                }
            }
            if (position.X + 1 < agent.boardSize.X)
            {
                Assert.AreEqual(agent.board[position.Y, position.X + 1].distToPiece, 6);
            }
        }

        #endregion

        #region Exchange information

        [Test]
        public void ProcessMessage_ExchangeInformationPayload_If_Not_TeamLeader_Asking_Should_Be_Added_To_Waiting_List()
        {
            agent.agentState = AgentState.InGame;

            agent.AcceptMessage(GetBaseMessage(new ExchangeInformationPayload(2, false, Messaging.Enumerators.TeamId.Blue), 1));

            Assert.AreEqual(agent.waitingPlayers.Count, 1);
            Assert.AreEqual(agent.waitingPlayers[0], 2);
        }


        [Test]
        public void ProcessMessage_ExchangeInformationResponse_Should_Update_Agent_Board_State()
        {
            agent.agentState = AgentState.InGame;

            var blueGoalAreaInformation = new GoalInformation[,] { { GoalInformation.Goal, GoalInformation.NoGoal, GoalInformation.NoGoal, GoalInformation.NoGoal, GoalInformation.NoInformation } };
            var redGoalAreaInformation = new GoalInformation[,] { { GoalInformation.NoInformation, GoalInformation.NoGoal, GoalInformation.NoGoal, GoalInformation.Goal, GoalInformation.NoInformation } };
            var distances = new int[,] {{ 1, 2, 3, 1, 4 }, { 2, 2, 2, 1, 3 }, { 3, 0, 2, 1, 2 }, { 2, 2, 2, 1, 1 }, { 3, 0, 2, 1, 2 }, { 2, 1, 3, 4, 1 } };

            agent.AcceptMessage(GetBaseMessage(new ExchangeInformationResponse(2, distances, redGoalAreaInformation, blueGoalAreaInformation ), 1));

            for(int i = 0; i < agent.boardSize.Y; i++)
            {
                for(int j = 0; j < agent.boardSize.X; j++)
                {
                    Assert.AreEqual(agent.board[i, j].distToPiece, distances[i, j]);
                }
            }

            for (int i = 0; i < agent.goalAreaSize; i++)
            {
                for (int j = 0; j < agent.boardSize.X; j++)
                {
                    Assert.AreEqual(agent.board[i, j].goalInfo, blueGoalAreaInformation[i, j]);
                }
            }

            for (int i = agent.boardSize.Y - agent.goalAreaSize + 1; i < agent.boardSize.Y; i++)
            {
                for (int j = 0; j < agent.boardSize.X; j++)
                {
                    Assert.AreEqual(agent.board[i, j].goalInfo, redGoalAreaInformation[i, j]);
                }
            }
        }

        #endregion

        #region Move

        [Test]
        public void ProcessMessage_MoveResponse_When_Move_Made_Agent_Position_Should_Change_And_DistToPiece_Should_Update()
        {
            agent.agentState = AgentState.InGame;

            agent.AcceptMessage(GetBaseMessage(new MoveResponse(true, new Point(1, 0), 2), 1));

            Assert.AreEqual(agent.position, new Point(1, 0));
            Assert.AreEqual(agent.board[agent.position.Y, agent.position.X].distToPiece, 2);
        }

        [Test]
        public void ProcessMessage_MoveResponse_When_DistToPiece_Equal_Zero_Agent_Should_PickUp_Piece()
        {
            agent.agentState = AgentState.InGame;

            agent.AcceptMessage(GetBaseMessage(new MoveResponse(true, new Point(1, 0), 0), 1));

            agent.AcceptMessage(GetBaseMessage(new PickUpPieceResponse(), 1));

            Assert.AreEqual(agent.position, new Point(1, 0));
            Assert.AreEqual(agent.board[agent.position.Y, agent.position.X].distToPiece, 0);
            Assert.IsNotNull(agent.piece);
        }

        [Test]
        public void ProcessMessage_MoveResponse_When_Move_Denied_AgentShould_Update_Position_And_Board_State()
        {
            agent.agentState = AgentState.InGame;
            agent.position = new Point(0, 0);

            agent.AcceptMessage(GetBaseMessage(new MoveResponse(false, new Point(1, 0), 2), 1));

            Assert.AreEqual(agent.position, new Point(1, 0));
            Assert.AreNotEqual(agent.board[agent.position.Y, agent.position.X].deniedMove, startTime);
        }

        #endregion

        #region PickUp

        [Test]
        public void ProcessMessage_PickUpPieceResponse_When_DistToPiece_Is_Zero_Agent_Should_PickUp_Piece()
        {
            agent.agentState = AgentState.InGame;

            agent.board[agent.position.Y, agent.position.X].distToPiece = 0;

            Assert.IsNull(agent.piece);

            agent.AcceptMessage(GetBaseMessage(new PickUpPieceResponse(), 1));

            Assert.IsNotNull(agent.piece);
        }

        [Test]
        public void ProcessMessage_PickUpPieceResponse_When_DistToPiece_Is_Not_Zero_Agent_Should_Not_PickUp_Piece()
        {
            agent.agentState = AgentState.InGame;

            agent.board[agent.position.Y, agent.position.X].distToPiece = 1;

            Assert.IsNull(agent.piece);

            agent.AcceptMessage(GetBaseMessage(new PickUpPieceResponse(), 1));

            Assert.IsNull(agent.piece);
        }

        [Test]
        public void ProcessMessage_PickUpPieceResponse_When_PickUpPieceError_DistToPiece_Should_Be_Set_To_Default()
        {
            agent.agentState = AgentState.InGame;

            agent.board[agent.position.Y, agent.position.X].distToPiece = 0;

            Assert.IsNull(agent.piece);

            agent.AcceptMessage(GetBaseMessage(new PickUpPieceResponse(), 1));

            agent.AcceptMessage(GetBaseMessage(new PickUpPieceError(PickUpPieceErrorSubtype.NothingThere), 1));

            Assert.AreEqual(agent.board[agent.position.Y, agent.position.X].distToPiece, int.MaxValue);
        }

        #endregion

        #region PutDown

        [Test]
        public void ProcessMessage_PutDownPieceResponse_DistToPiece_Should_Be_Updated_And_Agent_Should_Not_Have_Piece()
        {
            agent.agentState = AgentState.InGame;

            agent.board[agent.position.Y, agent.position.X].distToPiece = 1;
            agent.piece = new Piece();

            Assert.IsNotNull(agent.piece);
            Assert.AreEqual(agent.board[agent.position.Y, agent.position.X].distToPiece, 1);

            agent.AcceptMessage(GetBaseMessage(new PutDownPieceResponse(), 1));

            Assert.IsNull(agent.piece);
            Assert.AreEqual(agent.board[agent.position.Y, agent.position.X].distToPiece, 0);
        }

        [Test]
        public void ProcessMessage_PutDownPieceResponse_When_PutDownPieceError_AgentNotHolding_Agent_Should_Not_Have_Piece()
        {
            agent.agentState = AgentState.InGame;

            agent.piece = new Piece();

            Assert.IsNotNull(agent.piece);

            agent.AcceptMessage(GetBaseMessage(new PutDownPieceResponse(), 1));
            agent.AcceptMessage(GetBaseMessage(new PutDownPieceError(PutDownPieceErrorSubtype.AgentNotHolding), 1));

            Assert.IsNull(agent.piece);
        }

        #endregion

        #region Agent state

        [Test]
        public void Joins_When_Accepted()
        {
            var agent = new Agent.Agent(false);
            agent.SetDoNothingStrategy();
            agent.JoinTheGame();
            agent.AcceptMessage(GetBaseMessage(new JoinResponse(true, 1), 1));
            Assert.AreEqual(agent.agentState, AgentState.WaitingForStart);

        }

        [Test]
        public void Does_Not_Join_When_Rejected()
        {
            var agent = new Agent.Agent(false);
            agent.SetDoNothingStrategy();
            agent.JoinTheGame();
            agent.AcceptMessage(GetBaseMessage(new JoinResponse(false, 1), 1));
            Assert.AreEqual(agent.agentState, AgentState.WaitingForJoin);
        }

        #endregion

        // This method simulates normal situation where messages are stored in IEnumerable<BaseMessage>
        private BaseMessage GetBaseMessage<T>(T payload, int agentFromId) where T : IPayload
        {
            return MessageFactory.GetMessage(payload, agentFromId);
        }
    }
}