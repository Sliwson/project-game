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
        private const int shortPieceDistance = 40;

        private const int shortTime = 10;

        private const int smallUndiscoveredNumber = 6;

        private const float smallShamProbability = 0.3f;

        private const int askInterval = 10;

        private readonly Dictionary<ActionType, int> actionImportance = new Dictionary<ActionType, int>
        {
            { ActionType.CheckForSham, 4 },
            { ActionType.PutPiece, 3 },
            { ActionType.Move, 2 },
            { ActionType.Discovery, 1 },
            { ActionType.InformationExchange, 0 },
            { ActionType.DestroyPiece, 6 }
        };

        private int stayInLineCount = 0;

        private int didNotAskCount = 0;

        private bool discovered = false;

        private bool IsActionExpensive(ActionType action, Dictionary<ActionType, TimeSpan> penalties)
        {
            if (!penalties.ContainsKey(action)) return false;
            int thisAction = (int)penalties[action].TotalMilliseconds;
            int maxAction = (int)penalties.Where(a => actionImportance[a.Key] < actionImportance[action]).Max(x => (int)x.Value.TotalMilliseconds);
            int averageAction = (int)penalties.Where(a => actionImportance[a.Key] < actionImportance[action]).Average(x => (int)x.Value.TotalMilliseconds);
            if (thisAction > 2 * averageAction) return true;
            return false;
        }

        private ActionResult DiscoverAndMove(Agent agent)
        {
            if (!discovered)
            {
                discovered = true;
                return agent.Discover();
            }
            discovered = false;
            Common.FindClosest(agent, shortTime, out Direction direction);
            if (agent.AgentInformationsComponent.DeniedLastMove && direction == agent.AgentInformationsComponent.LastDirection)
                direction = Common.GetRandomDirection();
            return agent.Move(direction);
        }

        public ActionResult MakeDecision(Agent agent)
        {
            if (!Common.InGoalArea(agent.StartGameComponent.Team, agent.BoardLogicComponent.Position, agent.BoardLogicComponent.BoardSize, agent.BoardLogicComponent.GoalAreaSize)) stayInLineCount = 0;
            didNotAskCount++;
            if (agent.WaitingPlayers.Count > 0)
            {
                return agent.GiveInfo();
            }
            if (didNotAskCount > askInterval && agent.StartGameComponent.TeamMates.Length > 0)
            {
                didNotAskCount = 0;
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
                Common.InGoalArea(agent.StartGameComponent.Team, agent.BoardLogicComponent.Position, agent.BoardLogicComponent.BoardSize, agent.BoardLogicComponent.GoalAreaSize))
            {
                stayInLineCount = 0;
                return agent.Put();
            }
            if (agent.Piece != null &&
                Common.InGoalArea(agent.StartGameComponent.Team, agent.BoardLogicComponent.Position, agent.BoardLogicComponent.BoardSize, agent.BoardLogicComponent.GoalAreaSize))
            {
                var dir = Common.StayInGoalArea(agent, shortTime, stayInLineCount);
                if (agent.AgentInformationsComponent.DeniedLastMove && dir == agent.AgentInformationsComponent.LastDirection)
                    dir = Common.GetRandomDirection();
                if (!Common.IsDirectionGoalDirection(dir)) stayInLineCount++;
                else stayInLineCount = 0;
                return agent.Move(dir);
            }
            if (agent.Piece != null)
            {
                return agent.Move(Common.GetGoalDirection(agent, shortTime));
            }
            return DiscoverAndMove(agent);
            //if (Common.FindClosest(agent, shortTime, out Direction direction) <= shortPieceDistance)
            //{
            //    return agent.Move(direction);
            //}
            //if (Common.CountUndiscoveredFields(agent, shortTime) > smallUndiscoveredNumber)
            //{
            //    return agent.Discover();
            //}
            //return agent.BegForInfo();
        }
    }
}
