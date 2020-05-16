using Agent.Enums;
using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Agent.strategies
{
    class WinningStrategy : IStrategy
    {
        private const int shortTime = 10;

        private const float smallShamProbability = 0.3f;

        private const int askInterval = int.MaxValue;

        private const double manyDenies = 0.35;

        public ActionResult MakeForcedDecision(Agent agent, SpecificActionType action, int argument = -1)
        {
            switch (action)
            {
                case SpecificActionType.DestroyPiece:
                    return agent.DestroyPiece();
                case SpecificActionType.GiveInfo:
                    return agent.GiveInfo(argument);
                case SpecificActionType.PickUp:
                    return agent.PickUp();
                default:
                    return MakeDecision(agent);
            }
        }

        public ActionResult MakeDecision(Agent agent)
        {
            if (agent.AgentInformationsComponent.IsComingBack && Common.IsBack(agent))
            {
                agent.AgentInformationsComponent.IsComingBack = false;
            }
            agent.AgentInformationsComponent.DidNotAskCount = agent.AgentInformationsComponent.DidNotAskCount + 1;
            if (agent.WaitingPlayers.Count > 0)
            {
                return agent.GiveInfo();
            }
            if (agent.AgentInformationsComponent.DidNotAskCount > askInterval &&
                agent.AgentInformationsComponent.TeamMatesToAsk.Length > 0)
            {
                agent.AgentInformationsComponent.DidNotAskCount = 0;
                return agent.BegForInfo();
            }
            if (agent.Piece != null &&
                !agent.Piece.isDiscovered &&
                agent.StartGameComponent.ShamPieceProbability > smallShamProbability)
            {
                return agent.CheckPiece();
            }
            if (agent.Piece != null &&
                !Common.DoesAgentKnowGoalInfo(agent) &&
                Common.InRectangle(agent.BoardLogicComponent.Position, agent.AgentInformationsComponent.OwnGoalArea))
            {
                return agent.Put();
            }
            Direction movingDirection = Common.GetRandomDirection();
            if (agent.Piece != null &&
                Common.InRectangle(agent.BoardLogicComponent.Position, agent.AgentInformationsComponent.OwnGoalArea) &&
                !agent.AgentInformationsComponent.IsComingBack)
            {
                movingDirection = Common.StayInRectangle(agent, shortTime, agent.AgentInformationsComponent.StayInLineCount, agent.AgentInformationsComponent.DirectionEastWest, out bool shouldComeBack);
                if (shouldComeBack)
                {
                    agent.AgentInformationsComponent.IsComingBack = true;
                }
                else if (movingDirection != agent.AgentInformationsComponent.DirectionEastWest)
                {
                    movingDirection = agent.StartGameComponent.Team == TeamId.Red ? Direction.North : Direction.South;
                    if (!Common.InRectangle(Common.GetFieldInDirection(agent.BoardLogicComponent.Position, movingDirection), agent.AgentInformationsComponent.OwnGoalArea) ||
                        !Common.CouldMove(agent, movingDirection, shortTime))
                    {
                        agent.AgentInformationsComponent.IsComingBack = true;
                    }
                }
            }
            if (agent.Piece != null)
            {
                if (!Common.InRectangle(agent.BoardLogicComponent.Position, agent.AgentInformationsComponent.OwnGoalArea) ||
                    agent.AgentInformationsComponent.IsComingBack)
                {
                    movingDirection = Common.GetOwnGoalDirection(agent, shortTime);
                }
                movingDirection = Common.FixDirection(agent, movingDirection, manyDenies);
                if (!Common.InRectangle(agent.BoardLogicComponent.Position, agent.AgentInformationsComponent.OwnGoalArea) ||
                    agent.AgentInformationsComponent.IsComingBack)
                {
                    agent.AgentInformationsComponent.StayInLineCount = 0;
                }
                else
                {
                    if (movingDirection == agent.AgentInformationsComponent.DirectionEastWest)
                    {
                        agent.AgentInformationsComponent.StayInLineCount = agent.AgentInformationsComponent.StayInLineCount + 1;
                    }
                    else
                    {
                        agent.AgentInformationsComponent.StayInLineCount = 0;
                        agent.AgentInformationsComponent.DirectionEastWest = agent.AgentInformationsComponent.DirectionEastWest.GetOppositeDirection();
                    }
                }
                return agent.Move(movingDirection);
            }
            if (!agent.AgentInformationsComponent.Discovered)
            {
                return agent.Discover();
            }
            Common.FindClosest(agent, 0, out movingDirection);
            movingDirection = Common.FixDirection(agent, movingDirection, manyDenies);
            return agent.Move(movingDirection);
        }
    }
}
