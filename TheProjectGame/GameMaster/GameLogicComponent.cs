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

namespace GameMaster
{
    public class GameLogicComponent : IMessageProcessor
    {
        private GameMaster gameMaster;
        
        public GameLogicComponent(GameMaster gameMaster)
        {
            this.gameMaster = gameMaster;
        }

        public BaseMessage ProcessMessage(BaseMessage message)
        {
            var agent = gameMaster.GetAgent(message.AgentId);

            if (agent == null && message.MessageId != MessageId.JoinRequest)
                return MessageFactory.GetMessage(new UndefinedError(new Point(0, 0), false), message.AgentId);

            if (!agent.CanPerformAction())
                return MessageFactory.GetMessage(new IgnoredDelayError(DateTime.Now.AddSeconds(agent.Timeout)), agent.Id);

            if (agent.HaveToExchange() && message.MessageId != MessageId.ExchangeInformationMessage)
                return MessageFactory.GetMessage(new UndefinedError(agent.Position, false), agent.Id);

            if (agent != null)
            {
                var timeout = gameMaster.Configuration.GetTimeouts();
                agent.AddTimeout(timeout[message.MessageId.ToActionType()].TotalSeconds);
            }

            dynamic dynamicMessage = message;
            return Process(dynamicMessage, agent);
        }

        private BaseMessage Process(Message<CheckShamRequest> message, Agent agent)
        {
            //TODO: check error type
            if (agent.Piece == null)
                return MessageFactory.GetMessage(new UndefinedError(agent.Position, false), agent.Id);

            return MessageFactory.GetMessage(new CheckShamResponse(agent.Piece.IsSham), agent.Id);
        }

        private BaseMessage Process(Message<DestroyPieceRequest> message, Agent agent)
        {
            if (agent.Piece == null)
                return MessageFactory.GetMessage(new UndefinedError(agent.Position, false), agent.Id);

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
                return MessageFactory.GetMessage(new UndefinedError(agent.Position, agent.Piece != null), agent.Id);

            targetAgent.InformationExchangeRequested(agent.IsTeamLeader);
            return MessageFactory.GetMessage(new ExchangeInformationPayload(agent.Id, agent.IsTeamLeader, agent.Team), targetAgent.Id);
        }

        private BaseMessage Process(Message<ExchangeInformationResponse> message, Agent agent)
        {
            if (!agent.CanExchange())
                return MessageFactory.GetMessage(new UndefinedError(agent.Position, agent.Piece != null), agent.Id);

            agent.ClearExchangeState();
            return MessageFactory.GetMessage(message.Payload, message.Payload.RespondToId);
        }

        private BaseMessage Process(Message<JoinRequest> message, Agent agent)
        {
            // do not accept new agents during the game
            return MessageFactory.GetMessage(new JoinResponse(false, -1), agent.Id);
        }
        
        private BaseMessage Process(Message<MoveRequest> message, Agent agent)
        {
            var board = gameMaster.BoardLogic;
            bool canMove = board.CanMove(agent, message.Payload.Direction);
            if (canMove)
                board.MoveAgent(agent, message.Payload.Direction);

            return MessageFactory.GetMessage(new MoveResponse(canMove, agent.Position, board.CalculateDistanceToNearestPiece(agent.Position)), agent.Id);
        }
        
        private BaseMessage Process(Message<PickUpPieceRequest> message, Agent agent)
        {
            if (agent.Piece != null)
                return MessageFactory.GetMessage(new PickUpPieceError(PickUpPieceErrorSubtype.Other), agent.Id);

            var field = gameMaster.BoardLogic.GetField(agent.Position);
            if (field.Pieces.Count == 0)
                return MessageFactory.GetMessage(new PickUpPieceError(PickUpPieceErrorSubtype.NothingThere), agent.Id);

            agent.PickUpPiece(field.Pieces.Pop());
            return MessageFactory.GetMessage(new PickUpPieceResponse(), agent.Id);
        }

        private BaseMessage Process(Message<PutDownPieceRequest> message, Agent agent)
        {
            if (agent.Piece == null)
                return MessageFactory.GetMessage(new PutDownPieceError(PutDownPieceErrorSubtype.AgentNotHolding), agent.Id);

            var field = gameMaster.BoardLogic.GetField(agent.Position);
            if (field.State == FieldState.Empty)
            {
                field.Pieces.Push(agent.RemovePiece());
            }
            else 
            {
                //TODO: changes in specification? send information about result
                if (field.Pieces.Count == 0)
                {
                    field.Pieces.Push(agent.RemovePiece());
                    if (field.State == FieldState.Goal)
                        gameMaster.ScoreComponent.TeamScored(agent.Team);
                }
                else 
                    return MessageFactory.GetMessage(new PutDownPieceError(PutDownPieceErrorSubtype.CannotPutThere), agent.Id);
            }

            return MessageFactory.GetMessage(new PutDownPieceResponse(), agent.Id);
        }
    }
}
