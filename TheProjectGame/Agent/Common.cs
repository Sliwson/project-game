using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Agent
{
    public class Common
    {
        public static Point GetFieldInDirection(Point position, Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return new Point(position.X, position.Y + 1);
                case Direction.East:
                    return new Point(position.X + 1, position.Y);
                case Direction.South:
                    return new Point(position.X, position.Y - 1);
                case Direction.West:
                    return new Point(position.X - 1, position.Y);
            }
            return position;
        }

        public static bool InGoalArea(TeamId team, Point position, Point boardSize, int goalAreaSize)
        {
            return team == TeamId.Red ? position.Y >= boardSize.Y - goalAreaSize : position.Y < goalAreaSize;
        }

        public static bool OnBoard(Point position, Point boardSize)
        {
            return position.X >= 0 && position.Y >= 0 && position.X < boardSize.X && position.Y < boardSize.Y;
        }

        public static bool CouldMove(Agent agent, Direction direction, int shortTime)
        {
            Point target = Common.GetFieldInDirection(agent.StartGameComponent.position, direction);
            return OnBoard(target, agent.BoardLogicComponent.boardSize) &&
                !InGoalArea(agent.StartGameComponent.team == TeamId.Red ? TeamId.Blue : TeamId.Red, target, agent.BoardLogicComponent.boardSize, agent.BoardLogicComponent.goalAreaSize) &&
                DateTime.Now - agent.BoardLogicComponent.board[target.Y, target.X].deniedMove > shortTime * TimeSpan.FromMilliseconds(agent.StartGameComponent.averageTime);
        }

        public static Direction GetGoalDirection(Agent agent, int shortTime)
        {
            if (agent.StartGameComponent.team == TeamId.Red)
            {
                foreach (var direction in new[] { Direction.North, Direction.West, Direction.East })
                    if (CouldMove(agent, direction, shortTime)) return direction;
                return Direction.North;
            }
            else
            {
                foreach (var direction in new[] { Direction.South, Direction.West, Direction.East })
                    if (CouldMove(agent, direction, shortTime)) return direction;
                return Direction.South;
            }
        }

        public static Direction StayInGoalArea(Agent agent, int shortTime, int stayInLineCount)
        {
            if (stayInLineCount > agent.BoardLogicComponent.boardSize.X) return GetGoalDirection(agent, shortTime);
            if (CouldMove(agent, Direction.East, shortTime)) return Direction.East;
            if (CouldMove(agent, Direction.West, shortTime)) return Direction.West;
            return GetGoalDirection(agent, shortTime);
        }

        public static bool IsDirectionGoalDirection(Direction direction)
        {
            return direction == Direction.North || direction == Direction.South;
        }

        public static bool DoesAgentKnowGoalInfo(Agent agent)
        {
            return agent.BoardLogicComponent.board[agent.StartGameComponent.position.Y, agent.StartGameComponent.position.X].goalInfo != GoalInformation.NoInformation;
        }

        public static int FindClosest(Agent agent, int shortTime, out Direction direction)
        {
            int shortest = int.MaxValue;
            direction = Direction.North;
            for (int i = agent.StartGameComponent.position.X - 1; i <= agent.StartGameComponent.position.X + 1; i++)
                for (int j = agent.StartGameComponent.position.Y - 1; j <= agent.StartGameComponent.position.Y + 1; j++)
                    if ((i != agent.StartGameComponent.position.X || j != agent.StartGameComponent.position.Y) &&
                        OnBoard(new Point(i, j), agent.BoardLogicComponent.boardSize) &&
                        DateTime.Now - agent.BoardLogicComponent.board[j, i].distLearned > TimeSpan.FromMilliseconds(shortTime * agent.StartGameComponent.averageTime) &&
                        agent.BoardLogicComponent.board[j, i].distToPiece < Math.Min(shortest, agent.BoardLogicComponent.board[agent.StartGameComponent.position.Y, agent.StartGameComponent.position.X].distToPiece))
                    {
                        shortest = agent.BoardLogicComponent.board[j, i].distToPiece;
                        if (j > agent.StartGameComponent.position.Y) direction = Direction.North;
                        else if (j < agent.StartGameComponent.position.Y) direction = Direction.South;
                        else if (i < agent.StartGameComponent.position.X) direction = Direction.West;
                        else direction = Direction.East;
                    }
            return shortest;
        }

        public static int CountUndiscoveredFields(Agent agent, int shortTime)
        {
            int count = 0;
            for (int i = agent.StartGameComponent.position.X - 1; i <= agent.StartGameComponent.position.X + 1; i++)
                for (int j = agent.StartGameComponent.position.Y - 1; j <= agent.StartGameComponent.position.Y + 1; j++)
                    if (OnBoard(new Point(i, j), agent.BoardLogicComponent.boardSize) &&
                        DateTime.Now - agent.BoardLogicComponent.board[j, i].distLearned > TimeSpan.FromMilliseconds(shortTime * agent.StartGameComponent.averageTime))
                        count++;
            return count;
        }
    }
}
