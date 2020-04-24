using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.Errors;
using Messaging.Contracts.GameMaster;
using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Agent
{
    public class ProcessMessages
    {
        private Agent agent;
        private static NLog.Logger logger; 

        public ProcessMessages(Agent agent)
        {
            this.agent = agent;
            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public ActionResult Process(Message<CheckShamResponse> message)
        {
            if (agent.AgentState != AgentState.InGame)
            {
                logger.Warn("Process check scham response: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }
            if (message.Payload.Sham)
            {
                logger.Info("Process check scham response: Agent checked sham and destroy piece." + " AgentID: " + agent.id.ToString());
                return agent.DestroyPiece();
            }
            else
            {
                logger.Info("Process check scham response: Agent checked not sham." + " AgentID: " + agent.id.ToString());
                agent.Piece.isDiscovered = true;
                return agent.MakeDecisionFromStrategy();
            }
        }

        public ActionResult Process(Message<DestroyPieceResponse> message)
        {
            if (agent.AgentState != AgentState.InGame)
            {
                logger.Warn("Process destroy piece response: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }
            agent.Piece = null;
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<DiscoverResponse> message)
        {
            if (agent.AgentState != AgentState.InGame)
            {
                logger.Warn("Process discover response: Agent not in game." + " AgentID: " + agent.id.ToString());
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }
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
            if (agent.AgentState != AgentState.InGame)
            {
                logger.Warn("Process exchange information response: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }
            //agent.BoardLogicComponent.UpdateDistances(message.Payload.Distances);
            agent.BoardLogicComponent.UpdateBlueTeamGoalAreaInformation(message.Payload.BlueTeamGoalAreaInformation);
            agent.BoardLogicComponent.UpdateRedTeamGoalAreaInformation(message.Payload.RedTeamGoalAreaInformation);
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<MoveResponse> message)
        {
            if (agent.AgentState != AgentState.InGame)
            {
                logger.Warn("Process move response: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }
            agent.BoardLogicComponent.Position = message.Payload.CurrentPosition;
            if (message.Payload.MadeMove)
            {
                agent.AgentInformationsComponent.DeniedLastMove = false;
                agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece = message.Payload.ClosestPiece;
                agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distLearned = DateTime.Now;
                if (message.Payload.ClosestPiece == 0 && agent.Piece == null)
                {
                    logger.Info("Process move response: agent pick up piece." + " AgentID: " + agent.id.ToString());
                    return agent.PickUp();
                }
            }
            else
            {
                agent.AgentInformationsComponent.DeniedLastMove = true;
                logger.Info("Process move response: agent did not move." + " AgentID: " + agent.id.ToString());
                var deniedField = Common.GetFieldInDirection(agent.BoardLogicComponent.Position, agent.AgentInformationsComponent.LastDirection);
                if (Common.OnBoard(deniedField, agent.BoardLogicComponent.BoardSize)) agent.BoardLogicComponent.Board[deniedField.Y, deniedField.X].deniedMove = DateTime.Now;
            }
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<PickUpPieceResponse> message)
        {
            if (agent.AgentState != AgentState.InGame)
            {
                logger.Warn("Process pick up piece response: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }
            if (agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece == 0)
            {
                logger.Info("Process pick up piece response: Agent picked up piece" + " AgentID: " + agent.id.ToString());
                agent.Piece = new Piece();
            }
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<PutDownPieceResponse> message)
        {
            if (agent.AgentState != AgentState.InGame)
            {
                logger.Warn("Process put down piece response: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }
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
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<ExchangeInformationRequestForward> message)
        {
            if (agent.AgentState != AgentState.InGame)
            {
                logger.Warn("Process exchange information payload: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }
            if (message.Payload.Leader)
            {
                logger.Info("Process exchange information payload: Agent give info to leader" + " AgentID: " + agent.id.ToString());
                return agent.GiveInfo(message.Payload.AskingAgentId);
            }
            if (message.Payload.TeamId != agent.StartGameComponent.Team)
            {
                logger.Info("Process exchange information payload: Agent got request from opposite team, rejecting " + " AgentID: " + agent.id.ToString());
                return agent.MakeDecisionFromStrategy();
            }
            else
            {
                agent.WaitingPlayers.Add(message.Payload.AskingAgentId);
                return agent.MakeDecisionFromStrategy();
            }
        }

        public ActionResult Process(Message<JoinResponse> message)
        {
            if (agent.AgentState != AgentState.WaitingForJoin)
            {
                logger.Warn("Process join response: Agent not waiting for join" + " AgentID: " + agent.id.ToString());
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }
            if (message.Payload.Accepted)
            {
                bool wasWaiting = agent.AgentState == AgentState.WaitingForJoin;
                agent.AgentState = AgentState.WaitingForStart;
                agent.id = message.Payload.AgentId;
                return wasWaiting ? ActionResult.Continue : agent.MakeDecisionFromStrategy();
            }
            else
            {
                logger.Info("Process join response: Join request not accepted" + " AgentID: " + agent.id.ToString());
                return ActionResult.Finish;
            }
        }

        public ActionResult Process(Message<StartGamePayload> message)
        {
            if (agent.AgentState != AgentState.WaitingForStart)
            {
                logger.Warn("Process start game payload: Agent not waiting for startjoin" + " AgentID: " + agent.id.ToString());
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }
            agent.StartGameComponent.Initialize(message.Payload);
            if (agent.id != message.Payload.AgentId)
            {
                logger.Warn("Process start game payload: payload.agnetId not equal agentId" + " AgentID: " + agent.id.ToString());
            }
            agent.AgentState = AgentState.InGame;
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<EndGamePayload> message)
        {
            if (agent.AgentState != AgentState.InGame)
            {
                logger.Warn("Process end game payload: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }
            logger.Info("Process End Game: end game" + " AgentID: " + agent.id.ToString());
            return ActionResult.Finish;
        }

        public ActionResult Process(Message<IgnoredDelayError> message)
        {
            if (agent.AgentState != AgentState.InGame)
            {
                logger.Warn("Process ignoreed delay error: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }
            logger.Warn("IgnoredDelay error" + " AgentID: " + agent.id.ToString());
            agent.AgentInformationsComponent.DeniedLastRequest = true;
            var time = message.Payload.RemainingDelay;
            agent.SetPenalty(time.TotalSeconds, false);
            return ActionResult.Continue;
        }

        public ActionResult Process(Message<MoveError> message)
        {
            if (agent.AgentState != AgentState.InGame)
            {
                logger.Warn("Process move error: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }
            logger.Warn("Move error" + " AgentID: " + agent.id.ToString());
            agent.AgentInformationsComponent.DeniedLastMove = true;
            agent.BoardLogicComponent.Position = message.Payload.Position;
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<PickUpPieceError> message)
        {
            if (agent.AgentState != AgentState.InGame)
            {
                logger.Warn("Process pick up piece error: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }
            logger.Warn("Pick up piece error" + " AgentID: " + agent.id.ToString());
            if (message.Payload.ErrorSubtype == PickUpPieceErrorSubtype.NothingThere)
            {
                agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distLearned = DateTime.Now;
                agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece = int.MaxValue;
            }
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<PutDownPieceError> message)
        {
            if (agent.AgentState != AgentState.InGame)
            {
                logger.Warn("Process put down piece error: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.EndIfUnexpectedMessage) return ActionResult.Finish;
            }
            logger.Warn("Put down piece error" + " AgentID: " + agent.id.ToString());
            if (message.Payload.ErrorSubtype == PutDownPieceErrorSubtype.AgentNotHolding) agent.Piece = null;
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<UndefinedError> message)
        {
            logger.Warn("Undefined error" + " AgentID: " + agent.id.ToString());
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
