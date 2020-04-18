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

        private StartGamePayload startGamePayload;

        [SetUp]
        public void Setup()
        {
            AgentConfiguration agentConfiguration = new AgentConfiguration();
            agentConfiguration.WantsToBeTeamLeader = false;
            agentConfiguration.TeamID = "Blue";
            agent = new Agent.Agent(agentConfiguration);
            var teamMates = new int[3] { 2, 3, 4 };
            var enemiesIds = new int[3] { 5, 7, 6 };

            startGamePayload = new StartGamePayload(1, teamMates, 1, enemiesIds, TeamId.Blue, new Point(5, 6), 1, 3, 3, 4, 4, new System.Collections.Generic.Dictionary<ActionType, TimeSpan>(), 0.5f, new Point(0, 0));

            agent.StartGameComponent.Initialize(startGamePayload);
            agent.SetDoNothingStrategy();
            startTime = DateTime.Now;
        }

        [Test]
        public void Set_agent_TeamLeader()
        {
            AgentConfiguration agentConfiguration = new AgentConfiguration();
            agentConfiguration.WantsToBeTeamLeader = false;
            agentConfiguration.TeamID = "Blue";
            agent = new Agent.Agent(agentConfiguration);
            agent.id = 1;
            var teamMates = new int[3] { 2, 3, 4 };
            var enemiesIds = new int[3] { 5, 7, 6 };
            startGamePayload = new StartGamePayload(1, teamMates, 1, enemiesIds, TeamId.Blue, new Point(5,5), 1, 3, 3, 4, 4, new System.Collections.Generic.Dictionary<ActionType, TimeSpan>(), 0.5f, new Point(0, 0));

            agent.StartGameComponent.Initialize(startGamePayload);
            Assert.AreEqual(agent.StartGameComponent.isLeader, true);
        }

        [Test]
        public void Set_other_agent_TeamLeader()
        {
            AgentConfiguration agentConfiguration = new AgentConfiguration();
            agentConfiguration.WantsToBeTeamLeader = true;
            agentConfiguration.TeamID = "Blue";
            agent = new Agent.Agent(agentConfiguration);
            agent.id = 1;
            var teamMates = new int[3] { 2, 3, 4 };
            var enemiesIds = new int[3] { 5, 7, 6 };

            startGamePayload = new StartGamePayload(2, teamMates, 1, enemiesIds, TeamId.Blue, new Point(5, 5), 1, 3, 3, 4, 4, new System.Collections.Generic.Dictionary<ActionType, TimeSpan>(), 0.5f, new Point(0, 0));

            Assert.AreEqual(agent.StartGameComponent.isLeader, false);
        }

        [Test]
        public void Set_agent_boardSize()
        {
            Assert.AreEqual(agent.BoardLogicComponent.BoardSize.X, 5);
            Assert.AreEqual(agent.BoardLogicComponent.BoardSize.Y, 6);
        }

        [Test]
        public void Set_agent_position()
        {
            Assert.AreEqual(agent.BoardLogicComponent.Position.X, 0);
            Assert.AreEqual(agent.BoardLogicComponent.Position.Y, 0);
        }

        [Test]
        public void Is_initialized()
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
        public void Has_correct_board_size()
        {
            Assert.AreEqual(agent.BoardLogicComponent.BoardSize.X, agent.BoardLogicComponent.Board.GetLength(1));
            Assert.AreEqual(agent.BoardLogicComponent.BoardSize.Y, agent.BoardLogicComponent.Board.GetLength(0));
            Assert.True(agent.BoardLogicComponent.Position.X >= 0 && agent.BoardLogicComponent.Position.Y >= 0 && agent.BoardLogicComponent.Position.X < agent.BoardLogicComponent.BoardSize.X && agent.BoardLogicComponent.Position.Y < agent.BoardLogicComponent.BoardSize.Y);
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
            agent.AgentState = AgentState.InGame;

            agent.Piece = new Piece();

            agent.AcceptMessage(GetBaseMessage(new CheckShamResponse(true), 1));

            agent.AcceptMessage(GetBaseMessage(new DestroyPieceResponse(), 1));

            Assert.IsNull(agent.Piece);
        }

        [Test]
        public void ProcessMessage_CheckShamResponse_If_Not_Sham_Agent_Piece_Should_Be_Discovered()
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
        public void ProcessMessage_DiscoverResponse_Should_Update_Agent_Board_State()
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
        public void ProcessMessage_ExchangeInformationPayload_If_Not_TeamLeader_Asking_Should_Be_Added_To_Waiting_List()
        {
            agent.AgentState = AgentState.InGame;

            agent.AcceptMessage(GetBaseMessage(new ExchangeInformationRequestForward(2, false, Messaging.Enumerators.TeamId.Blue), 1));

            Assert.AreEqual(agent.WaitingPlayers.Count, 1);
            Assert.AreEqual(agent.WaitingPlayers[0], 2);
        }


        [Test]
        public void ProcessMessage_ExchangeInformationResponsePayload_Should_Update_Agent_Board_State()
        {
            agent.AgentState = AgentState.InGame;

            var blueGoalAreaInformation = new GoalInformation[,] { { GoalInformation.Goal, GoalInformation.NoGoal, GoalInformation.NoGoal, GoalInformation.NoGoal, GoalInformation.NoInformation } };
            var redGoalAreaInformation = new GoalInformation[,] { { GoalInformation.NoInformation, GoalInformation.NoGoal, GoalInformation.NoGoal, GoalInformation.Goal, GoalInformation.NoInformation } };
            var distances = new int[,] {{ 1, 2, 3, 1, 4 }, { 2, 2, 2, 1, 3 }, { 3, 0, 2, 1, 2 }, { 2, 2, 2, 1, 1 }, { 3, 0, 2, 1, 2 }, { 2, 1, 3, 4, 1 } };

            agent.AcceptMessage(GetBaseMessage(new ExchangeInformationResponseForward(2, distances, redGoalAreaInformation, blueGoalAreaInformation ), 1));

            for(int i = 0; i < agent.BoardLogicComponent.BoardSize.Y; i++)
            {
                for(int j = 0; j < agent.BoardLogicComponent.BoardSize.X; j++)
                {
                    Assert.AreEqual(agent.BoardLogicComponent.Board[i, j].distToPiece, distances[i, j]);
                }
            }

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
        public void ProcessMessage_MoveResponse_When_Move_Made_Agent_Position_Should_Change_And_DistToPiece_Should_Update()
        {
            agent.AgentState = AgentState.InGame;

            agent.AcceptMessage(GetBaseMessage(new MoveResponse(true, new Point(1, 0), 2), 1));

            Assert.AreEqual(agent.BoardLogicComponent.Position, new Point(1, 0));
            Assert.AreEqual(agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece, 2);
        }

        [Test]
        public void ProcessMessage_MoveResponse_When_DistToPiece_Equal_Zero_Agent_Should_PickUp_Piece()
        {
            agent.AgentState = AgentState.InGame;

            agent.AcceptMessage(GetBaseMessage(new MoveResponse(true, new Point(1, 0), 0), 1));

            agent.AcceptMessage(GetBaseMessage(new PickUpPieceResponse(), 1));

            Assert.AreEqual(agent.BoardLogicComponent.Position, new Point(1, 0));
            Assert.AreEqual(agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece, 0);
            Assert.IsNotNull(agent.Piece);
        }

        [Test]
        public void ProcessMessage_MoveResponse_When_Move_Denied_AgentShould_Update_Position_And_Board_State()
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
        public void ProcessMessage_PickUpPieceResponse_When_DistToPiece_Is_Zero_Agent_Should_PickUp_Piece()
        {
            agent.AgentState = AgentState.InGame;

            agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece = 0;

            Assert.IsNull(agent.Piece);

            agent.AcceptMessage(GetBaseMessage(new PickUpPieceResponse(), 1));

            Assert.IsNotNull(agent.Piece);
        }

        [Test]
        public void ProcessMessage_PickUpPieceResponse_When_DistToPiece_Is_Not_Zero_Agent_Should_Not_PickUp_Piece()
        {
            agent.AgentState = AgentState.InGame;

            agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece = 1;

            Assert.IsNull(agent.Piece);

            agent.AcceptMessage(GetBaseMessage(new PickUpPieceResponse(), 1));

            Assert.IsNull(agent.Piece);
        }

        [Test]
        public void ProcessMessage_PickUpPieceResponse_When_PickUpPieceError_DistToPiece_Should_Be_Set_To_Default()
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
        public void ProcessMessage_PutDownPieceResponse_Agent_Should_Not_Have_Piece()
        {
            agent.AgentState = AgentState.InGame;

            agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece = 1;
            agent.Piece = new Piece();

            Assert.IsNotNull(agent.Piece);

            agent.AcceptMessage(GetBaseMessage(new PutDownPieceResponse(PutDownPieceResult.TaskField), 1));

            Assert.IsNull(agent.Piece);
        }

        [Test]
        public void ProcessMessage_PutDownPieceResponse_When_PutDownPieceError_AgentNotHolding_Agent_Should_Not_Have_Piece()
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
        public void Joins_When_Accepted()
        {
            AgentConfiguration agentConfiguration = new AgentConfiguration();
            agentConfiguration.WantsToBeTeamLeader = false;
            agentConfiguration.TeamID = "Blue";
            agent = new Agent.Agent(agentConfiguration);
            agent.SetDoNothingStrategy();
            agent.AgentState = AgentState.WaitingForJoin;
            agent.AcceptMessage(GetBaseMessage(new JoinResponse(true, 1), 1));
            Assert.AreEqual(agent.AgentState, AgentState.WaitingForStart);

        }

        [Test]
        public void Does_Not_Join_When_Rejected()
        {
            AgentConfiguration agnetConfiguration = new AgentConfiguration();
            agnetConfiguration.WantsToBeTeamLeader = false;
            agnetConfiguration.TeamID = "Blue";
            agent = new Agent.Agent(agnetConfiguration);
            agent.SetDoNothingStrategy();
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
    }
}