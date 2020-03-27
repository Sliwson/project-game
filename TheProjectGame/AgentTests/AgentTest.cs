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
            agent = new Agent.Agent();
            agent.id = 1;
            var teamMates = new int[3] { 2, 3, 4 };
            agent.Initialize(1, Messaging.Enumerators.TeamId.Blue, new System.Drawing.Point(5, 5), 1, new System.Drawing.Point(0, 0), teamMates);
        }

        [Test]
        public void Set_agent_TeamLeader()
        {
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
        public void Agent_should_not_pickUp_piece_if_diatance_is_not_zero()
        {
            var agent = new Agent.Agent();
            agent.id = 1;
            var teamMates = new int[3] { 2, 3, 4 };
            agent.Initialize(2, Messaging.Enumerators.TeamId.Blue, new System.Drawing.Point(5, 5), 1, new System.Drawing.Point(0, 0), teamMates);
            agent.board[agent.position.X, agent.position.Y].distToPiece = 1;
            Assert.IsNull(agent.piece);
            agent.PickUp();
            Assert.IsNull(agent.piece);
        }

        [Test]
        public void Agent_should_putDown_piece()
        {
            var agent = new Agent.Agent();
            agent.id = 1;
            var teamMates = new int[3] { 2, 3, 4 };
            agent.Initialize(2, Messaging.Enumerators.TeamId.Blue, new System.Drawing.Point(5, 5), 1, new System.Drawing.Point(0, 0), teamMates);
            agent.board[agent.position.X, agent.position.Y].distToPiece = 1;
            agent.piece = new Piece();
            Assert.IsNotNull(agent.piece);
            agent.Put();
            Assert.IsNull(agent.piece);
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
    }
}