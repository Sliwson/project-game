using Agent.Enums;
using Agent.strategies;
using Messaging.Communication;
using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.GameMaster;
using Messaging.Enumerators;
using Messaging.Implementation;
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

        private const double penaltyMultiply = 1.5;

        private const int maxSkip = 10;

        public int Id { get; set; }

        private IStrategy strategy;

        private List<BaseMessage> injectedMessages;

        public bool WantsToBeLeader { get; private set; }

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


        private static NLog.Logger logger;

        public Agent(AgentConfiguration agentConfiguration)
        {
            logger.Info("[Agent] Agent created");

            var teamId = agentConfiguration.TeamID.ToLower() == "red" ? TeamId.Red : TeamId.Blue;
            
            StartGameComponent = new StartGameComponent(this, teamId);
            AgentInformationsComponent = new AgentInformationsComponent(this);
            AgentConfiguration = agentConfiguration;
            WantsToBeLeader = agentConfiguration.WantsToBeTeamLeader;
            NetworkComponent = new ClientNetworkComponent(agentConfiguration.CsIP, agentConfiguration.CsPort);
            Piece = null;
            WaitingPlayers = new List<int>();
            strategy = new SimpleStrategy();
            injectedMessages = new List<BaseMessage>();
            AgentState = AgentState.Created;
            logger = NLog.LogManager.GetCurrentClassLogger();
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
            AgentInformationsComponent.RemainingPenalty += add * penaltyMultiply;
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
            if (AgentState == AgentState.Finished) return ActionResult.Finish;
            AgentInformationsComponent.RemainingPenalty = Math.Max(0.0, AgentInformationsComponent.RemainingPenalty - dt);
            if (AgentInformationsComponent.RemainingPenalty > 0.0) return ActionResult.Continue;
            switch (AgentState)
            {
                case AgentState.Created:
                    SendMessage(MessageFactory.GetMessage(new JoinRequest(StartGameComponent.Team, WantsToBeLeader)), false);
                    AgentState = AgentState.WaitingForJoin;
                    return ActionResult.Continue;
                case AgentState.WaitingForJoin:
                    var joinResponse = GetMessage(MessageId.JoinResponse);
                    if (joinResponse == null) return ActionResult.Continue;
                    if (AcceptMessage(joinResponse) == ActionResult.Finish)
                    {
                        AgentState = AgentState.Finished;
                        return ActionResult.Finish;
                    }
                    return ActionResult.Continue;
                case AgentState.WaitingForStart:
                    var startResponse = GetMessage(MessageId.StartGameMessage);
                    if (startResponse == null) return ActionResult.Continue;
                    if (AcceptMessage(startResponse) == ActionResult.Finish)
                    {
                        AgentState = AgentState.Finished;
                        return ActionResult.Finish;
                    }
                    return ActionResult.Continue;
                case AgentState.InGame:
                    if (AgentInformationsComponent.DeniedLastRequest) return RepeatRequest();
                    BaseMessage message = GetMessage();
                    if (message == null && AgentInformationsComponent.SkipTime < TimeSpan.FromTicks(StartGameComponent.AverageTime.Ticks * maxSkip))
                    {
                        AgentInformationsComponent.SkipTime += TimeSpan.FromSeconds(dt);
                        return ActionResult.Continue;
                    }
                    AgentInformationsComponent.SkipTime = TimeSpan.Zero;
                    ActionResult ret = message == null ? MakeDecisionFromStrategy() : AcceptMessage(message);
                    if (ret == ActionResult.Finish)
                    {
                        AgentState = AgentState.Finished;
                        return ActionResult.Finish;
                    }
                    return ActionResult.Continue;
                default:
                    logger.Error("[Agent {id}] in unknown state: {state}", Id, AgentState);
                    return ActionResult.Finish;
            }
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
            logger.Warn("[Agent {id}] Sent move request in direction {direction}", Id, direction);
            return ActionResult.Continue;
        }

        public ActionResult PickUp()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Pick up: Agent not in game" + " AgentID: " + Id.ToString());
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }
            SendMessage(MessageFactory.GetMessage(new PickUpPieceRequest()), true);
            logger.Info("Pick up: Agent sent pick up piece request." + " AgentID: " + Id.ToString());
            return ActionResult.Continue;
        }

        public ActionResult Put()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Put: Agent not in game" + " AgentID: " + Id.ToString());
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }
            SetPenalty(ActionType.PutPiece, true);
            SendMessage(MessageFactory.GetMessage(new PutDownPieceRequest()), true);
            logger.Info("Put: Agent sent put down piece request." + " AgentID: " + Id.ToString());
            return ActionResult.Continue;
        }

        public ActionResult BegForInfo()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Beg for info: Agent not in game" + " AgentID: " + Id.ToString());
                if (EndIfUnexpectedAction) return ActionResult.Finish;
                return MakeDecisionFromStrategy();
            }
            if (StartGameComponent.TeamMatesToAsk.Length == 0)
            {
                logger.Warn("Beg for info: Agent does not know his teammates" + " AgentID: " + Id.ToString());
                if (EndIfUnexpectedAction) return ActionResult.Finish;
                return MakeDecisionFromStrategy();
            }
            AgentInformationsComponent.LastAskedTeammate++;
            AgentInformationsComponent.LastAskedTeammate %= StartGameComponent.TeamMatesToAsk.Length;
            SetPenalty(ActionType.InformationExchange, true);
            SendMessage(MessageFactory.GetMessage(new ExchangeInformationRequest(StartGameComponent.TeamMatesToAsk[AgentInformationsComponent.LastAskedTeammate])), true);
            logger.Info("Beg for info: Agent sent exchange information request." + " AgentID: " + Id.ToString());
            return ActionResult.Continue;
        }

        public ActionResult GiveInfo(int respondToId = -1)
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Give info: Agent not in game" + " AgentID: " + Id.ToString());
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }
            if (respondToId == -1 && WaitingPlayers.Count > 0)
            {
                respondToId = WaitingPlayers[0];
                WaitingPlayers.RemoveAt(0);
                logger.Info("Give info: ResponfdId is -1. Respond to first waiting player." + " AgentID: " + Id.ToString());
            }
            if (respondToId == -1)
            {
                logger.Warn("Give info: Respond to id -1 while give info" + " AgentID: " + Id.ToString());
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }
            else if (respondToId == -1) return MakeDecisionFromStrategy();
            SetPenalty(ActionType.InformationExchange, false);
            SendMessage(MessageFactory.GetMessage(new ExchangeInformationResponse(respondToId,
                BoardLogicComponent.GetDistances(),
                BoardLogicComponent.GetRedTeamGoalAreaInformation(),
                BoardLogicComponent.GetBlueTeamGoalAreaInformation())),
                false);
            logger.Info("Give info: Agent sent exchange information response to adentId: " + respondToId.ToString() + " AgentID: " + Id.ToString());
            return ActionResult.Continue;
        }

        public ActionResult CheckPiece()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Check piece: Agent not in game" + " AgentID: " + Id.ToString());
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }
            SetPenalty(ActionType.CheckForSham, true);
            SendMessage(MessageFactory.GetMessage(new CheckShamRequest()), true);
            logger.Info("Check piece: Agent sent check scham request." + " AgentID: " + Id.ToString());
            return ActionResult.Continue;
        }

        public ActionResult Discover()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Discover: Agent not in game" + " AgentID: " + Id.ToString());
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }
            SetPenalty(ActionType.Discovery, true);
            SendMessage(MessageFactory.GetMessage(new DiscoverRequest()), true);
            logger.Info("Discover: Agent sent discover request." + " AgentID: " + Id.ToString());
            return ActionResult.Continue;
        }

        public ActionResult DestroyPiece()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Destroy Piece: Agent not in game" + " AgentID: " + Id.ToString());
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }
            SetPenalty(ActionType.DestroyPiece, true);
            SendMessage(MessageFactory.GetMessage(new DestroyPieceRequest()), true);
            logger.Info("Destroy Piece: Agent sent destroy piece request." + " AgentID: " + Id.ToString());
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

        public ActionResult RepeatRequest()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Repeat Action: Agent not in game" + " AgentID: " + Id.ToString());
                if (EndIfUnexpectedAction) return ActionResult.Finish;
            }
            AgentInformationsComponent.DeniedLastRequest = false;
            SetPenalty(AgentInformationsComponent.LastRequestPenalty, true);
            SendMessage(AgentInformationsComponent.LastRequest, true);
            logger.Info("Repeat Action: Agent resent previous action." + " AgentID: " + Id.ToString());
            return ActionResult.Continue;
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
            dynamic dynamicMessage = message;
            return ProcessMessages.Process(dynamicMessage);
        }
    }
}
