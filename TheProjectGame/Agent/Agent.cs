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
    public class Agent
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

        private void Communicate() { }

        public void Initialize(int leaderId, TeamId teamId, Point boardSize, int goalAreaHeight, Point pos, int[] alliesIds)
        {
            isLeader = id == leaderId ? true : false;
            team = teamId;
            this.boardSize = boardSize;
            board = new Field[boardSize.Y, boardSize.X];
            for(int i = 0; i < boardSize.Y; i++)
            {
                for (int j = 0; j < boardSize.X; j++)
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
            var request = MessageFactory.GetMessage(new JoinRequest(team));
            var response = MessageFactory.GetMessage(new JoinResponse(true, 1));
            if(response.Payload.Accepted)
            {
                id = response.Payload.AgentId;
                // wait for start game response
                // get start game response
                // Initialize();
            }
        }

        public void Start() 
        {
            JoinTheGame();
            MakeDecisionFromStrategy();
        }

        public void Stop() { }

        public void Move(Direction direction) 
        { 
            var request = MessageFactory.GetMessage(new MoveRequest(direction));
            var response = MessageFactory.GetMessage(new MoveResponse(true, new Point(1, 1), 2));
            if(response.Payload.MadeMove)
            {
                position = response.Payload.CurrentPosition;
                board[position.Y, position.X].distToPiece = response.Payload.ClosestPoint;
                board[position.Y, position.X].distLearned = DateTime.Now;
                if(response.Payload.ClosestPoint == 0)
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

        public void PickUp()
        {
            var request = MessageFactory.GetMessage(new PickUpPieceRequest());

            var response = MessageFactory.GetMessage(new PickUpPieceResponse());
            if (board[position.Y, position.X].distToPiece == 0)
            {
                piece = new Piece();
            }

            MakeDecisionFromStrategy();
        }

        public void Put() 
        {
            var request = MessageFactory.GetMessage(new PutDownPieceRequest());

            var response = MessageFactory.GetMessage(new PutDownPieceResponse());

            board[position.Y, position.X].distToPiece = 0;
            piece = null;

            MakeDecisionFromStrategy();
        }

        public void BegForInfo() 
        {
            var request = MessageFactory.GetMessage(new ExchangeInformationRequest(teamMates[lastAskedTeammate]));

            var response = MessageFactory.GetMessage(new ExchangeInformationResponse(1, GetDistances(), GetRedTeamGoalAreaInformation(), GetBlueTeamGoalAreaInformation()));

            UpdateDistances(response.Payload.Distances);
            UpdateBlueTeamGoalAreaInformation(response.Payload.BlueTeamGoalAreaInformation);
            UpdateRedTeamGoalAreaInformation(response.Payload.RedTeamGoalAreaInformation);

            lastAskedTeammate++;
            lastAskedTeammate %= teamMates.Length;
        }

        public void GiveInfo() /*to first waiting player*/
        { 
            int respondToId = waitingPlayers.Count > 0 ? waitingPlayers[0] : -1;
            if (respondToId == -1) return;
            
            var response = MessageFactory.GetMessage(new ExchangeInformationResponse(respondToId, GetDistances(), GetRedTeamGoalAreaInformation(), GetBlueTeamGoalAreaInformation()));

            MakeDecisionFromStrategy();
        }

        public void RequestResponse()
        {
            var response = MessageFactory.GetMessage(new ExchangeInformationPayload(1, true, TeamId.Blue));
            if(response.Payload.Leader)
            {
                GiveInfo();
            }
            else
            {
                waitingPlayers.Add(response.Payload.AskingAgentId);
            }
        }

        public void CheckPiece() 
        {
            var request = MessageFactory.GetMessage(new CheckShamRequest());
            /*send and receive*/
            var response = MessageFactory.GetMessage(new CheckShamResponse(true));

            if(response.Payload.Sham)
            {
                var destroyRequest = MessageFactory.GetMessage(new DestroyPieceRequest());
                /*send and receive*/
                var destroyResponse = MessageFactory.GetMessage(new DestroyPieceResponse());
                piece = null;
            }
            else
            {
                piece.isDiscovered = true;
            }

            MakeDecisionFromStrategy();
        }

        public void Discover() 
        {
            var request = MessageFactory.GetMessage(new DiscoverRequest());
            /*send and receive*/
            var response = MessageFactory.GetMessage(new DiscoverResponse(new Distances(1, 1, 1, 1, 1, 1, 1, 1, 1)));

            board[position.Y, position.X].distToPiece = response.Payload.Distances.distanceFromCurrent;
            board[position.Y + 1, position.X].distToPiece = response.Payload.Distances.distanceN;
            board[position.Y, position.X - 1].distToPiece = response.Payload.Distances.distanceW;
            board[position.Y, position.X + 1].distToPiece = response.Payload.Distances.distanceE;
            board[position.Y - 1, position.X].distToPiece = response.Payload.Distances.distanceS;
            board[position.Y + 1, position.X + 1].distToPiece = response.Payload.Distances.distanceNE;
            board[position.Y + 1, position.X - 1].distToPiece = response.Payload.Distances.distanceNW;
            board[position.Y - 1, position.X + 1].distToPiece = response.Payload.Distances.distanceSE;
            board[position.Y - 1, position.X - 1].distToPiece = response.Payload.Distances.distanceSW;
        }

        public void AcceptMessage() { }

        public void MakeDecisionFromStrategy() 
        {
            Thread.Sleep(penaltyTime);
            strategy.MakeDecision(this);
        }

    }
}
