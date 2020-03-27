using Agent;
using Agent.strategies;
using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.GameMaster;
using Messaging.Implementation;
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
            agent = new Agent.Agent();
            var teamMates = new int[3] { 2, 3, 4 };
            agent.Initialize(1, Messaging.Enumerators.TeamId.Blue, new System.Drawing.Point(5, 5), 1, new System.Drawing.Point(0, 0), teamMates);
            agent.SetDoNothingStrategy();
        }

        [Test]
        public void Set_agent_TeamLeader()
        {
            agent = new Agent.Agent();
            agent.id = 1;
            agent.Initialize(1, Messaging.Enumerators.TeamId.Blue, new System.Drawing.Point(5, 5), 1, new System.Drawing.Point(0, 0), new int[3] { 2, 3, 4 });
            Assert.AreEqual(agent.isLeader, true);
        }

        [Test]
        public void Set_other_agent_TeamLeader()
        {
            var agent = new Agent.Agent();
            agent.id = 1;
            var teamMates = new int[3] { 2, 3, 4 };
            agent.Initialize(2, Messaging.Enumerators.TeamId.Blue, new System.Drawing.Point(5, 5), 1, new System.Drawing.Point(0, 0), teamMates);
            Assert.AreEqual(agent.isLeader, false);
        }

        [Test]
        public void Set_agent_boardSize()
        {
            Assert.AreEqual(agent.boardSize.X, 5);
            Assert.AreEqual(agent.boardSize.Y, 5);
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
        public void Has_coorect_board_size()
        {
            Assert.AreEqual(agent.boardSize.X, agent.board.GetLength(0));
            Assert.AreEqual(agent.boardSize.Y, agent.board.GetLength(1));
            Assert.True(agent.position.X >= 0 && agent.position.Y >= 0 && agent.position.X < agent.boardSize.X && agent.position.Y < agent.boardSize.Y);
        }

        #region Join

        [Test]
        public void ProcessMessage_JoinRequest_When_Accepted_Should_Set_Agent_Id()
        {
            Assert.AreEqual(agent.id, 0);

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
            agent.piece = new Piece();

            agent.AcceptMessage(GetBaseMessage(new CheckShamResponse(true), 1));

            agent.AcceptMessage(GetBaseMessage(new DestroyPieceResponse(), 1));

            Assert.IsNull(agent.piece);
        }

        [Test]
        public void ProcessMessage_CheckShamResponse_If_Not_Sham_Agent_Piece_Should_Be_Discovered()
        {
            agent.piece = new Piece();

            Assert.IsFalse(agent.piece.isDiscovered);

            agent.AcceptMessage(GetBaseMessage(new CheckShamResponse(false), 1));

            Assert.IsTrue(agent.piece.isDiscovered);
        }

        #endregion

        #region Discover

        [Test]
        public void ProcessMessage_DiscoverResponse_()
        {
            agent.AcceptMessage(GetBaseMessage(new DiscoverResponse(new int[,] { { 1, 2, 3 }, { 2, 2, 2 }, { 3, 0, 2 } }), 1));
            var position = agent.position;

            Assert.AreEqual(agent.board[position.Y, position.X].distToPiece, 2);
            if (position.Y + 1 < agent.boardSize.Y)
            {
                Assert.AreEqual(agent.board[position.Y + 1, position.X].distToPiece, 2);
                if (position.X + 1 < agent.boardSize.X)
                {
                    Assert.AreEqual(agent.board[position.Y + 1, position.X + 1].distToPiece, 3);
                }
                if (position.X - 1 >= 0)
                {
                    Assert.AreEqual(agent.board[position.Y + 1, position.X - 1].distToPiece, 1);
                }
            }
            if (position.X - 1 >= 0)
            {
                Assert.AreEqual(agent.board[position.Y, position.X - 1].distToPiece, 2);
                if (position.Y - 1 >= 0)
                {
                    Assert.AreEqual(agent.board[position.Y - 1, position.X - 1].distToPiece, 3);
                }
            }
            if (position.Y - 1 >= 0)
            {
                Assert.AreEqual(agent.board[position.Y - 1, position.X].distToPiece, 0);
                if (position.X + 1 < agent.boardSize.X)
                {
                    Assert.AreEqual(agent.board[position.Y - 1, position.X + 1].distToPiece, 2);
                }
            }
            if (position.X + 1 < agent.boardSize.X)
            {
                Assert.AreEqual(agent.board[position.Y, position.X + 1].distToPiece, 2);
            }
        }

        #endregion

        // This method simulates normal situation where messages are stored in IEnumerable<BaseMessage>
        private BaseMessage GetBaseMessage<T>(T payload, int agentFromId) where T : IPayload
        {
            return MessageFactory.GetMessage(payload, agentFromId);
        }
    }
}