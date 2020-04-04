using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.Errors;
using Messaging.Implementation;
using Messaging.Enumerators;
using Messaging.Contracts.GameMaster;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using GameMaster.Interfaces;
using System.Linq;

namespace GameMaster
{
    public class GameLogicComponent : IMessageProcessor
    {
        private GameMaster gameMaster;
        private NLog.Logger logger;
        
        public GameLogicComponent(GameMaster gameMaster)
        {
            this.gameMaster = gameMaster;
            logger = gameMaster.Logger.Get();
        }

        public List<BaseMessage> GetStartGameMessages()
        {
            //TODO: refactor
            var agents = gameMaster.Agents;
            var config = gameMaster.Configuration;
            var messages = new List<BaseMessage>();

            foreach (var a in agents)
            {
                int[] alliesIds = agents.Where(ag => ag.Team == a.Team && ag.Id != a.Id).Select(a => a.Id).ToArray();
                int[] enemiesIds = agents.Where(ag => ag.Team != a.Team).Select(a => a.Id).ToArray();
                int leaderId = agents.FirstOrDefault(ag => ag.Team == a.Team && ag.IsTeamLeader).Id;
                var payload = new StartGamePayload(
                    a.Id,
                    alliesIds,
                    leaderId,
                    enemiesIds,
                    a.Team,
                    new Point(config.BoardX, config.BoardY),
                    config.GoalAreaHeight,
                    alliesIds.Length,
                    enemiesIds.Length,
                    config.NumberOfPieces,
                    config.NumberOfGoals,
                    config.GetTimeouts(),
                    config.ShamProbability,
                    a.Position
                   );

                messages.Add(MessageFactory.GetMessage(payload, a.Id));
            }

            return messages;
        }

        public List<BaseMessage> GetPauseMessages()
        {
            var agents = gameMaster.Agents;
            var messages = new List<BaseMessage>();

            //TODO: check for pause message
            return messages;
        }

        public List<BaseMessage> GetResumeMessages()
        {
            var agents = gameMaster.Agents;
            var messages = new List<BaseMessage>();

            //TODO: check for resume message
            return messages;
        }

        public List<BaseMessage> GetEndGameMessages(TeamId winner)
        {
            var agents = gameMaster.Agents;
            var messages = new List<BaseMessage>();

            foreach (var a in agents)
                messages.Add(MessageFactory.GetMessage(new EndGamePayload(winner), a.Id));

            return messages;
        }

        public BaseMessage ProcessMessage(BaseMessage message)
        {
            logger.Info("[Logic] Received message {type} from Agent {id}", message.MessageId, message.AgentId);
            NLog.NestedDiagnosticsContext.Push("    ");
            var agent = gameMaster.GetAgent(message.AgentId);

            if (agent == null)
            {
                logger.Warn("[Logic] Cannot find agent");
                NLog.NestedDiagnosticsContext.Pop();

                if (message.MessageId != MessageId.JoinRequest)
                    return MessageFactory.GetMessage(new UndefinedError(new Point(0, 0), false), message.AgentId);
                else
                    return MessageFactory.GetMessage(new JoinResponse(false, message.AgentId), message.AgentId);
            }

            if (!agent.CanPerformAction())
            {
                logger.Info("[Logic] Agent delayed ({time})", agent.Timeout);
                NLog.NestedDiagnosticsContext.Pop();

                return MessageFactory.GetMessage(new IgnoredDelayError(TimeSpan.FromSeconds(agent.Timeout)), agent.Id);
            }

            if (agent.HaveToExchange() && message.MessageId != MessageId.ExchangeInformationMessage)
            {
                logger.Info("[Logic] Agent has to exchange information");
                NLog.NestedDiagnosticsContext.Pop();

                return MessageFactory.GetMessage(new UndefinedError(agent.Position, false), agent.Id);
            }

            if (message.MessageId != MessageId.JoinRequest && message.MessageId != MessageId.PickUpPieceRequest)
            {
                var timeout = gameMaster.Configuration.GetTimeouts();
                var time = timeout[message.MessageId.ToActionType()].TotalSeconds;
                logger.Info("[Logic] Adding timeout for request ({time}s)", time);
                agent.AddTimeout(time);
            }

            dynamic dynamicMessage = message;
            var response = Process(dynamicMessage, agent);
            NLog.NestedDiagnosticsContext.Pop();
            return response;
        }

        private BaseMessage Process(Message<CheckShamRequest> message, Agent agent)
        {
            //TODO: check error type
            if (agent.Piece == null)
            {
                logger.Info("[Logic] Check sham requested without a piece");
                return MessageFactory.GetMessage(new UndefinedError(agent.Position, false), agent.Id);
            }

            return MessageFactory.GetMessage(new CheckShamResponse(agent.Piece.IsSham), agent.Id);
        }

        private BaseMessage Process(Message<DestroyPieceRequest> message, Agent agent)
        {
            if (agent.Piece == null)
            {
                logger.Info("[Logic] DestroyPiece requested without a piece");
                return MessageFactory.GetMessage(new UndefinedError(agent.Position, false), agent.Id);
            }

            agent.RemovePiece();
            return MessageFactory.GetMessage(new DestroyPieceResponse(), agent.Id);
        }

        private BaseMessage Process(Message<DiscoverRequest> message, Agent agent)
        {
            var distances = gameMaster.BoardLogic.GetDiscoverArray(agent.Position);
            return MessageFactory.GetMessage(new DiscoverResponse(distances), agent.Id);
        }

        private BaseMessage Process(Message<ExchangeInformationRequest> message, Agent agent)
        {
            var targetAgent = gameMaster.GetAgent(message.Payload.AskedAgentId);
            if (targetAgent == null)
            {
                logger.Info("[Logic] ExchangeRequest for non-existent agent");
                return MessageFactory.GetMessage(new UndefinedError(agent.Position, agent.Piece != null), agent.Id);
            }

            targetAgent.InformationExchangeRequested(agent.IsTeamLeader);
            return MessageFactory.GetMessage(new ExchangeInformationPayload(agent.Id, agent.IsTeamLeader, agent.Team), targetAgent.Id);
        }

        private BaseMessage Process(Message<ExchangeInformationResponse> message, Agent agent)
        {
            if (!agent.CanExchange())
            {
                logger.Info("[Logic] Exchange response sent without permission");
                return MessageFactory.GetMessage(new UndefinedError(agent.Position, agent.Piece != null), agent.Id);
            }

            agent.ClearExchangeState();
            return MessageFactory.GetMessage(message.Payload, message.Payload.RespondToId);
        }

        private BaseMessage Process(Message<JoinRequest> message, Agent agent)
        {
            logger.Info("[Logic] Rejecting join response");
            return MessageFactory.GetMessage(new JoinResponse(false, agent.Id), agent.Id);
        }
        
        private BaseMessage Process(Message<MoveRequest> message, Agent agent)
        {
            var board = gameMaster.BoardLogic;
            bool canMove = board.CanMove(agent, message.Payload.Direction);
            if (canMove)
            {
                board.MoveAgent(agent, message.Payload.Direction);
            }
            else
            {
                logger.Info("[Logic] Agent can't move from {pos} in direction {dir}", agent.Position, message.Payload.Direction);
            }

            return MessageFactory.GetMessage(new MoveResponse(canMove, agent.Position, board.CalculateDistanceToNearestPiece(agent.Position)), agent.Id);
        }
        
        private BaseMessage Process(Message<PickUpPieceRequest> message, Agent agent)
        {
            if (agent.Piece != null)
            {
                logger.Info("[Logic] Agent cannot pick up a piece - it is already holding one");
                return MessageFactory.GetMessage(new PickUpPieceError(PickUpPieceErrorSubtype.Other), agent.Id);
            }

            var field = gameMaster.BoardLogic.GetField(agent.Position);
            if (field.Pieces.Count == 0)
            {
                logger.Info("[Logic] Agent cannot pick up a piece - field is empty");
                return MessageFactory.GetMessage(new PickUpPieceError(PickUpPieceErrorSubtype.NothingThere), agent.Id);
            }

            agent.PickUpPiece(field.Pieces.Pop());
            return MessageFactory.GetMessage(new PickUpPieceResponse(), agent.Id);
        }

        private BaseMessage Process(Message<PutDownPieceRequest> message, Agent agent)
        {
            if (agent.Piece == null)
            {
                logger.Info("[Logic] Agent cannot put down a piece - it is not holding one");
                return MessageFactory.GetMessage(new PutDownPieceError(PutDownPieceErrorSubtype.AgentNotHolding), agent.Id);
            }

            var field = gameMaster.BoardLogic.GetField(agent.Position);
            if (field.State == FieldState.Empty)
            {
                field.Pieces.Push(agent.RemovePiece());
            }
            else 
            {
                //TODO: changes in specification? send information about result and add tests
                //TODO: add new state to field state, do not leave pieces on  board. Hereby no log yet
                if (field.Pieces.Count == 0)
                {
                    field.Pieces.Push(agent.RemovePiece());
                    if (field.State == FieldState.Goal)
                        gameMaster.ScoreComponent.TeamScored(agent.Team);
                }
                else
                {
                    return MessageFactory.GetMessage(new PutDownPieceError(PutDownPieceErrorSubtype.CannotPutThere), agent.Id);
                }
            }

            return MessageFactory.GetMessage(new PutDownPieceResponse(), agent.Id);
        }
    }
}
