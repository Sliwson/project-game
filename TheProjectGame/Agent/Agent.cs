using Agent.strategies;
using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.Errors;
using Messaging.Contracts.GameMaster;
using Messaging.Enumerators;
using Messaging.Implementation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace Agent
{
    public class Agent : IMessageProcessor
    {
        private const bool endIfUnexpectedMessage = false;

        private const bool endIfUnexpectedAction = false;

        public int id;

        private int lastAskedTeammate;

        private Direction lastDirection;

        private ISender sender;

        private IStrategy strategy;

        private List<BaseMessage> injectedMessages;

        public int penaltyTime;

        public TeamId team;

        public bool isLeader;

        public bool wantsToBeLeader;

        public Field[,] board;

        public Point boardSize;

        public int goalAreaSize;

        public Point position;

        public List<int> waitingPlayers;

        public int[] teamMates;

        public Dictionary<ActionType, TimeSpan> penalties;

        public int averageTime;

        public float shamPieceProbability;

        public Piece piece;

        public AgentState agentState;

        public Agent(bool wantsToBeLeader)
        {
            this.wantsToBeLeader = wantsToBeLeader;
            penaltyTime = 0;
            piece = null;
            lastAskedTeammate = 0;
            waitingPlayers = new List<int>();
            strategy = new SimpleStrategy();
            injectedMessages = new List<BaseMessage>();
            agentState = AgentState.Created;
        }

        public void Initialize(int leaderId, TeamId teamId, Point boardSize, int goalAreaHeight, Point pos, int[] alliesIds, Dictionary<ActionType, TimeSpan> penalties, float shamPieceProbability)
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
            this.penalties = penalties;
            averageTime = penalties.Count > 0 ? (int)penalties.Values.Max().TotalMilliseconds : 500;
            this.shamPieceProbability = shamPieceProbability;
        }

        private void Penalty() 
        {
            Thread.Sleep(penaltyTime);
            penaltyTime = 0;
        }

        private void SetPenalty(ActionType action)
        {
            penaltyTime = penalties.TryGetValue(action, out TimeSpan span) ? (int)span.TotalMilliseconds : 0;
        }

        public void SetDoNothingStrategy()
        {
            this.strategy = new DoNothingStrategy();
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
            //TODO: update only when distLearned old
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
                   if (board[i, j].goalInfo == GoalInformation.NoInformation) board[i, j].goalInfo = goalAreaInformation[i, j];
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

        private bool WaitForJoin()
        {
            if (agentState != AgentState.WaitingForJoin) return true;
            return AcceptMessage(WaitForMessage(typeof(Message<JoinResponse>)));
        }

        private bool WaitForStart()
        {
            if (agentState != AgentState.WaitingForStart) return true;
            return AcceptMessage(WaitForMessage(typeof(Message<StartGamePayload>)));
        }

        private void MainLoop()
        {
            while (true)
            {
                BaseMessage message = WaitForMessage();
                if (AcceptMessage(message)) break;
                Penalty();
            }
        }

        public void JoinTheGame()
        {
            if (agentState != AgentState.Created) return;
            agentState = AgentState.WaitingForJoin;
            SendMessage(MessageFactory.GetMessage(new JoinRequest(team, wantsToBeLeader)));
            if (strategy is DoNothingStrategy) return;
            if (WaitForJoin()) return;
            if (WaitForStart()) return;
            Penalty();
            MainLoop();
        }

        public bool Move(Direction direction)
        {
            if (agentState != AgentState.InGame && endIfUnexpectedAction) return true;
            lastDirection = direction;
            SetPenalty(ActionType.Move);
            SendMessage(MessageFactory.GetMessage(new MoveRequest(direction)));
            return false;
        }

        public bool PickUp()
        {
            if (agentState != AgentState.InGame && endIfUnexpectedAction) return true;
            SendMessage(MessageFactory.GetMessage(new PickUpPieceRequest()));
            return false;
        }

        public bool Put()
        {
            if (agentState != AgentState.InGame && endIfUnexpectedAction) return true;
            SetPenalty(ActionType.PutPiece);
            SendMessage(MessageFactory.GetMessage(new PutDownPieceRequest()));
            return false;
        }

        public bool BegForInfo()
        {
            if (agentState != AgentState.InGame && endIfUnexpectedAction) return true;
            lastAskedTeammate++;
            lastAskedTeammate %= teamMates.Length;
            SetPenalty(ActionType.InformationRequest);
            SendMessage(MessageFactory.GetMessage(new ExchangeInformationRequest(teamMates[lastAskedTeammate])));
            return false;
        }

        public bool GiveInfo()
        {
            if (agentState != AgentState.InGame && endIfUnexpectedAction) return true;
            int respondToId = waitingPlayers.Count > 0 ? waitingPlayers[0] : -1;
            if (respondToId == -1 && endIfUnexpectedAction) return true;
            SetPenalty(ActionType.InformationResponse);
            SendMessage(MessageFactory.GetMessage(new ExchangeInformationResponse(respondToId, GetDistances(), GetRedTeamGoalAreaInformation(), GetBlueTeamGoalAreaInformation())));
            return false;
        }

        public bool CheckPiece()
        {
            if (agentState != AgentState.InGame && endIfUnexpectedAction) return true;
            SetPenalty(ActionType.CheckForSham);
            SendMessage(MessageFactory.GetMessage(new CheckShamRequest()));
            return false;
        }

        public bool Discover()
        {
            if (agentState != AgentState.InGame && endIfUnexpectedAction) return true;
            SetPenalty(ActionType.Discovery);
            SendMessage(MessageFactory.GetMessage(new DiscoverRequest()));
            return false;
        }

        public bool DestroyPiece()
        {
            if (agentState != AgentState.InGame && endIfUnexpectedAction) return true;
            SetPenalty(ActionType.DestroyPiece);
            SendMessage(MessageFactory.GetMessage(new DestroyPieceRequest()));
            return false;
        }

        public bool MakeDecisionFromStrategy()
        {
            return strategy.MakeDecision(this);
        }

        private BaseMessage GetMessage()
        {
            if (injectedMessages.Count == 0) return null;
            var message = injectedMessages[0];
            injectedMessages.RemoveAt(0);
            return message;
        }

        private BaseMessage GetMessage(Type type)
        {
            var message = injectedMessages.FirstOrDefault(m => m.GetType() == type);
            if (message != null) injectedMessages.Remove(message);
            return message;
        }

        private BaseMessage WaitForMessage()
        {
            BaseMessage message = GetMessage();
            while (message == null)
            {
                Thread.Sleep(50);
                message = GetMessage();
            }
            return message;
        }

        private BaseMessage WaitForMessage(Type type)
        {
            BaseMessage message = GetMessage(type);
            while (message == null)
            {
                Thread.Sleep(50);
                message = GetMessage(type);
            }
            return message;
        }

        public void InjectMessage(BaseMessage message)
        {
            injectedMessages.Add(message);
        }

        public void SendMessage(BaseMessage message) { }

        public bool AcceptMessage(BaseMessage message)
        {
            dynamic dynamicMessage = message;
            return Process(dynamicMessage);
        }

        private bool Process(Message<CheckShamResponse> message)
        {
            if (agentState != AgentState.InGame && endIfUnexpectedMessage) return true;
            if (message.Payload.Sham)
            {
                return DestroyPiece();
            }
            else
            {
                piece.isDiscovered = true;
                return MakeDecisionFromStrategy();
            }
        }

        private bool Process(Message<DestroyPieceResponse> message)
        {
            if (agentState != AgentState.InGame && endIfUnexpectedMessage) return true;
            piece = null;
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<DiscoverResponse> message)
        {
            if (agentState != AgentState.InGame && endIfUnexpectedMessage) return true;
            if (Common.OnBoard(new Point(position.X, position.Y), boardSize)) board[position.Y, position.X].distToPiece = message.Payload.Distances[1, 1];
            if (Common.OnBoard(new Point(position.X, position.Y + 1), boardSize)) board[position.Y + 1, position.X].distToPiece = message.Payload.Distances[0, 1];
            if (Common.OnBoard(new Point(position.X - 1, position.Y), boardSize)) board[position.Y, position.X - 1].distToPiece = message.Payload.Distances[1, 0];
            if (Common.OnBoard(new Point(position.X + 1, position.Y), boardSize)) board[position.Y, position.X + 1].distToPiece = message.Payload.Distances[1, 2];
            if (Common.OnBoard(new Point(position.X, position.Y - 1), boardSize)) board[position.Y - 1, position.X].distToPiece = message.Payload.Distances[2, 1];
            if (Common.OnBoard(new Point(position.X + 1, position.Y + 1), boardSize)) board[position.Y + 1, position.X + 1].distToPiece = message.Payload.Distances[0, 2];
            if (Common.OnBoard(new Point(position.X - 1, position.Y + 1), boardSize)) board[position.Y + 1, position.X - 1].distToPiece = message.Payload.Distances[0, 0];
            if (Common.OnBoard(new Point(position.X + 1, position.Y - 1), boardSize)) board[position.Y - 1, position.X + 1].distToPiece = message.Payload.Distances[2, 2];
            if (Common.OnBoard(new Point(position.X - 1, position.Y - 1), boardSize)) board[position.Y - 1, position.X - 1].distToPiece = message.Payload.Distances[2, 0];
            DateTime now = DateTime.Now;
            for (int i = position.X - 1; i <= position.X + 1; i++)
                for (int j = position.Y - 1; j <= position.Y + 1; j++)
                    if (Common.OnBoard(new Point(i, j), boardSize))
                        board[j, i].distLearned = now;
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<ExchangeInformationResponse> message)
        {
            if (agentState != AgentState.InGame && endIfUnexpectedMessage) return true;
            UpdateDistances(message.Payload.Distances);
            UpdateBlueTeamGoalAreaInformation(message.Payload.BlueTeamGoalAreaInformation);
            UpdateRedTeamGoalAreaInformation(message.Payload.RedTeamGoalAreaInformation);
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<MoveResponse> message)
        {
            if (agentState != AgentState.InGame && endIfUnexpectedMessage) return true;
            if (message.Payload.MadeMove)
            {
                position = message.Payload.CurrentPosition;
                board[position.Y, position.X].distToPiece = message.Payload.ClosestPoint;
                board[position.Y, position.X].distLearned = DateTime.Now;
                if (message.Payload.ClosestPoint == 0)
                {
                    return PickUp();
                }
            }
            else
            {
                var denied = Common.GetFieldInDirection(position, lastDirection);
                board[denied.Y, denied.X].deniedMove = DateTime.Now;
            }
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<PickUpPieceResponse> message)
        {
            if (agentState != AgentState.InGame && endIfUnexpectedMessage) return true;
            if (board[position.Y, position.X].distToPiece == 0)
            {
                piece = new Piece();
            }
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<PutDownPieceResponse> message)
        {
            if (agentState != AgentState.InGame && endIfUnexpectedMessage) return true;
            piece = null;
            board[position.Y, position.X].distToPiece = 0;
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<ExchangeInformationPayload> message)
        {
            if (agentState != AgentState.InGame && endIfUnexpectedMessage) return true;
            if (message.Payload.Leader)
            {
                return GiveInfo();
            }
            else
            {
                waitingPlayers.Add(message.Payload.AskingAgentId);
                return MakeDecisionFromStrategy();
            }
        }

        private bool Process(Message<JoinResponse> message)
        {
            if (agentState != AgentState.WaitingForJoin && endIfUnexpectedMessage) return true;
            if (message.Payload.Accepted)
            {
                agentState = AgentState.WaitingForStart;
                id = message.Payload.AgentId;
                return MakeDecisionFromStrategy();
            }
            else
            {
                return true;
            }
        }

        private bool Process(Message<StartGamePayload> message)
        {
            if (agentState != AgentState.WaitingForStart && endIfUnexpectedMessage) return true;
            Initialize(message.Payload.LeaderId, message.Payload.TeamId, message.Payload.BoardSize, message.Payload.GoalAreaHeight, message.Payload.Position, message.Payload.AlliesIds, message.Payload.Penalties, message.Payload.ShamPieceProbability);
            //TODO: if (id != message.Payload.AgentID) log.warning
            agentState = AgentState.InGame;
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<EndGamePayload> message)
        {
            return true;
        }

        private bool Process(Message<IgnoredDelayError> message)
        {
            var time = message.Payload.WaitUntil - DateTime.Now;
            if (time.CompareTo(TimeSpan.Zero) > 0) Thread.Sleep(time);
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<MoveError> message)
        {
            position = message.Payload.Position;
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<PickUpPieceError> message)
        {
            board[position.Y, position.X].distLearned = DateTime.Now;
            board[position.Y, position.X].distToPiece = int.MaxValue;
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<PutDownPieceError> message)
        {
            if (message.Payload.ErrorSubtype == PutDownPieceErrorSubtype.AgentNotHolding) piece = null;
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<UndefinedError> message)
        {
            return MakeDecisionFromStrategy();
        }
    }
}
