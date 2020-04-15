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
        public bool endIfUnexpectedMessage = false;

        public bool endIfUnexpectedAction = false;

        private const int maxSkipCount = int.MaxValue;

        private int skipCount;

        public int id;

        private int lastAskedTeammate;

        public Direction LastDirection { get; private set; }

        private ISender sender;

        private IStrategy strategy;

        private List<BaseMessage> injectedMessages;

        public double RemainingPenalty { get; set; }

        public bool WantsToBeLeader { get; private set; }

        public List<int> WaitingPlayers { get; private set; }

        public Piece Piece { get; set; }

        public AgentState AgentState { get; set; }

        private static NLog.Logger logger;

        public bool DeniedLastMove { get; set; }

        public Action<Agent, BaseMessage> MockMessageSendFunction { get; set; }

        public ProcessMessages ProcessMessages { get; set; }

        public BoardLogicComponent BoardLogicComponent { get; set; }

        public StartGameComponent StartGameComponent { get; private set; }

        public AgentConfiguration AgentConfiguration { get; set; }

        public Agent(TeamId teamId, bool wantsToBeLeader = false)
        {
            StartGameComponent = new StartGameComponent(this, teamId);
            this.WantsToBeLeader = wantsToBeLeader;
            Piece = null;
            lastAskedTeammate = 0;
            DeniedLastMove = false;
            RemainingPenalty = 0.0;
            skipCount = 0;
            WaitingPlayers = new List<int>();
            strategy = new SimpleStrategy();
            injectedMessages = new List<BaseMessage>();
            AgentState = AgentState.Created;
            logger = NLog.LogManager.GetCurrentClassLogger();
            ProcessMessages = new ProcessMessages(this);
        }

        private void SetPenalty(ActionType action)
        {
            var ret = StartGameComponent.penalties.TryGetValue(action, out TimeSpan span);
            if (ret) RemainingPenalty += span.TotalSeconds;
        }

        public void SetDoNothingStrategy()
        {
            this.strategy = new DoNothingStrategy();
        }

        public ActionResult Update(double dt)
        {
            if (AgentState == AgentState.Finished) return ActionResult.Finish;
            RemainingPenalty = Math.Max(0.0, RemainingPenalty - dt);
            if (RemainingPenalty > 0.0) return ActionResult.Continue;
            switch (AgentState)
            {
                case AgentState.Created:
                    SendMessage(MessageFactory.GetMessage(new JoinRequest(StartGameComponent.team, WantsToBeLeader)));
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
                    BaseMessage message = GetMessage();
                    if (message == null && skipCount < maxSkipCount)
                    {
                        skipCount++;
                        return ActionResult.Continue;
                    }
                    skipCount = 0;
                    ActionResult ret = message == null ? MakeDecisionFromStrategy() : AcceptMessage(message);
                    if (ret == ActionResult.Finish)
                    {
                        AgentState = AgentState.Finished;
                        return ActionResult.Finish;
                    }
                    return ActionResult.Continue;
                default:
                    logger.Error("Agent in unknown state: " + AgentState.ToString() + " AgentID: " + id.ToString());
                    return ActionResult.Finish;
            }
        }

        public ActionResult Move(Direction direction)
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Move: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return ActionResult.Finish;
            }
            LastDirection = direction;
            SetPenalty(ActionType.Move);
            SendMessage(MessageFactory.GetMessage(new MoveRequest(direction)));
            logger.Info("Move: Agent sent move request in direction " + direction.ToString() + " AgentID: " + id.ToString());
            return ActionResult.Continue;
        }

        public ActionResult PickUp()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Pick up: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return ActionResult.Finish;
            }
            SendMessage(MessageFactory.GetMessage(new PickUpPieceRequest()));
            logger.Info("Pick up: Agent sent pick up piece request." + " AgentID: " + id.ToString());
            return ActionResult.Continue;
        }

        public ActionResult Put()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Put: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return ActionResult.Finish;
            }
            SetPenalty(ActionType.PutPiece);
            SendMessage(MessageFactory.GetMessage(new PutDownPieceRequest()));
            logger.Info("Put: Agent sent put down piece request." + " AgentID: " + id.ToString());
            return ActionResult.Continue;
        }

        public ActionResult BegForInfo()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Beg for info: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return ActionResult.Finish;
                return MakeDecisionFromStrategy();
            }
            if (StartGameComponent.teamMates.Length == 0)
            {
                logger.Warn("Beg for info: Agent does not know his teammates" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return ActionResult.Finish;
                return MakeDecisionFromStrategy();
            }
            lastAskedTeammate++;
            lastAskedTeammate %= StartGameComponent.teamMates.Length;
            SetPenalty(ActionType.InformationExchange);
            SendMessage(MessageFactory.GetMessage(new ExchangeInformationRequest(StartGameComponent.teamMates[lastAskedTeammate])));
            logger.Info("Beg for info: Agent sent exchange information request." + " AgentID: " + id.ToString());
            return ActionResult.Continue;
        }

        public ActionResult GiveInfo(int respondToId = -1)
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Give info: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return ActionResult.Finish;
            }
            if (respondToId == -1 && WaitingPlayers.Count > 0)
            {
                respondToId = WaitingPlayers[0];
                WaitingPlayers.RemoveAt(0);
                logger.Info("Give info: ResponfdId is -1. Respond to first waiting player." + " AgentID: " + id.ToString());
            }
            if (respondToId == -1)
            {
                logger.Warn("Give info: Respond to id -1 while give info" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return ActionResult.Finish;
            }
            else if (respondToId == -1) return MakeDecisionFromStrategy();
            SetPenalty(ActionType.InformationExchange);
            SendMessage(MessageFactory.GetMessage(new ExchangeInformationResponse(respondToId, BoardLogicComponent.GetDistances(), BoardLogicComponent.GetRedTeamGoalAreaInformation(), BoardLogicComponent.GetBlueTeamGoalAreaInformation())));
            logger.Info("Give info: Agent sent exchange information response to adentId: " + respondToId.ToString() + " AgentID: " + id.ToString());
            return ActionResult.Continue;
        }

        public ActionResult CheckPiece()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Check piece: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return ActionResult.Finish;
            }
            SetPenalty(ActionType.CheckForSham);
            SendMessage(MessageFactory.GetMessage(new CheckShamRequest()));
            logger.Info("Check piece: Agent sent check scham request." + " AgentID: " + id.ToString());
            return ActionResult.Continue;
        }

        public ActionResult Discover()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Discover: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return ActionResult.Finish;
            }
            SetPenalty(ActionType.Discovery);
            SendMessage(MessageFactory.GetMessage(new DiscoverRequest()));
            logger.Info("Discover: Agent sent discover request." + " AgentID: " + id.ToString());
            return ActionResult.Continue;
        }

        public ActionResult DestroyPiece()
        {
            if (AgentState != AgentState.InGame)
            {
                logger.Warn("Destroy Piece: Agent not in game" + " AgentID: " + id.ToString());
                if (endIfUnexpectedAction) return ActionResult.Finish;
            }
            SetPenalty(ActionType.DestroyPiece);
            SendMessage(MessageFactory.GetMessage(new DestroyPieceRequest()));
            logger.Info("Destroy Piece: Agent sent destroy piece request." + " AgentID: " + id.ToString());
            return ActionResult.Continue;
        }

        public ActionResult MakeDecisionFromStrategy()
        {
            return strategy.MakeDecision(this);
        }

        private BaseMessage GetMessage()
        {
            if (injectedMessages.Count == 0)
            {
                return null;
            }
            var message = injectedMessages.FirstOrDefault(m => m.MessageId == MessageId.EndGameMessage);
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

        public void InjectMessage(BaseMessage message)
        {
            injectedMessages.Add(message);
        }

        public void SendMessage(BaseMessage message)
        {
            MockMessageSendFunction?.Invoke(this, message);
        }

        public ActionResult AcceptMessage(BaseMessage message)
        {
            dynamic dynamicMessage = message;
            return ProcessMessages.Process(dynamicMessage);
        }

    }
}
