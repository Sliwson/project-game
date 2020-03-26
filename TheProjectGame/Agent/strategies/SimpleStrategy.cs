using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Agent.strategies
{
    class SimpleStrategy : IStrategy
    {
        private const int shortPieceDistance = 4;

        private const int shortTime = 4;

        private const int smallUndiscoveredNumber = 4;

        private int stayInLineCount = 0;

        public void MakeDecision(Agent agent)
        {
            if (!Common.InGoalArea(agent.team, agent.position, agent.boardSize, agent.goalAreaSize)) stayInLineCount = 0;
            if (agent.waitingPlayers.Count > 0)
            {
                agent.GiveInfo();
                return;
            }
            if (agent.piece != null && !agent.piece.isDiscovered)
            {
                agent.CheckPiece();
                return;
            }
            if (agent.piece != null && agent.piece.isDiscovered &&
                Common.DoesAgentKnowGoalInfo(agent) &&
                Common.InGoalArea(agent.team, agent.position, agent.boardSize, agent.goalAreaSize))
            {
                agent.Put();
                stayInLineCount = 0;
                return;
            }
            if (agent.piece != null && agent.piece.isDiscovered &&
                Common.InGoalArea(agent.team, agent.position, agent.boardSize, agent.goalAreaSize))
            {
                var dir = Common.StayInGoalArea(agent, shortTime, stayInLineCount);
                agent.Move(dir);
                if (!Common.IsDirectionGoalDirection(dir)) stayInLineCount++;
                else stayInLineCount = 0;
                return;
            }
            if (agent.piece != null && agent.piece.isDiscovered)
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
