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
            if (agent.agentState != AgentState.InGame)
            {
                logger.Warn("Process check scham response: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.endIfUnexpectedMessage) return ActionResult.Finish;
            }
            if (message.Payload.Sham)
            {
                logger.Info("Process check scham response: Agent checked sham and destroy piece." + " AgentID: " + agent.id.ToString());
                return agent.DestroyPiece();
            }
            else
            {
                logger.Info("Process check scham response: Agent checked not sham." + " AgentID: " + agent.id.ToString());
                agent.piece.isDiscovered = true;
                return agent.MakeDecisionFromStrategy();
            }
        }

        public ActionResult Process(Message<DestroyPieceResponse> message)
        {
            if (agent.agentState != AgentState.InGame)
            {
                logger.Warn("Process destroy piece response: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.endIfUnexpectedMessage) return ActionResult.Finish;
            }
            agent.piece = null;
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<DiscoverResponse> message)
        {
            if (agent.agentState != AgentState.InGame)
            {
                logger.Warn("Process discover response: Agent not in game." + " AgentID: " + agent.id.ToString());
                if (agent.endIfUnexpectedMessage) return ActionResult.Finish;
            }
            DateTime now = DateTime.Now;
            for (int y = agent.startGameComponent.position.Y - 1; y <= agent.startGameComponent.position.Y + 1; y++)
            {
                for (int x = agent.startGameComponent.position.X - 1; x <= agent.startGameComponent.position.X + 1; x++)
                {
                    int taby = y - agent.startGameComponent.position.Y + 1;
                    int tabx = x - agent.startGameComponent.position.X + 1;
                    if (Common.OnBoard(new Point(x, y), agent.boardLogicComponent.boardSize))
                    {
                        agent.boardLogicComponent.board[y, x].distToPiece = message.Payload.Distances[taby, tabx];
                        agent.boardLogicComponent.board[y, x].distLearned = now;
                    }
                }
            }
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<ExchangeInformationResponse> message)
        {
            if (agent.agentState != AgentState.InGame)
            {
                logger.Warn("Process exchange information response: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.endIfUnexpectedMessage) return ActionResult.Finish;
            }
            agent.boardLogicComponent.UpdateDistances(message.Payload.Distances);
            agent.boardLogicComponent.UpdateBlueTeamGoalAreaInformation(message.Payload.BlueTeamGoalAreaInformation);
            agent.boardLogicComponent.UpdateRedTeamGoalAreaInformation(message.Payload.RedTeamGoalAreaInformation);
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<MoveResponse> message)
        {
            if (agent.agentState != AgentState.InGame)
            {
                logger.Warn("Process move response: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.endIfUnexpectedMessage) return ActionResult.Finish;
            }
            agent.startGameComponent.position = message.Payload.CurrentPosition;
            if (message.Payload.MadeMove)
            {
                agent.deniedLastMove = false;
                agent.board[agent.position.Y, agent.position.X].distToPiece = message.Payload.ClosestPiece;
                agent.board[agent.position.Y, agent.position.X].distLearned = DateTime.Now;
                if (message.Payload.ClosestPiece == 0/* && board[position.Y, position.X].goalInfo == GoalInformation.NoInformation*/)
                {
                    logger.Info("Process move response: agent pick up piece." + " AgentID: " + agent.id.ToString());
                    return agent.PickUp();
                }
            }
            else
            {
                agent.deniedLastMove = true;
                logger.Info("Process move response: agent did not move." + " AgentID: " + agent.id.ToString());
                var deniedField = Common.GetFieldInDirection(agent.startGameComponent.position, agent.lastDirection);
                if (Common.OnBoard(deniedField, agent.boardLogicComponent.boardSize)) agent.boardLogicComponent.board[deniedField.Y, deniedField.X].deniedMove = DateTime.Now;
            }
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<PickUpPieceResponse> message)
        {
            if (agent.agentState != AgentState.InGame)
            {
                logger.Warn("Process pick up piece response: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.endIfUnexpectedMessage) return ActionResult.Finish;
            }
            if (agent.boardLogicComponent.board[agent.startGameComponent.position.Y, agent.startGameComponent.position.X].distToPiece == 0)
            {
                logger.Info("Process pick up piece response: Agent picked up piece" + " AgentID: " + agent.id.ToString());
                agent.piece = new Piece();
            }
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<PutDownPieceResponse> message)
        {
            if (agent.agentState != AgentState.InGame)
            {
                logger.Warn("Process put down piece response: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.endIfUnexpectedMessage) return ActionResult.Finish;
            }
            agent.piece = null;
            switch (message.Payload.Result)
            {
                case PutDownPieceResult.NormalOnGoalField:
                    agent.boardLogicComponent.board[agent.startGameComponent.position.Y, agent.startGameComponent.position.X].goalInfo = GoalInformation.Goal;
                    break;
                case PutDownPieceResult.NormalOnNonGoalField:
                    agent.boardLogicComponent.board[agent.startGameComponent.position.Y, agent.startGameComponent.position.X].goalInfo = GoalInformation.NoGoal;
                    break;
                case PutDownPieceResult.ShamOnGoalArea:
                    break;
                case PutDownPieceResult.TaskField:
                    agent.boardLogicComponent.board[agent.startGameComponent.position.Y, agent.startGameComponent.position.X].goalInfo = GoalInformation.NoGoal;
                    agent.boardLogicComponent.board[agent.startGameComponent.position.Y, agent.startGameComponent.position.X].distToPiece = 0;
                    agent.boardLogicComponent.board[agent.startGameComponent.position.Y, agent.startGameComponent.position.X].distLearned = DateTime.Now;
                    break;
            }
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<ExchangeInformationPayload> message)
        {
            if (agent.agentState != AgentState.InGame)
            {
                logger.Warn("Process exchange information payload: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.endIfUnexpectedMessage) return ActionResult.Finish;
            }
            if (message.Payload.Leader)
            {
                logger.Info("Process exchange information payload: Agent give info to leader" + " AgentID: " + agent.id.ToString());
                return agent.GiveInfo(message.Payload.AskingAgentId);
            }
            else
            {
                agent.waitingPlayers.Add(message.Payload.AskingAgentId);
                return agent.MakeDecisionFromStrategy();
            }
        }

        public ActionResult Process(Message<JoinResponse> message)
        {
            if (agent.agentState != AgentState.WaitingForJoin)
            {
                logger.Warn("Process join response: Agent not waiting for join" + " AgentID: " + agent.id.ToString());
                if (agent.endIfUnexpectedMessage) return ActionResult.Finish;
            }
            if (message.Payload.Accepted)
            {
                bool wasWaiting = agent.agentState == AgentState.WaitingForJoin;
                agent.agentState = AgentState.WaitingForStart;
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
            if (agent.agentState != AgentState.WaitingForStart)
            {
                logger.Warn("Process start game payload: Agent not waiting for startjoin" + " AgentID: " + agent.id.ToString());
                if (agent.endIfUnexpectedMessage) return ActionResult.Finish;
            }
            agent.startGameComponent.Initialize(message.Payload);
            if (agent.id != message.Payload.AgentId)
            {
                logger.Warn("Process start game payload: payload.agnetId not equal agentId" + " AgentID: " + agent.id.ToString());
            }
            agent.agentState = AgentState.InGame;
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<EndGamePayload> message)
        {
            if (agent.agentState != AgentState.InGame)
            {
                logger.Warn("Process end game payload: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.endIfUnexpectedMessage) return ActionResult.Finish;
            }
            logger.Info("Process End Game: end game" + " AgentID: " + agent.id.ToString());
            return ActionResult.Finish;
        }

        public ActionResult Process(Message<IgnoredDelayError> message)
        {
            if (agent.agentState != AgentState.InGame)
            {
                logger.Warn("Process ignoreed delay error: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.endIfUnexpectedMessage) return ActionResult.Finish;
            }
            logger.Warn("IgnoredDelay error" + " AgentID: " + agent.id.ToString());
            var time = message.Payload.RemainingDelay;
            agent.remainingPenalty = Math.Max(0.0, time.TotalSeconds);
            return ActionResult.Continue;
        }

        public ActionResult Process(Message<MoveError> message)
        {
            if (agent.agentState != AgentState.InGame)
            {
                logger.Warn("Process move error: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.endIfUnexpectedMessage) return ActionResult.Finish;
            }
            logger.Warn("Move error" + " AgentID: " + agent.id.ToString());
            agent.deniedLastMove = true;
            agent.startGameComponent.position = message.Payload.Position;
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<PickUpPieceError> message)
        {
            if (agent.agentState != AgentState.InGame)
            {
                logger.Warn("Process pick up piece error: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.endIfUnexpectedMessage) return ActionResult.Finish;
            }
            logger.Warn("Pick up piece error" + " AgentID: " + agent.id.ToString());
            agent.boardLogicComponent.board[agent.startGameComponent.position.Y, agent.startGameComponent.position.X].distLearned = DateTime.Now;
            agent.boardLogicComponent.board[agent.startGameComponent.position.Y, agent.startGameComponent.position.X].distToPiece = int.MaxValue;
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<PutDownPieceError> message)
        {
            if (agent.agentState != AgentState.InGame)
            {
                logger.Warn("Process put down piece error: Agent not in game" + " AgentID: " + agent.id.ToString());
                if (agent.endIfUnexpectedMessage) return ActionResult.Finish;
            }
            logger.Warn("Put down piece error" + " AgentID: " + agent.id.ToString());
            if (message.Payload.ErrorSubtype == PutDownPieceErrorSubtype.AgentNotHolding) agent.piece = null;
            return agent.MakeDecisionFromStrategy();
        }

        public ActionResult Process(Message<UndefinedError> message)
        {
            logger.Warn("Undefined error" + " AgentID: " + agent.id.ToString());
            return agent.MakeDecisionFromStrategy();
        }
    }
}
