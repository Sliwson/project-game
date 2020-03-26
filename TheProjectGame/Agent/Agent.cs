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
        private int id;

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

        private void Initialize()
        {
            waitingPlayers = new List<int>();
            piece = null;
            lastAskedTeammate = 0;
        }
        private void Penalty() 
        {
            Thread.Sleep(penaltyTime);
        }

        private int[,] GetDistances()
        {
            int[,] distances = new int[boardSize.X, boardSize.Y];
            for (int i = 0; i < boardSize.X; i++)
            {
                for (int j = 0; j < boardSize.Y; j++)
                {
                    distances[i, j] = board[i, j].distToPiece;
                }
            }
            return distances;
        }

        private GoalInformation[,] GetBlueTeamGoalAreaInformation()
        {
            GoalInformation[,] goalAreaInformation = new GoalInformation[boardSize.X, goalAreaSize];
            for (int i = 0; i < boardSize.X; i++)
            {
                for (int j = 0; j < goalAreaSize; j++)
                {
                    goalAreaInformation[i, j] = board[i, j].goalInfo;
                }
            }
            return goalAreaInformation;
        }

        private GoalInformation[,] GetRedTeamGoalAreaInformation()
        {
            GoalInformation[,] goalAreaInformation = new GoalInformation[boardSize.X, goalAreaSize];
            for (int i = 0; i < boardSize.X; i++)
            {
                for (int j = boardSize.Y - goalAreaSize + 1; j < boardSize.Y; j++)
                {
                    goalAreaInformation[i, j] = board[i, j].goalInfo;
                }
            }
            return goalAreaInformation;
        }

        public void JoinTheGame() 
        {
            var request = MessageFactory.GetMessage(new JoinRequest(team));
            var response = MessageFactory.GetMessage(new JoinResponse(true, 1));
            if(response.Payload.Accepted)
            {
                id = response.Payload.AgentId;
            }
        }

        public void Start() 
        {
            MakeDecisionFromStrategy();
        }

        public void Stop() { }

        public void Move(Direction direction) 
        { /*if distance=0 send pick up request*/
            var request = MessageFactory.GetMessage(new MoveRequest(direction));
            var response = MessageFactory.GetMessage(new MoveResponse(true, new Point(1, 1), 2));
            if(response.Payload.MadeMove)
            {
                position = response.Payload.CurrentPosition;
                board[position.X, position.Y].distToPiece = response.Payload.ClosestPoint;
                board[position.X, position.Y].distLearned = DateTime.Now;
                if(response.Payload.ClosestPoint == 0)
                {
                    PickUp();
                }
            }
            else
            {
                board[position.X, position.Y].deniedMove = DateTime.Now;
            }
        }

        public void PickUp()
        {
            var request = MessageFactory.GetMessage(new PickUpPieceRequest());

            var response = MessageFactory.GetMessage(new PickUpPieceResponse());

            piece = new Piece();

            MakeDecisionFromStrategy();
        }

        public void Put() 
        {
            var request = MessageFactory.GetMessage(new PutDownPieceRequest());

            var response = MessageFactory.GetMessage(new PutDownPieceResponse());

            board[position.X, position.Y].distToPiece = 0;
            piece = null;

            if(board[position.X, position.Y].goalInfo == GoalInformation.Goal)
            {
                //goal
            }

            if (board[position.X, position.Y].goalInfo == GoalInformation.NoGoal)
            {
                //noGoal
            }

            MakeDecisionFromStrategy();
        }

        public void BegForInfo() 
        {
            var request = MessageFactory.GetMessage(new ExchangeInformationRequest(teamMates[lastAskedTeammate]));

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

            board[position.X, position.Y].distToPiece = response.Payload.Distances.distanceFromCurrent;
            board[position.X+1, position.Y].distToPiece = response.Payload.Distances.distanceE ;
            board[position.X, position.Y-1].distToPiece = response.Payload.Distances.distanceS;
            board[position.X, position.Y+1].distToPiece = response.Payload.Distances.distanceN;
            board[position.X-1, position.Y].distToPiece = response.Payload.Distances.distanceW;
            board[position.X + 1, position.Y+1].distToPiece = response.Payload.Distances.distanceNE;
            board[position.X+1, position.Y-1].distToPiece = response.Payload.Distances.distanceSE;
            board[position.X-1, position.Y+1].distToPiece = response.Payload.Distances.distanceNW;
            board[position.X-1, position.Y-1].distToPiece = response.Payload.Distances.distanceSW;
        }

        public void AcceptMessage() { }

        public void MakeDecisionFromStrategy() { }

    }
}
