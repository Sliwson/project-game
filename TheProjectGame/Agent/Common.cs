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
            Point target = Common.GetFieldInDirection(agent.position, direction);
            return OnBoard(target, agent.boardSize) &&
                DateTime.Now - agent.board[target.Y, target.X].deniedMove > shortTime * TimeSpan.FromMilliseconds(agent.penaltyTime);
        }

        public static Direction GetGoalDirection(Agent agent, int shortTime)
        {
            if (agent.team == TeamId.Red)
            {
                foreach (var direction in new [] { Direction.North, Direction.West, Direction.East })
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
            if (stayInLineCount > agent.boardSize.X) return GetGoalDirection(agent, shortTime);
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
            return agent.board[agent.position.Y, agent.position.X].goalInfo == GoalInformation.NoInformation;
        }

        public static int FindClosest(Agent agent, int shortTime, out Direction direction)
        {
            int shortest = int.MaxValue;
            direction = Direction.North;
            for (int i = agent.position.X - 1; i <= agent.position.X + 1; i++)
                for (int j = agent.position.Y - 1; j <= agent.position.Y + 1; j++)
                    if ((i != agent.position.X || j != agent.position.Y) &&
                        OnBoard(new Point(i, j), agent.boardSize) &&
                        DateTime.Now - agent.board[j, i].distLearned > TimeSpan.FromMilliseconds(shortTime * agent.penaltyTime) &&
                        agent.board[j, i].distToPiece < Math.Min(shortest, agent.board[agent.position.Y, agent.position.X].distToPiece))
                    {
                        shortest = agent.board[j, i].distToPiece;
                        if (j > agent.position.Y) direction = Direction.North;
                        else if (j < agent.position.Y) direction = Direction.South;
                        else if (i < agent.position.X) direction = Direction.West;
                        else direction = Direction.East;
                    }
            return shortest;
        }

        public static int CountUndiscoveredFields(Agent agent, int shortTime)
        {
            int count = 0;
            for (int i = agent.position.X - 1; i <= agent.position.X + 1; i++)
                for (int j = agent.position.Y - 1; j <= agent.position.Y + 1; j++)
                    if (OnBoard(new Point(i, j), agent.boardSize) &&
                        DateTime.Now - agent.board[j, i].distLearned > TimeSpan.FromMilliseconds(shortTime * agent.penaltyTime))
                        count++;
            return count;
        }
    }
}
