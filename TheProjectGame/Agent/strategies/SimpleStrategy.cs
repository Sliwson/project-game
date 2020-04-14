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

        private const int shortTime = 1000;

        private const int smallUndiscoveredNumber = 6;

        private const float smallShamProbability = 0.3f;

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

        private bool IsActionExpensive(ActionType action, Dictionary<ActionType, TimeSpan> penalties)
        {
            if (!penalties.ContainsKey(action)) return false;
            int thisAction = (int)penalties[action].TotalMilliseconds;
            int maxAction = (int)penalties.Where(a => actionImportance[a.Key] < actionImportance[action]).Max(x => (int)x.Value.TotalMilliseconds);
            int averageAction = (int)penalties.Where(a => actionImportance[a.Key] < actionImportance[action]).Average(x => (int)x.Value.TotalMilliseconds);
            if (thisAction > 2 * averageAction) return true;
            return false;
        }
        private bool discovered = false;
        public Point target;
        public Random random = new Random();
        private ActionResult MoveSomewhere(Agent agent)
        {
            if (agent.piece != null && Common.InGoalArea(agent.startGameComponent.team, agent.startGameComponent.position, agent.boardLogicComponent.boardSize, agent.boardLogicComponent.goalAreaSize))
            {
                return agent.Put();
            }
            if (agent.piece != null)
            {
                return agent.Move(Common.GetGoalDirection(agent, shortTime));
            }
            if (agent.deniedLastMove || target.IsEmpty || (agent.startGameComponent.position.X == target.X && agent.startGameComponent.position.Y == target.Y)) target = new Point(random.Next(agent.boardLogicComponent.boardSize.X - 1), random.Next(agent.boardLogicComponent.boardSize.Y - 1));
            if (target.X < agent.startGameComponent.position.X &&
                Common.CouldMove(agent, Direction.West, shortTime) &&
                agent.lastDirection != Direction.East)
            {
                return agent.Move(Direction.West);
            }
            if (target.X > agent.startGameComponent.position.X &&
                Common.CouldMove(agent, Direction.East, shortTime) &&
                agent.lastDirection != Direction.West)
            {
                return agent.Move(Direction.East);
            }
            if (target.Y > agent.startGameComponent.position.Y &&
                Common.CouldMove(agent, Direction.North, shortTime) &&
                agent.lastDirection != Direction.South)
            {
                return agent.Move(Direction.North);
            }
            if (target.Y < agent.startGameComponent.position.Y &&
                Common.CouldMove(agent, Direction.South, shortTime) &&
                agent.lastDirection != Direction.North)
            {
                return agent.Move(Direction.South);
            }
            return agent.Move(Common.GetGoalDirection(agent, shortTime));
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
            if (agent.deniedLastMove && direction == agent.lastDirection)
                direction = direction.GetOppositeDirection();
            return agent.Move(direction);
        }

        public ActionResult MakeDecision(Agent agent)
        {
            if (!Common.InGoalArea(agent.team, agent.position, agent.boardSize, agent.goalAreaSize)) stayInLineCount = 0;
            if (agent.waitingPlayers.Count > 0 && !IsActionExpensive(ActionType.InformationExchange, agent.penalties))
            {
                return agent.GiveInfo();
            }
            if (agent.piece != null &&
                !agent.piece.isDiscovered &&
                agent.startGameComponent.shamPieceProbability > smallShamProbability &&
                !IsActionExpensive(ActionType.CheckForSham, agent.startGameComponent.penalties))
            {
                return agent.CheckPiece();
            }
            if (agent.piece != null &&
                !Common.DoesAgentKnowGoalInfo(agent) &&
                Common.InGoalArea(agent.startGameComponent.team, agent.startGameComponent.position, agent.boardLogicComponent.boardSize, agent.boardLogicComponent.goalAreaSize))
            {
                stayInLineCount = 0;
                return agent.Put();
            }
            if (agent.piece != null &&
                Common.InGoalArea(agent.startGameComponent.team, agent.startGameComponent.position, agent.boardLogicComponent.boardSize, agent.boardLogicComponent.goalAreaSize))
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
            return DiscoverAndMove(agent);
            //if (Common.FindClosest(agent, shortTime, out Direction direction) <= shortPieceDistance)
            //{
            //    return agent.Move(direction);
            //}
            //if (Common.CountUndiscoveredFields(agent, shortTime) > smallUndiscoveredNumber)
            //{
            //    return agent.Discover();
            //}
            //return MoveSomewhere(agent);
            //return agent.BegForInfo();
        }
    }
}
