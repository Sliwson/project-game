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
        private const int shortPieceDistance = 4;

        private const int shortTime = 4;

        private const int smallUndiscoveredNumber = 4;

        private const float smallShamProbability = 0.3f;

        private readonly Dictionary<ActionType, int> actionImportance = new Dictionary<ActionType, int>
        {
            { ActionType.InformationResponse, 5 },
            { ActionType.CheckForSham, 4 },
            { ActionType.PutPiece, 3 },
            { ActionType.Move, 2 },
            { ActionType.Discovery, 1 },
            { ActionType.InformationRequest, 0 }
        };

        private int stayInLineCount = 0;

        private bool IsActionExpensive(ActionType action, Dictionary<ActionType, TimeSpan> penalties)
        {
            if (!penalties.ContainsKey(action)) return false;
            int thisAction = (int)penalties[action].TotalMilliseconds;
            int maxAction = (int)penalties.Where(a => actionImportance[a.Key] < actionImportance[action]).Max(x => (int)x.Value.TotalMilliseconds);
            int averageAction = (int)penalties.Where(a => actionImportance[a.Key] < actionImportance[action]).Average(x => (int)x.Value.TotalMilliseconds);
            if (thisAction > 2 * averageAction) return true;
            return false;
        }

        public bool MakeDecision(Agent agent)
        {
            if (!Common.InGoalArea(agent.team, agent.position, agent.boardSize, agent.goalAreaSize)) stayInLineCount = 0;
            if (agent.waitingPlayers.Count > 0 && !IsActionExpensive(ActionType.InformationResponse, agent.penalties))
            {
                return agent.GiveInfo();
            }
            if (agent.piece != null &&
                !agent.piece.isDiscovered &&
                agent.shamPieceProbability > smallShamProbability &&
                !IsActionExpensive(ActionType.CheckForSham, agent.penalties))
            {
                return agent.CheckPiece();
            }
            if (agent.piece != null &&
                Common.DoesAgentKnowGoalInfo(agent) &&
                Common.InGoalArea(agent.team, agent.position, agent.boardSize, agent.goalAreaSize))
            {
                stayInLineCount = 0;
                return agent.Put();
            }
            if (agent.piece != null &&
                Common.InGoalArea(agent.team, agent.position, agent.boardSize, agent.goalAreaSize))
            {
                var dir = Common.StayInGoalArea(agent, shortTime, stayInLineCount);
                if (!Common.IsDirectionGoalDirection(dir)) stayInLineCount++;
                else stayInLineCount = 0;
                return agent.Move(dir);
            }
            if (agent.piece != null)
            {
                return agent.Move(Common.GetGoalDirection(agent, shortTime));
            }
            if (Common.FindClosest(agent, shortTime, out Direction direction) <= shortPieceDistance)
            {
                return agent.Move(direction);
            }
            if (Common.CountUndiscoveredFields(agent, shortTime) > smallUndiscoveredNumber)
            {
                return agent.Discover();
            }
            return agent.BegForInfo();
        }
    }
}
