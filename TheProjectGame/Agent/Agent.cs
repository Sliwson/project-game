using Agent.strategies;
using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.GameMaster;
using Messaging.Enumerators;
using Messaging.Implementation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;

namespace Agent
{
    public class Agent : IMessageProcessor
    {
        public int id;

        private int lastAskedTeammate;

        private ISender sender;

        private IStrategy strategy;

        public int penaltyTime;

        public TeamId team;

        public bool isLeader;

        public Field[,] board;

        public Point boardSize;

        public int goalAreaSize;

        public Point position;

        public List<int> waitingPlayers;

        public int[] teamMates;

        public Piece piece;

        public Agent() { }

        public void Initialize(int leaderId, TeamId teamId, Point boardSize, int goalAreaHeight, Point pos, int[] alliesIds)
        {
            isLeader = id == leaderId ? true : false;
            team = teamId;
            this.boardSize = boardSize;
            board = new Field[boardSize.X, boardSize.Y];
            for(int i = 0; i < boardSize.X; i++)
            {
                for (int j = 0; j < boardSize.Y; j++)
                {
                    board[i, j] = new Field();
                }
            }
            position = pos;
            teamMates = new int[alliesIds.Length];
            teamMates = alliesIds;
            goalAreaSize = goalAreaHeight;
            waitingPlayers = new List<int>();
            piece = null;
            lastAskedTeammate = 0;
            strategy = new SimpleStrategy();
        }
        private void Penalty() 
        {
            Thread.Sleep(penaltyTime);
        }

        private int[,] GetDistances()
        {
            int[,] distances = new int[boardSize.Y, boardSize.X];
            for (int i = 0; i < boardSize.Y; i++)
            {
                for (int j = 0; j < boardSize.X; j++)
                {
                    distances[i, j] = board[i, j].distToPiece;
                }
            }
            return distances;
        }

        private GoalInformation[,] GetBlueTeamGoalAreaInformation()
        {
            GoalInformation[,] goalAreaInformation = new GoalInformation[goalAreaSize, boardSize.X];
            for (int i = 0; i < goalAreaSize ; i++)
            {
                for (int j = 0; j < boardSize.X; j++)
                {
                    goalAreaInformation[i, j] = board[i, j].goalInfo;
                }
            }
            return goalAreaInformation;
        }

        private GoalInformation[,] GetRedTeamGoalAreaInformation()
        {
            GoalInformation[,] goalAreaInformation = new GoalInformation[goalAreaSize, boardSize.X];
            for (int i = boardSize.Y - goalAreaSize + 1; i < boardSize.Y; i++)
            {
                for (int j = 0; j < boardSize.X; j++)
                {
                    goalAreaInformation[i, j] = board[i, j].goalInfo;
                }
            }
            return goalAreaInformation;
        }

        private void UpdateDistances(int[,] distances)
        {
            for (int i = 0; i < boardSize.Y; i++)
            {
                for (int j = 0; j < boardSize.X; j++)
                {
                      board[i, j].distToPiece = distances[i, j];
                }
            }
        }

        private void UpdateBlueTeamGoalAreaInformation(GoalInformation[,] goalAreaInformation)
        {
            for (int i = 0; i < goalAreaSize; i++)
            {
                for (int j = 0; j < boardSize.X; j++)
                {
                   board[i, j].goalInfo = goalAreaInformation[i, j];
                }
            }
        }

        private void UpdateRedTeamGoalAreaInformation(GoalInformation[,] goalAreaInformation)
        {
            for (int i = boardSize.Y - goalAreaSize + 1; i < boardSize.Y; i++)
            {
                for (int j = 0; j < boardSize.X; j++)
                {
                   board[i, j].goalInfo = goalAreaInformation[i, j];
                }
            }
        }

        public void JoinTheGame() 
        {
            SendMessage(MessageFactory.GetMessage(new JoinRequest(team, isLeader)));
            MakeDecisionFromStrategy();
        }

        public void Start() 
        {
            JoinTheGame();
        }

        public void Stop() { }

        public void Move(Direction direction) 
        { 
            SendMessage(MessageFactory.GetMessage(new MoveRequest(direction)));
            MakeDecisionFromStrategy();
        }

        public void PickUp()
        {
            SendMessage(MessageFactory.GetMessage(new PickUpPieceRequest()));
            MakeDecisionFromStrategy();
        }

        public void Put() 
        {
            SendMessage(MessageFactory.GetMessage(new PutDownPieceRequest()));
            MakeDecisionFromStrategy();
        }

        public void BegForInfo() 
        {
            SendMessage(MessageFactory.GetMessage(new ExchangeInformationRequest(teamMates[lastAskedTeammate])));
            MakeDecisionFromStrategy();
        }

        public void GiveInfo()
        { 
            int respondToId = waitingPlayers.Count > 0 ? waitingPlayers[0] : -1;
            if (respondToId == -1)
            {
                MakeDecisionFromStrategy();
                return;
            }
            SendMessage(MessageFactory.GetMessage(new ExchangeInformationResponse(respondToId, GetDistances(), GetRedTeamGoalAreaInformation(), GetBlueTeamGoalAreaInformation())));
            MakeDecisionFromStrategy();
        }

        public void CheckPiece() 
        {
            SendMessage(MessageFactory.GetMessage(new CheckShamRequest()));
            MakeDecisionFromStrategy();
        }

        public void Discover() 
        {
            SendMessage(MessageFactory.GetMessage(new DiscoverRequest()));
            MakeDecisionFromStrategy();
        }

        public void DestroyPiece()
        {
            SendMessage(MessageFactory.GetMessage(new DestroyPieceRequest()));
            MakeDecisionFromStrategy();
        }

        public void MakeDecisionFromStrategy()
        {
            Thread.Sleep(penaltyTime);
            strategy.MakeDecision(this);
        }

        void IMessageProcessor.AcceptMessage(BaseMessage message)
        {
            dynamic dynamicMessage = message;
            Process(dynamicMessage);
        }

        public void SendMessage(BaseMessage message) { }

        private void Process(Message<CheckShamResponse> message)
        {
            if (message.Payload.Sham)
            {
                Penalty();
                DestroyPiece();
            }
            else
            {
                piece.isDiscovered = true;
                MakeDecisionFromStrategy();
            }
        }

        private void Process(Message<DestroyPieceResponse> message)
        {
            piece = null;
            MakeDecisionFromStrategy();
        }

        private void Process(Message<DiscoverResponse> message)
        {
            board[position.Y, position.X].distToPiece = message.Payload.Distances[1, 1];
            board[position.Y + 1, position.X].distToPiece = message.Payload.Distances[0, 1];
            board[position.Y, position.X - 1].distToPiece = message.Payload.Distances[1, 0];
            board[position.Y, position.X + 1].distToPiece = message.Payload.Distances[1, 2];
            board[position.Y - 1, position.X].distToPiece = message.Payload.Distances[2, 1];
            board[position.Y + 1, position.X + 1].distToPiece = message.Payload.Distances[0, 2];
            board[position.Y + 1, position.X - 1].distToPiece = message.Payload.Distances[0, 0];
            board[position.Y - 1, position.X + 1].distToPiece = message.Payload.Distances[2, 2];
            board[position.Y - 1, position.X - 1].distToPiece = message.Payload.Distances[2, 0];
            MakeDecisionFromStrategy();
        }

        private void Process(Message<EndGamePayload> message)
        {
            Stop();
        }
        
        private void Process(Message<ExchangeInformationPayload> message)
        {
            if (message.Payload.Leader)
            {
                GiveInfo();
            }
            else
            {
                waitingPlayers.Add(message.Payload.AskingAgentId);
                MakeDecisionFromStrategy();
            }
        }

        private void Process(Message<ExchangeInformationResponse> message)
        {
            UpdateDistances(message.Payload.Distances);
            UpdateBlueTeamGoalAreaInformation(message.Payload.BlueTeamGoalAreaInformation);
            UpdateRedTeamGoalAreaInformation(message.Payload.RedTeamGoalAreaInformation);

            lastAskedTeammate++;
            lastAskedTeammate %= teamMates.Length;

            MakeDecisionFromStrategy();
        }

        private void Process(Message<JoinResponse> message)
        {
            if (message.Payload.Accepted)
            {
                id = message.Payload.AgentId;
                // wait for start game response
                // get start game response
                // Initialize();
            }
            MakeDecisionFromStrategy();
        }

        private void Process(Message<MoveResponse> message)
        {
            if (message.Payload.MadeMove)
            {
                position = message.Payload.CurrentPosition;
                board[position.Y, position.X].distToPiece = message.Payload.ClosestPoint;
                board[position.Y, position.X].distLearned = DateTime.Now;
                if (message.Payload.ClosestPoint == 0)
                {
                    PickUp();
                    return;
                }
            }
            else
            {
                board[position.Y, position.X].deniedMove = DateTime.Now;
            }
            MakeDecisionFromStrategy();
        }

        private void Process(Message<PickUpPieceResponse> message)
        {
            if (board[position.Y, position.X].distToPiece == 0)
            {
                piece = new Piece();
            }
            MakeDecisionFromStrategy();
        }

        private void Process(Message<PutDownPieceResponse> message)
        {
            board[position.Y, position.X].distToPiece = 0;
            piece = null;
            MakeDecisionFromStrategy();
        }

        private void Process(Message<StartGamePayload> message)
        {
            Start();
        }
    }
}
