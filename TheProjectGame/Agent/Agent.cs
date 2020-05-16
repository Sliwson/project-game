using Agent.Enums;
using Agent.strategies;
using Messaging.Communication;
using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.GameMaster;
using Messaging.Enumerators;
using Messaging.Implementation;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Agent
{
    public class Agent : IMessageProcessor
    {
        public bool EndIfUnexpectedMessage { get; } = false;

        public bool EndIfUnexpectedAction { get; } = false;

        public bool DivideAgents { get; } = true;

        private const double penaltyMultiply = 1.0;

        private const double penaltyAdd = 0.06;

        private const int maxSkip = 10;

        public int Id { get; set; }

        private IStrategy strategy;

        private List<BaseMessage> injectedMessages;

        public List<int> WaitingPlayers { get; private set; }

        public Piece Piece { get; set; }

        public AgentState AgentState { get; set; }

        public Action<Agent, BaseMessage> MockMessageSendFunction { get; set; }

        public ProcessMessages ProcessMessages { get; set; }

        public BoardLogicComponent BoardLogicComponent { get; set; }

        public StartGameComponent StartGameComponent { get; private set; }

        public AgentConfiguration AgentConfiguration { get; set; }

        public AgentInformationsComponent AgentInformationsComponent { get; set; }

        public INetworkComponent NetworkComponent { get; private set; }


        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Agent(AgentConfiguration agentConfiguration)
        {
            logger.Info("[Agent] Agent created");

            var teamId = agentConfiguration.TeamID.ToLower() == "red" ? TeamId.Red : TeamId.Blue;
            
            StartGameComponent = new StartGameComponent(this, teamId);
            AgentInformationsComponent = new AgentInformationsComponent(this);
            AgentConfiguration = agentConfiguration;
            NetworkComponent = new ClientNetworkComponent(agentConfiguration.CsIP, agentConfiguration.CsPort);
            Piece = null;
            WaitingPlayers = new List<int>();
            strategy = new SimpleStrategy();
            injectedMessages = new List<BaseMessage>();
            AgentState = AgentState.Created;
            ProcessMessages = new ProcessMessages(this);
        }

        public void ConnectToCommunicationServer()
        {
            if (!NetworkComponent.Connect(ClientType.Agent))
                throw new ApplicationException("Unable to connect to CS", NetworkComponent.Exception);
        }

        public void OnDestroy()
        {
            NetworkComponent?.Disconnect();
        }

        public void SetPenalty(double add, bool shouldRepeat)
        {
            if (add <= 0.0) return;
            AgentInformationsComponent.RemainingPenalty += add * penaltyMultiply + penaltyAdd;
            if (shouldRepeat) AgentInformationsComponent.LastRequestPenalty = add;
        }

        private void SetPenalty(ActionType action, bool shouldRepeat)
        {
            var ret = StartGameComponent.Penalties.TryGetValue(action, out TimeSpan span);
            if (ret) SetPenalty(span.TotalSeconds, shouldRepeat);
        }

        public void SetDoNothingStrategy()
        {
            strategy = new DoNothingStrategy();
        }

        public ActionResult Update(double dt)
        {
            if (NetworkComponent.Exception != null)
                throw NetworkComponent.Exception;
            
            injectedMessages.AddRange(NetworkComponent.GetIncomingMessages());

            AgentInformationsComponent.RemainingPenalty = Math.Max(0.0, AgentInformationsComponent.RemainingPenalty - dt);
            if (AgentInformationsComponent.RemainingPenalty > 0.0) 
                return ActionResult.Continue;

            var result = ActionResult.Continue;
            switch (AgentState)
            {
                case AgentState.Created:
                    result = UpdateStateCreated();
                    break;
                case AgentState.WaitingForJoin:
                    result = UpdateStateWaitingForJoin();
                    break;
                case AgentState.WaitingForStart:
                    result = UpdateStateWaitingForStart();
                    break;
                case AgentState.InGame:
                    result = UpdateStateInGame(dt);
                    break;
                case AgentState.Finished:
                    return ActionResult.Finish;
                default:
                    logger.Error("[Agent {id}] in unknown state: {state}", Id, AgentState);
                    return ActionResult.Finish;
            }

            if (result == ActionResult.Finish)
                AgentState = AgentState.Finished;

            return result;
        }

        private ActionResult UpdateStateCreated()
        {
            SendMessage(MessageFactory.GetMessage(new JoinRequest(StartGameComponent.Team)), false);
            AgentState = AgentState.WaitingForJoin;
            return ActionResult.Continue;
        }

        private ActionResult UpdateStateWaitingForJoin()
        { 
            var joinResponse = GetMessage(MessageId.JoinResponse);
            if (joinResponse == null) 
                return ActionResult.Continue;

            return AcceptMessage(joinResponse);
        }

        private ActionResult UpdateStateWaitingForStart()
        {
            var startResponse = GetMessage(MessageId.StartGameMessage);
            if (startResponse == null)
                return ActionResult.Continue;

            return AcceptMessage(startResponse);
        }

        private ActionResult UpdateStateInGame(double dt)
        {
            if (AgentInformationsComponent.DeniedLastRequest) 
                return RepeatRequest();
                    
            var message = GetMessage();

            if (message == null && AgentInformationsComponent.SkipTime < TimeSpan.FromTicks(StartGameComponent.AverageTime.Ticks * maxSkip))
            {
                AgentInformationsComponent.SkipTime += TimeSpan.FromSeconds(dt);
                return ActionResult.Continue;
            }

            AgentInformationsComponent.SkipTime = TimeSpan.Zero;
            return message == null ? MakeDecisionFromStrategy() : AcceptMessage(message);
        }

        public ActionResult Move(Direction direction)
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("[Agent {id}] Requested move, but not in game", Id);
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }

            AgentInformationsComponent.LastDirection = direction;
            SetPenalty(ActionType.Move, true);
            SendMessage(MessageFactory.GetMessage(new MoveRequest(direction)), true);
            logger.Debug("[Agent {id}] Sent move request in direction {direction}", Id, direction);
            return ActionResult.Continue;
        }

        public ActionResult PickUp()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("[Agent {id}] Requested pick up, but not in game", Id);
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }

            SendMessage(MessageFactory.GetMessage(new PickUpPieceRequest()), true);
            logger.Debug("[Agent {id}] Sent pick up request", Id);
            return ActionResult.Continue;
        }

        public ActionResult Put()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("[Agent {id}] Requested put, but not in game", Id);
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }

            SetPenalty(ActionType.PutPiece, true);
            SendMessage(MessageFactory.GetMessage(new PutDownPieceRequest()), true);
            logger.Debug("[Agent {id}] Sent put down piece request", Id);
            return ActionResult.Continue;
        }

        public ActionResult BegForInfo()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("[Agent {id}] Begged for info, but not in game", Id);
                if (EndIfUnexpectedAction) return ActionResult.Finish;
                return MakeDecisionFromStrategy();
            }

            if (StartGameComponent.TeamMatesToAsk.Length == 0)
            {
                logger.Warn("[Agent {id}] Wanted information exchange but do not know teammates", Id);
                if (EndIfUnexpectedAction) return ActionResult.Finish;
                return MakeDecisionFromStrategy();
            }

            AgentInformationsComponent.LastAskedTeammate++;
            AgentInformationsComponent.LastAskedTeammate %= StartGameComponent.TeamMatesToAsk.Length;
            SetPenalty(ActionType.InformationExchange, true);
            SendMessage(MessageFactory.GetMessage(new ExchangeInformationRequest(StartGameComponent.TeamMatesToAsk[AgentInformationsComponent.LastAskedTeammate])), true);
            logger.Debug("[Agent {id}] Sent exchange information request", Id);
            return ActionResult.Continue;
        }

        public ActionResult GiveInfo(int respondToId = -1)
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("[Agent {id}] Requested give info, but not in game", Id);
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }

            if (respondToId == -1 && WaitingPlayers.Count > 0)
            {
                respondToId = WaitingPlayers[0];
                WaitingPlayers.RemoveAt(0);
                logger.Debug("[Agent {id}] Sent exchange information response to first waiting player ({id2})", Id, respondToId);
            }

            if (respondToId == -1)
            {
                logger.Warn("[Agent {id}] Requested give info, but has no target", Id);
                if (EndIfUnexpectedAction)
                    return ActionResult.Finish;
                else
                    return MakeDecisionFromStrategy();
            }

            SetPenalty(ActionType.InformationExchange, false);
            SendMessage(MessageFactory.GetMessage(new ExchangeInformationResponse(respondToId,
                BoardLogicComponent.GetDistances(),
                BoardLogicComponent.GetRedTeamGoalAreaInformation(),
                BoardLogicComponent.GetBlueTeamGoalAreaInformation())),
                false);

            logger.Debug("[Agent {id}] Sent exchange information response to {id2} ", Id, respondToId);
            return ActionResult.Continue;
        }

        public ActionResult CheckPiece()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("[Agent {id}] Requested check piece, but not in game", Id);
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }

            SetPenalty(ActionType.CheckForSham, true);
            SendMessage(MessageFactory.GetMessage(new CheckShamRequest()), true);
            logger.Debug("[Agent {id}] Sent check piece request", Id);
            return ActionResult.Continue;
        }

        public ActionResult Discover()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("[Agent {id}] Requested discover, but not in game", Id);
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }

            SetPenalty(ActionType.Discovery, true);
            SendMessage(MessageFactory.GetMessage(new DiscoverRequest()), true);
            logger.Debug("[Agent {id}] Sent discover request", Id);
            return ActionResult.Continue;
        }

        public ActionResult DestroyPiece()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("[Agent {id}] Requested destroy piece, but not in game", Id);
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }
            SetPenalty(ActionType.DestroyPiece, true);
            SendMessage(MessageFactory.GetMessage(new DestroyPieceRequest()), true);
            logger.Debug("[Agent {id}] Sent destroy piece request", Id);
            return ActionResult.Continue;
        }
        public ActionResult RepeatRequest()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("[Agent {id}] Requested request repeat, but not in game", Id);
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }

            AgentInformationsComponent.DeniedLastRequest = false;
            SetPenalty(AgentInformationsComponent.LastRequestPenalty, true);
            SendMessage(AgentInformationsComponent.LastRequest, true);
            logger.Debug("[Agent {id}] Resent previous action", Id);
            return ActionResult.Continue;
        }

        public ActionResult MakeDecisionFromStrategy()
        {
            return strategy.MakeDecision(this);
        }

        public ActionResult MakeForcedDecision(SpecificActionType action, int argument = -1)
        {
            return strategy.MakeForcedDecision(this, action, argument);
        }

        public BaseMessage GetMessage()
        {
            if (injectedMessages.Count == 0)
                return null;

            var message = injectedMessages.FirstOrDefault(m => m.MessageId == MessageId.EndGameMessage);
            if (message == null) message = injectedMessages[0];
            injectedMessages.Remove(message);
            return message;
        }

        private BaseMessage GetMessage(MessageId messageId)
        {
            var message = injectedMessages.FirstOrDefault(m => m.MessageId == messageId);
            if (message != null) injectedMessages.Remove(message);
            return message;
        }

        public BaseMessage GetMessageFromLeader()
        {
            var message = injectedMessages.FirstOrDefault(m =>
            {
                if (m.MessageId != MessageId.ExchangeInformationRequestForward) return false;
                var exchangeMessage = m as Message<ExchangeInformationRequestForward>;
                if (exchangeMessage == null) return false;
                return exchangeMessage.Payload.Leader;
            });

            if (message != null) injectedMessages.Remove(message);
                return message;
        }

        public void InjectMessage(BaseMessage message)
        {
            injectedMessages.Add(message);
        }

        public void SendMessage(BaseMessage message, bool shouldRepeat)
        {
            if (shouldRepeat)
                AgentInformationsComponent.LastRequest = message;

            try
            {
                NetworkComponent.SendMessage(message);
            }
            catch (CommunicationErrorException e)
            {
                logger.Error("[Agent {id}] {message}", Id, e.Message);
                if (e.Type == CommunicationExceptionType.InvalidSocket)
                    throw;
            }
        }

        public ActionResult AcceptMessage(BaseMessage message)
        {
            AgentInformationsComponent.Discovered = false;

            var ingameTypes = ProcessMessages.GetIngameMessageTypes();
            if (ingameTypes.Contains(message.MessageId)  &&AgentState != AgentState.InGame)
            {
                logger.Warn("[Agent {id}] Received message of type {type}, but not in game", Id, message.MessageId);
                return EndIfUnexpectedAction ? ActionResult.Finish : ActionResult.Continue;
            }

            dynamic dynamicMessage = message;
            return ProcessMessages.Process(dynamicMessage);
        }
    }
}
