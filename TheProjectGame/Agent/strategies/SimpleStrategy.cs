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

        public void MakeDecision(Agent agent)
        {
            if (!Common.InGoalArea(agent.team, agent.position, agent.boardSize, agent.goalAreaSize)) stayInLineCount = 0;
            if (agent.waitingPlayers.Count > 0 && !IsActionExpensive(ActionType.InformationResponse, agent.penalties))
            {
                agent.GiveInfo();
                return;
            }
            if (agent.piece != null &&
                !agent.piece.isDiscovered &&
                agent.shamPieceProbability > smallShamProbability &&
                !IsActionExpensive(ActionType.CheckForSham, agent.penalties))
            {
                agent.CheckPiece();
                return;
            }
            if (agent.piece != null &&
                Common.DoesAgentKnowGoalInfo(agent) &&
                Common.InGoalArea(agent.team, agent.position, agent.boardSize, agent.goalAreaSize))
            {
                agent.Put();
                stayInLineCount = 0;
                return;
            }
            if (agent.piece != null &&
                Common.InGoalArea(agent.team, agent.position, agent.boardSize, agent.goalAreaSize))
            {
                var dir = Common.StayInGoalArea(agent, shortTime, stayInLineCount);
                agent.Move(dir);
                if (!Common.IsDirectionGoalDirection(dir)) stayInLineCount++;
                else stayInLineCount = 0;
                return;
            }
            if (agent.piece != null)
            {
                agent.Move(Common.GetGoalDirection(agent, shortTime));
                return;
            }
            if (Common.FindClosest(agent, shortTime, out Direction direction) <= shortPieceDistance)
            {
                agent.Move(direction);
                return;
            }
            if (Common.CountUndiscoveredFields(agent, shortTime) > smallUndiscoveredNumber)
            {
                agent.Discover();
                return;
            }
            agent.BegForInfo();
        }
    }
}
