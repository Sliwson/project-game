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

        public static bool InGoalArea(Team team, Point position, Point boardSize, int goalAreaSize)
        {
            return team == Team.Red ? position.X >= boardSize.X - goalAreaSize : position.X < goalAreaSize;
        }

        public static bool OnBoard(Point position, Point boardSize)
        {
            return position.X >= 0 && position.Y >= 0 && position.X < boardSize.X && position.Y < boardSize.Y;
        }

        public static bool CouldMove(Agent agent, Direction direction, int shortTime)
        {
            Point target = Common.GetFieldInDirection(agent.position, direction);
            return OnBoard(target, agent.boardSize) &&
                DateTime.Now - agent.board[target.X, target.Y].deniedMove > shortTime * TimeSpan.FromMilliseconds(agent.penaltyTime);
        }

        public static Direction GetGoalDirection(Agent agent, int shortTime)
        {
            if (agent.team == Team.Red)
            {
                if (CouldMove(agent, Direction.East, shortTime)) return Direction.East;
                if (CouldMove(agent, Direction.North, shortTime)) return Direction.North;
                if (CouldMove(agent, Direction.South, shortTime)) return Direction.South;
                return Direction.East;
            }
            else
            {
                if (CouldMove(agent, Direction.West, shortTime)) return Direction.West;
                if (CouldMove(agent, Direction.North, shortTime)) return Direction.North;
                if (CouldMove(agent, Direction.South, shortTime)) return Direction.South;
                return Direction.West;
            }
        }

        public static Direction StayInGoalArea(Agent agent, int shortTime, int stayInLineCount)
        {
            if (stayInLineCount > agent.boardSize.Y) return GetGoalDirection(agent, shortTime);
            if (CouldMove(agent, Direction.North, shortTime)) return Direction.North;
            if (CouldMove(agent, Direction.South, shortTime)) return Direction.South;
            return GetGoalDirection(agent, shortTime);
        }

        public static int FindClosest(Agent agent, int shortTime, out Direction direction)
        {
            int shortest = int.MaxValue;
            direction = Direction.North;
            for (int i = agent.position.X - 1; i <= agent.position.X + 1; i++)
                for (int j = agent.position.Y - 1; j <= agent.position.Y + 1; j++)
                    if ((i != agent.position.X || j != agent.position.Y) &&
                        OnBoard(new Point(i, j), agent.boardSize) &&
                        DateTime.Now - agent.board[i, j].distLearned > TimeSpan.FromMilliseconds(shortTime * agent.penaltyTime) &&
                        agent.board[i, j].distToPiece < Math.Min(shortest, agent.board[agent.position.X, agent.position.Y].distToPiece))
                    {
                        shortest = agent.board[i, j].distToPiece;
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
                        DateTime.Now - agent.board[i, j].distLearned > TimeSpan.FromMilliseconds(shortTime * agent.penaltyTime))
                        count++;
            return count;
        }
    }
}
