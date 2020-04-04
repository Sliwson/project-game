using Agent.Interfaces;
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

        private const int sleepInterval = 50;

        private const int maxSkipCount = int.MaxValue;

        private int skipCount;

        public int id;

        private int lastAskedTeammate;

        public Direction lastDirection;

        private ISender sender;

        private IStrategy strategy;

        private List<BaseMessage> injectedMessages;

        public int penaltyTime;

        private DateTime waitUntil;

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

        private static NLog.Logger logger;

        public string CsIP;

        public string CsPort;

        public bool deniedLastMove;

        private bool runningAsync = false;

        public Action<Agent, BaseMessage> MockMessageSendFunction { get; set; }

        public Agent(bool wantsToBeLeader = false)
        {
            this.wantsToBeLeader = wantsToBeLeader;
            penaltyTime = 0;
            piece = null;
            lastAskedTeammate = 0;
            deniedLastMove = false;
            waitUntil = DateTime.MinValue;
            skipCount = 0;
            waitingPlayers = new List<int>();
            strategy = new SimpleStrategy();
            injectedMessages = new List<BaseMessage>();
            agentState = AgentState.Created;
            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public void Initialize(int leaderId, TeamId teamId, Point boardSize, int goalAreaHeight, Point pos, int[] alliesIds, Dictionary<ActionType, TimeSpan> penalties, float shamPieceProbability)
        {
            isLeader = id == leaderId ? true : false;
            team = teamId;
            this.boardSize = boardSize;
            board = new Field[boardSize.Y, boardSize.X];
            for (int i = 0; i < boardSize.Y; i++)
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
            logger.Info("Initialize: Agent initialized" + " AgentID: " + id.ToString());
        }

        private void Penalty()
        {
            if (!runningAsync)
                logger.Error("Running sleep in single threaded process, id: " + " AgentID: " + id.ToString());
            Thread.Sleep(penaltyTime);
            penaltyTime = 0;
        }

        private void SetPenalty(ActionType action)
        {
            var ret = penalties.TryGetValue(action, out TimeSpan span);
            if (ret)
            {
                penaltyTime = (int)span.TotalMilliseconds;
                waitUntil = DateTime.Now + span;
            }
            else
            {
                penaltyTime = 0;
                waitUntil = DateTime.Now;
            }
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
            for (int i = 0; i < goalAreaSize; i++)
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
                    goalAreaInformation[i - boardSize.Y + goalAreaSize, j] = board[i, j].goalInfo;
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
                    board[i, j].goalInfo = goalAreaInformation[i - boardSize.Y + goalAreaSize, j];
                }
            }
        }

        private bool WaitForJoin()
        {
            if (agentState != AgentState.WaitingForJoin)
            {
                logger.Warn("Wait for join: Agent not waiting for join. Agent state: " + agentState.ToString() + " AgentID: " + id.ToString());
                return true;
            }
            return AcceptMessage(WaitForMessage(MessageId.JoinResponse));
        }

        private bool WaitForStart()
        {
            if (agentState != AgentState.WaitingForStart)
            {
                logger.Warn("Wait for start: Agent not waiting for start. Agent state: " + agentState.ToString() + " AgentID: " + id.ToString());
                return true;
            }
            return AcceptMessage(WaitForMessage(MessageId.StartGameMessage));
        }

        private void MainLoop()
        {
            while (true)
            {
                BaseMessage message = GetMessage();
                if (message == null && skipCount < maxSkipCount)
                {
                    skipCount++;
                    continue;
                }
                skipCount = 0;
                bool ret = message == null ? MakeDecisionFromStrategy() : AcceptMessage(message);
                if (ret) break;
                Thread.Sleep(500);
                //Penalty();
            }
            logger.Warn("Main loop break");
        }

        public void JoinTheGame()
        {
            if (agentState != AgentState.Created) { logger.Error("Join the game: Agent is not created. Not join the game. Agent state: " + agentState.ToString() + " AgentID: " + id.ToString()); return; }
            runningAsync = true;
            agentState = AgentState.WaitingForJoin;
            SendMessage(MessageFactory.GetMessage(new JoinRequest(team, wantsToBeLeader)));
            if (strategy is DoNothingStrategy) { logger.Error("Join the game: Not join the game. Strategy is DoNothing." + " AgentID: " + id.ToString()); return; }
            if (WaitForJoin()) { logger.Error("Join the game: Not join the game." + " AgentID: " + id.ToString()); return; }
            if (WaitForStart()) { logger.Error("Join the game: Not join the game." + " AgentID: " + id.ToString()); return; }
            Penalty();
            MainLoop();
        }

        public void Update()
        {
            if (runningAsync)
                logger.Error("Running update in multi threaded process, id: " + " AgentID: " + id.ToString());
            runningAsync = false;
            var time = waitUntil - DateTime.Now;
            if (time.CompareTo(TimeSpan.Zero) > 0) return;
            switch (agentState)
            {
                case AgentState.Created:
                    SendMessage(MessageFactory.GetMessage(new JoinRequest(team, wantsToBeLeader)));
                    agentState = AgentState.WaitingForJoin;
                    break;
                case AgentState.WaitingForJoin:
                    var joinResponse = GetMessage(MessageId.JoinResponse);
                    if (joinResponse == null) return;
                    if (AcceptMessage(joinResponse))
                    {
                        waitUntil = DateTime.MaxValue;
                        return;
                    }
                    break;
                case AgentState.WaitingForStart:
                    var startResponse = GetMessage(MessageId.StartGameMessage);
                    if (startResponse == null) return;
                    if (AcceptMessage(startResponse))
                    {
                        waitUntil = DateTime.MaxValue;
                        return;
                    }
                    break;
                case AgentState.InGame:
                    BaseMessage message = GetMessage();
                    if (message == null && skipCount < maxSkipCount)
                    {
                        skipCount++;
                        return;
                    }
                    skipCount = 0;
                    bool ret = message == null ? MakeDecisionFromStrategy() : AcceptMessage(message);
                    if (ret)
                    {
                        waitUntil = DateTime.MaxValue;
                        return;
                    }
                    break;
                default:
                    logger.Error("Agent in unknown state: " + agentState.ToString() + " AgentID: " + id.ToString());
                    break;
            }
        }

        public bool Move(Direction direction)
        {
            if (agentState != AgentState.InGame)
            {
                logger.Warn("Move: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return true;
            }
            lastDirection = direction;
            SetPenalty(ActionType.Move);
            SendMessage(MessageFactory.GetMessage(new MoveRequest(direction)));
            logger.Info("Move: Agent sent move request in direction " + direction.ToString() + " AgentID: " + id.ToString());
            return false;
        }

        public bool PickUp()
        {
            if (agentState != AgentState.InGame)
            {
                logger.Warn("Pick up: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return true;
            }
            SendMessage(MessageFactory.GetMessage(new PickUpPieceRequest()));
            logger.Info("Pick up: Agent sent pick up piece request." + " AgentID: " + id.ToString());
            return false;
        }

        public bool Put()
        {
            if (agentState != AgentState.InGame)
            {
                logger.Warn("Put: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return true;
            }
            SetPenalty(ActionType.PutPiece);
            SendMessage(MessageFactory.GetMessage(new PutDownPieceRequest()));
            logger.Info("Put: Agent sent put down piece request." + " AgentID: " + id.ToString());
            return false;
        }

        public bool BegForInfo()
        {
            if (agentState != AgentState.InGame)
            {
                logger.Warn("Beg for info: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return true;
                return MakeDecisionFromStrategy();
            }
            if (teamMates.Length == 0)
            {
                logger.Warn("Beg for info: Agent does not know his teammates" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return true;
                return MakeDecisionFromStrategy();
            }
            lastAskedTeammate++;
            lastAskedTeammate %= teamMates.Length;
            SetPenalty(ActionType.InformationRequest);
            SendMessage(MessageFactory.GetMessage(new ExchangeInformationRequest(teamMates[lastAskedTeammate])));
            logger.Info("Beg for info: Agent sent exchange information request." + " AgentID: " + id.ToString());
            return false;
        }

        public bool GiveInfo(int respondToId = -1)
        {
            if (agentState != AgentState.InGame)
            {
                logger.Warn("Give info: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return true;
            }
            if (respondToId == -1 && waitingPlayers.Count > 0)
            {
                respondToId = waitingPlayers[0];
                waitingPlayers.RemoveAt(0);
                logger.Info("Give info: ResponfdId is -1. Respond to first waiting player." + " AgentID: " + id.ToString());
            }
            if (respondToId == -1)
            {
                logger.Error("Give info: Respond to id -1 while give info" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return true;
            }
            else if (respondToId == -1) return MakeDecisionFromStrategy();
            SetPenalty(ActionType.InformationResponse);
            SendMessage(MessageFactory.GetMessage(new ExchangeInformationResponse(respondToId, GetDistances(), GetRedTeamGoalAreaInformation(), GetBlueTeamGoalAreaInformation())));
            logger.Info("Give info: Agent sent exchange information response to adentId: " + respondToId.ToString() + " AgentID: " + id.ToString());
            return false;
        }

        public bool CheckPiece()
        {
            if (agentState != AgentState.InGame)
            {
                logger.Warn("Check piece: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return true;
            }
            SetPenalty(ActionType.CheckForSham);
            SendMessage(MessageFactory.GetMessage(new CheckShamRequest()));
            logger.Info("Check piece: Agent sent check scham request." + " AgentID: " + id.ToString());
            return false;
        }

        public bool Discover()
        {
            if (agentState != AgentState.InGame)
            {
                logger.Warn("Discover: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return true;
            }
            SetPenalty(ActionType.Discovery);
            SendMessage(MessageFactory.GetMessage(new DiscoverRequest()));
            logger.Info("Discover: Agent sent discover request." + " AgentID: " + id.ToString());
            return false;
        }

        public bool DestroyPiece()
        {
            if (agentState != AgentState.InGame)
            {
                logger.Warn("Destroy Piece: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return true;
            }
            SetPenalty(ActionType.DestroyPiece);
            SendMessage(MessageFactory.GetMessage(new DestroyPieceRequest()));
            logger.Info("Destroy Piece: Agent sent destroy piece request." + " AgentID: " + id.ToString());
            return false;
        }

        public bool MakeDecisionFromStrategy()
        {
            return strategy.MakeDecision(this);
        }

        private BaseMessage GetMessage()
        {
            if (injectedMessages.Count == 0)
            {
                return null;
            }
            var message = injectedMessages.FirstOrDefault(m => m.PayloadType == typeof(EndGamePayload));
            if (message == null) message = injectedMessages[0];
            injectedMessages.Remove(message);
            return message;
        }

        private BaseMessage GetMessage(MessageId messageId)
        {
            var message = injectedMessages.FirstOrDefault(m => m.MessageId == messageId);
            if (message == null) message = injectedMessages.FirstOrDefault(m => m.MessageId == messageId);
            if (message != null) injectedMessages.Remove(message);
            return message;
        }

        private BaseMessage WaitForMessage()
        {
            if (!runningAsync)
                logger.Error("Running sleep in single threaded process, id: " + " AgentID: " + id.ToString());
            BaseMessage message = GetMessage();
            while (message == null)
            {
                Thread.Sleep(sleepInterval);
                message = GetMessage();
            }
            return message;
        }

        private BaseMessage WaitForMessage(MessageId messageId)
        {
            if (!runningAsync)
                logger.Error("Running sleep in single threaded process, id: " + " AgentID: " + id.ToString());
            BaseMessage message = GetMessage(messageId);
            while (message == null)
            {
                Thread.Sleep(sleepInterval);
                message = GetMessage(messageId);
            }
            return message;
        }

        public void InjectMessage(BaseMessage message)
        {
            injectedMessages.Add(message);
        }

        public void SendMessage(BaseMessage message)
        {
            MockMessageSendFunction?.Invoke(this, message);
        }

        public bool AcceptMessage(BaseMessage message)
        {
            dynamic dynamicMessage = message;
            return Process(dynamicMessage);
        }

        private bool Process(Message<CheckShamResponse> message)
        {
            if (agentState != AgentState.InGame)
            {
                logger.Warn("Process check scham response: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedMessage) return true;
            }
            if (message.Payload.Sham)
            {
                logger.Info("Process check scham response: Agent checked sham and destroy piece." + " AgentID: " + id.ToString());
                return DestroyPiece();
            }
            else
            {
                logger.Info("Process check scham response: Agent checked not sham." + " AgentID: " + id.ToString());
                piece.isDiscovered = true;
                return MakeDecisionFromStrategy();
            }
        }

        private bool Process(Message<DestroyPieceResponse> message)
        {
            if (agentState != AgentState.InGame)
            {
                logger.Warn("Process destroy piece response: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedMessage) return true;
            }
            piece = null;
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<DiscoverResponse> message)
        {
            if (agentState != AgentState.InGame)
            {
                logger.Warn("Process discover response: Agent not in game." + " AgentID: " + id.ToString());
                if (endIfUnexpectedMessage) return true;
            }
            DateTime now = DateTime.Now;
            for (int y = position.Y - 1; y <= position.Y + 1; y++)
            {
                for (int x = position.X - 1; x <= position.X + 1; x++)
                {
                    int taby = y - position.Y + 1;
                    int tabx = x - position.X + 1;
                    if (Common.OnBoard(new Point(x, y), boardSize))
                    {
                        board[y, x].distToPiece = message.Payload.Distances[taby, tabx];
                        board[y, x].distLearned = now;
                    }
                }
            }
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<ExchangeInformationResponse> message)
        {
            if (agentState != AgentState.InGame)
            {
                logger.Warn("Process exchange information response: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedMessage) return true;
            }
            UpdateDistances(message.Payload.Distances);
            UpdateBlueTeamGoalAreaInformation(message.Payload.BlueTeamGoalAreaInformation);
            UpdateRedTeamGoalAreaInformation(message.Payload.RedTeamGoalAreaInformation);
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<MoveResponse> message)
        {
            if (agentState != AgentState.InGame)
            {
                logger.Warn("Process move response: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedMessage) return true;
            }
            position = message.Payload.CurrentPosition;
            if (message.Payload.MadeMove)
            {
                deniedLastMove = false;
                board[position.Y, position.X].distToPiece = message.Payload.ClosestPoint;
                board[position.Y, position.X].distLearned = DateTime.Now;
                if (message.Payload.ClosestPoint == 0/* && board[position.Y, position.X].goalInfo == GoalInformation.NoInformation*/)
                {
                    logger.Info("Process move response: agent pick up piece." + " AgentID: " + id.ToString());
                    return PickUp();
                }
            }
            else
            {
                deniedLastMove = true;
                logger.Info("Process move response: agent did not move." + " AgentID: " + id.ToString());
                var deniedField = Common.GetFieldInDirection(position, lastDirection);
                if (Common.OnBoard(deniedField, boardSize)) board[deniedField.Y, deniedField.X].deniedMove = DateTime.Now;
            }
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<PickUpPieceResponse> message)
        {
            if (agentState != AgentState.InGame)
            {
                logger.Warn("Process pick up piece response: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedMessage) return true;
            }
            if (board[position.Y, position.X].distToPiece == 0)
            {
                logger.Info("Process pick up piece response: Agent picked up piece" + " AgentID: " + id.ToString());
                piece = new Piece();
            }
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<PutDownPieceResponse> message)
        {
            if (agentState != AgentState.InGame)
            {
                logger.Warn("Process put down piece response: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedMessage) return true;
            }
            piece = null;
            board[position.Y, position.X].distToPiece = 0;
            board[position.Y, position.X].distLearned = DateTime.Now;
            //TODO: check if goal
            board[position.Y, position.X].goalInfo = GoalInformation.Goal;
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<ExchangeInformationPayload> message)
        {
            if (agentState != AgentState.InGame && endIfUnexpectedMessage) return true;
            if (message.Payload.Leader)
            {
                logger.Info("Process exchange information payload: Agent give info to leader" + " AgentID: " + id.ToString());
                return GiveInfo(message.Payload.AskingAgentId);
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
                bool wasWaiting = agentState == AgentState.WaitingForJoin;
                agentState = AgentState.WaitingForStart;
                id = message.Payload.AgentId;
                return wasWaiting ? false : MakeDecisionFromStrategy();
            }
            else
            {
                logger.Info("Process join response: Join request not accepted" + " AgentID: " + id.ToString());
                return true;
            }
        }

        private bool Process(Message<StartGamePayload> message)
        {
            if (agentState != AgentState.WaitingForStart && endIfUnexpectedMessage) return true;
            Initialize(message.Payload.LeaderId, message.Payload.TeamId, message.Payload.BoardSize, message.Payload.GoalAreaHeight, message.Payload.Position, message.Payload.AlliesIds, message.Payload.Penalties, message.Payload.ShamPieceProbability);
            if (id != message.Payload.AgentId)
            {
                logger.Warn("Process start game payload: payload.agnetId not equal agentId" + " AgentID: " + id.ToString());
            }
            agentState = AgentState.InGame;
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<EndGamePayload> message)
        {
            logger.Info("Process End Game: end game" + " AgentID: " + id.ToString());
            return true;
        }

        private bool Process(Message<IgnoredDelayError> message)
        {
            logger.Error("IgnoredDelay error" + " AgentID: " + id.ToString());
            if (runningAsync)
            {
                var time = message.Payload.RemainingDelay;
                if (time.CompareTo(TimeSpan.Zero) > 0) Thread.Sleep(time);
                return MakeDecisionFromStrategy();
            }
            else
            {
                var time = message.Payload.RemainingDelay;
                if (waitUntil != DateTime.MaxValue) waitUntil = DateTime.Now + time;
                if (time.CompareTo(TimeSpan.Zero) > 0) return false;
                else return MakeDecisionFromStrategy();
            }
        }

        private bool Process(Message<MoveError> message)
        {
            logger.Error("Move error" + " AgentID: " + id.ToString());
            deniedLastMove = true;
            position = message.Payload.Position;
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<PickUpPieceError> message)
        {
            logger.Error("Pick up piece error" + " AgentID: " + id.ToString());
            board[position.Y, position.X].distLearned = DateTime.Now;
            board[position.Y, position.X].distToPiece = int.MaxValue;
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<PutDownPieceError> message)
        {
            logger.Error("Put down piece error" + " AgentID: " + id.ToString());
            if (message.Payload.ErrorSubtype == PutDownPieceErrorSubtype.AgentNotHolding) piece = null;
            //TODO: check if goal
            board[position.Y, position.X].goalInfo = GoalInformation.NoGoal;
            return MakeDecisionFromStrategy();
        }

        private bool Process(Message<UndefinedError> message)
        {
            logger.Error("Undefined error" + " AgentID: " + id.ToString());
            return MakeDecisionFromStrategy();
        }
    }
}
