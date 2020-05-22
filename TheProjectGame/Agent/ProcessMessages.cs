using Agent.Enums;
using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.Errors;
using Messaging.Contracts.GameMaster;
using Messaging.Enumerators;
using Messaging.Serialization.Extensions;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Agent
{
    public class ProcessMessages
    {
        private Agent agent;
        private static Logger logger = LogManager.GetCurrentClassLogger(); 

        List<MessageId> ingameMessageTypes = new List<MessageId> { 
            MessageId.CheckShamResponse, MessageId.DestroyPieceResponse, MessageId.DiscoverResponse,
            MessageId.ExchangeInformationRequestForward, MessageId.ExchangeInformationResponseForward,
            MessageId.MoveResponse, MessageId.PickUpPieceResponse, MessageId.PutDownPieceResponse,
            MessageId.IgnoredDelayError, MessageId.MoveError, MessageId.PickUpPieceError,
            MessageId.PutDownPieceError, MessageId.EndGameMessage
        };

        public ProcessMessages(Agent agent)
        {
            this.agent = agent;
        }

        public List<MessageId> GetIngameMessageTypes()
        {
            return ingameMessageTypes;
        }

        public ActionResult Process(Message<CheckShamResponse> message)
        {
            logger.Debug("[Agent {id}] Received check sham response: {response}", agent.Id, message.Payload.Sham);

            if (message.Payload.Sham)
            {
                logger.Debug("[Agent {id}] Forced piece destroy after sham check", agent.Id);
                agent.Piece = null;
            }
            else
            {
                agent.Piece.isDiscovered = true;
            }
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<DestroyPieceResponse> message)
        {
            logger.Debug("[Agent {id}] Received destroy piece response", agent.Id);

            agent.Piece = null;
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<DiscoverResponse> message)
        {
            logger.Debug("[Agent {id}] Received discovery response", agent.Id);

            agent.AgentInformationsComponent.Discovered = true;
            DateTime now = DateTime.Now;
            for (int y = agent.BoardLogicComponent.Position.Y - 1; y <= agent.BoardLogicComponent.Position.Y + 1; y++)
            {
                for (int x = agent.BoardLogicComponent.Position.X - 1; x <= agent.BoardLogicComponent.Position.X + 1; x++)
                {
                    int taby = y - agent.BoardLogicComponent.Position.Y + 1;
                    int tabx = x - agent.BoardLogicComponent.Position.X + 1;
                    if (Common.OnBoard(new Point(x, y), agent.BoardLogicComponent.BoardSize))
                    {
                        agent.BoardLogicComponent.Board[y, x].distToPiece = message.Payload.Distances[taby, tabx];
                        agent.BoardLogicComponent.Board[y, x].distLearned = now;
                    }
                }
            }
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<ExchangeInformationResponseForward> message)
        {
            logger.Debug("[Agent {id}] Received information exchange response", agent.Id);

            agent.AgentInformationsComponent.IsWaiting = false;

            //agent.BoardLogicComponent.UpdateDistances(message.Payload.Distances);
            agent.BoardLogicComponent.UpdateBlueTeamGoalAreaInformation(message.Payload.BlueTeamGoalAreaInformation.ToTwoDimensionalArray(agent.BoardLogicComponent.GoalAreaSize, agent.BoardLogicComponent.BoardSize.X));
            agent.BoardLogicComponent.UpdateRedTeamGoalAreaInformation(message.Payload.RedTeamGoalAreaInformation.ToTwoDimensionalArray(agent.BoardLogicComponent.GoalAreaSize, agent.BoardLogicComponent.BoardSize.X));

            if (Common.KnowsAll(agent))
            {
                agent.AgentInformationsComponent.AssignToWholeTaskArea();
            }

            var newMessage = agent.GetMessage();
            return newMessage == null ? agent.MakeDecisionFromStrategy() : agent.AcceptMessage(newMessage);
        }

        public ActionResult Process(Message<MoveResponse> message)
        {
            logger.Debug("[Agent {id}] Move response: moved: {moved}, piece: {piece}", agent.Id, message.Payload.MadeMove, message.Payload.ClosestPiece);

            agent.BoardLogicComponent.Position = message.Payload.CurrentPosition;
            if (message.Payload.MadeMove)
            {
                agent.AgentInformationsComponent.DeniedMove(false);
                agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece = message.Payload.ClosestPiece;
                agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distLearned = DateTime.Now;
                if (message.Payload.ClosestPiece == 0 && agent.Piece == null)
                    return agent.MakeForcedDecision(SpecificActionType.PickUp);
            }
            else
            {
                agent.AgentInformationsComponent.DeniedMove(true);
                var deniedField = Common.GetFieldInDirection(agent.BoardLogicComponent.Position, agent.AgentInformationsComponent.LastDirection);
                if (Common.OnBoard(deniedField, agent.BoardLogicComponent.BoardSize)) 
                    agent.BoardLogicComponent.Board[deniedField.Y, deniedField.X].deniedMove = DateTime.Now;
            }

            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<PickUpPieceResponse> message)
        {
            logger.Debug("[Agent {id}] Received pick up piece response", agent.Id);
            if (agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece == 0)
            {
                logger.Debug("[Agent {id}] Picked up piece", agent.Id);
                agent.Piece = new Piece();
            }

            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<PutDownPieceResponse> message)
        {
            logger.Debug("[Agent {id}] Received put down piece reponse: {response}", agent.Id, message.Payload.Result);
            agent.Piece = null;
            switch (message.Payload.Result)
            {
                case PutDownPieceResult.NormalOnGoalField:
                    agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].goalInfo = GoalInformation.Goal;
                    break;
                case PutDownPieceResult.NormalOnNonGoalField:
                    agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].goalInfo = GoalInformation.NoGoal;
                    break;
                case PutDownPieceResult.ShamOnGoalArea:
                    break;
                case PutDownPieceResult.TaskField:
                    agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].goalInfo = GoalInformation.NoGoal;
                    agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece = 0;
                    agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distLearned = DateTime.Now;
                    break;
            }
            if (Common.KnowsAll(agent))
            {
                agent.AgentInformationsComponent.AssignToWholeTaskArea();
            }
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<ExchangeInformationRequestForward> message)
        {
            logger.Debug("[Agent {id}] Received information request from {id2}", agent.Id, message.Payload.AskingAgentId);

            if (message.Payload.Leader)
            {
                logger.Debug("[Agent {id}] Forcing immediate response - asking agent is team leader", agent.Id);
                return agent.MakeForcedDecision(SpecificActionType.GiveInfo, message.Payload.AskingAgentId);
            }
            if (message.Payload.TeamId != agent.StartGameComponent.Team)
            {
                logger.Debug("[Agent {id}] Request from opposite team", agent.Id);
            }
            else
            {
                agent.WaitingPlayers.Add(message.Payload.AskingAgentId);
            }
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<JoinResponse> message)
        {
            if (agent.AgentState != AgentState.WaitingForJoin)
            {
                logger.Warn("[Agent {id}] Received join response, but not in waiting for join state", agent.Id);
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }

            if (message.Payload.Accepted)
            {
                logger.Warn("[Agent {id}] Received join response, accepted", agent.Id);
                bool wasWaiting = agent.AgentState == AgentState.WaitingForJoin;
                agent.AgentState = AgentState.WaitingForStart;
                agent.Id = message.Payload.AgentId;
                return wasWaiting ? ActionResult.Continue : agent.MakeDecisionFromStrategy();
            }
            else
            {
                logger.Info("[Agent {id}] Received join response, rejected", agent.Id);
                return ActionResult.Finish;
            }
        }

        public ActionResult Process(Message<StartGamePayload> message)
        {
            if (agent.AgentState != AgentState.WaitingForStart)
            {
                logger.Warn("[Agent {id}] Received start game payload, but not waiting for start", agent.Id);
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }

            agent.StartGameComponent.Initialize(message.Payload);

            if (agent.Id != message.Payload.AgentId)
                logger.Warn("[Agent {id}] Received start game payload, mismatch in ids: received {id2}", agent.Id, message.Payload.AgentId);

            agent.AgentState = AgentState.InGame;
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<EndGamePayload> message)
        {
            logger.Info("[Agent {id}] Received end game payload", agent.Id);
            return ActionResult.Finish;
        }

        public ActionResult Process(Message<IgnoredDelayError> message)
        {
            logger.Debug("[Agent {id}] Received ignored delay {time}ms", agent.Id, message.Payload.RemainingDelay.TotalMilliseconds);
            agent.AgentInformationsComponent.DeniedLastRequest = true;
            var time = message.Payload.RemainingDelay;
            agent.SetPenalty(time.TotalSeconds, false);
            return ActionResult.Continue;
        }

        public ActionResult Process(Message<MoveError> message)
        {
            logger.Debug("[Agent {id}] Received move error message", agent.Id);
            agent.AgentInformationsComponent.DeniedMove(true);
            agent.BoardLogicComponent.Position = message.Payload.Position;
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<PickUpPieceError> message)
        {
            logger.Debug("[Agent {id}] Received pick up piece error", agent.Id);
            if (message.Payload.ErrorSubtype == PickUpPieceErrorSubtype.NothingThere)
            {
                agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distLearned = DateTime.Now;
                agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece = int.MaxValue;
            }
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<PutDownPieceError> message)
        {
            logger.Debug("[Agent {id}] Received put down piece error", agent.Id);
            if (message.Payload.ErrorSubtype == PutDownPieceErrorSubtype.AgentNotHolding) agent.Piece = null;
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<UndefinedError> message)
        {
            logger.Warn("[Agent {id}] Received undefined error", agent.Id);

            agent.BoardLogicComponent.Position = message.Payload.Position;
            BaseMessage messageFromLeader = agent.GetMessageFromLeader();
            if (messageFromLeader == null)
            {
                return agent.MakeDecisionFromStrategy();
            }
            else
            {
                var result = agent.AcceptMessage(messageFromLeader);
                agent.AgentInformationsComponent.DeniedLastRequest = true;
                return result;
            }
        }
    }
}


