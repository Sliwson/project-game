using Agent.Enums;
using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Agent.strategies
{
    class SimpleStrategy : IStrategy
    {
        private const int shortTime = 10;

        private const float smallShamProbability = 0.3f;

        private const int askInterval = 20;

        private readonly Dictionary<ActionType, int> actionImportance = new Dictionary<ActionType, int>
        {
            { ActionType.CheckForSham, 4 },
            { ActionType.PutPiece, 3 },
            { ActionType.Move, 2 },
            { ActionType.Discovery, 1 },
            { ActionType.InformationExchange, 0 },
            { ActionType.DestroyPiece, 6 }
        };

        private bool IsActionExpensive(ActionType action, Dictionary<ActionType, TimeSpan> penalties)
        {
            if (!penalties.ContainsKey(action)) return false;
            int thisAction = (int)penalties[action].TotalMilliseconds;
            int maxAction = (int)penalties.Where(a => actionImportance[a.Key] < actionImportance[action]).Max(x => (int)x.Value.TotalMilliseconds);
            int averageAction = (int)penalties.Where(a => actionImportance[a.Key] < actionImportance[action]).Average(x => (int)x.Value.TotalMilliseconds);
            if (thisAction > 2 * averageAction) return true;
            return false;
        }

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
            if (!Common.InRectangle(agent.BoardLogicComponent.Position, agent.AgentInformationsComponent.OwnGoalArea))
            {
                agent.AgentInformationsComponent.StayInLineCount = 0;
            }
            if (agent.AgentInformationsComponent.IsComingBack && Common.IsBack(agent))
            {
                agent.AgentInformationsComponent.IsComingBack = false;
                agent.AgentInformationsComponent.StayInLineCount = 0;
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
                agent.AgentInformationsComponent.StayInLineCount = 0;
                return agent.Put();
            }
            if (agent.Piece != null &&
                Common.InRectangle(agent.BoardLogicComponent.Position, agent.AgentInformationsComponent.OwnGoalArea) &&
                !agent.AgentInformationsComponent.IsComingBack)
            {
                var dir = Common.StayInRectangle(agent, shortTime, agent.AgentInformationsComponent.StayInLineCount, agent.AgentInformationsComponent.DirectionEastWest, out bool shouldComeBack);
                if (shouldComeBack)
                {
                    agent.AgentInformationsComponent.IsComingBack = true;
                }
                else
                {
                    if (!Common.IsDirectionGoalDirection(dir))
                    {
                        if (dir == agent.AgentInformationsComponent.DirectionEastWest)
                        {
                            agent.AgentInformationsComponent.StayInLineCount = agent.AgentInformationsComponent.StayInLineCount + 1;
                        }
                        else
                        {
                            dir = agent.StartGameComponent.Team == TeamId.Red ? Direction.North : Direction.South;
                            if (!Common.InRectangle(Common.GetFieldInDirection(agent.BoardLogicComponent.Position, dir), agent.AgentInformationsComponent.OwnGoalArea) ||
                                !Common.CouldMove(agent, dir, shortTime))
                            {
                                agent.AgentInformationsComponent.IsComingBack = true;
                            }
                        }
                    }
                    if (Common.IsDirectionGoalDirection(dir))
                    {
                        agent.AgentInformationsComponent.StayInLineCount = 0;
                        agent.AgentInformationsComponent.DirectionEastWest =
                            agent.AgentInformationsComponent.DirectionEastWest == Direction.West ?
                            Direction.East : Direction.West;
                    }
                }
                if (!agent.AgentInformationsComponent.IsComingBack)
                {
                    return agent.Move(dir);
                }
            }
            if (agent.Piece != null)
            {
                return agent.Move(Common.GetOwnGoalDirection(agent, shortTime));
            }
            if (!agent.AgentInformationsComponent.Discovered)
            {
                return agent.Discover();
            }
            Common.FindClosest(agent, shortTime, out Direction direction);
            if (agent.AgentInformationsComponent.DeniedLastMove &&
                direction == agent.AgentInformationsComponent.LastDirection)
            {
                direction = Common.GetRandomDirection();
            }
            return agent.Move(direction);
        }
    }
}
