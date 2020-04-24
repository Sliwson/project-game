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

        public static bool InRectangle(Point position, (Point, Point) rectangle)
        {
            return position.X >= Math.Min(rectangle.Item1.X, rectangle.Item2.X) &&
                position.X <= Math.Max(rectangle.Item1.X, rectangle.Item2.X) &&
                position.Y >= Math.Min(rectangle.Item1.Y, rectangle.Item2.Y) &&
                position.Y <= Math.Max(rectangle.Item1.Y, rectangle.Item2.Y);
        }

        public static bool OnBoard(Point position, Point boardSize)
        {
            return position.X >= 0 && position.Y >= 0 && position.X < boardSize.X && position.Y < boardSize.Y;
        }

        public static bool CouldMove(Agent agent, Direction direction, int shortTime)
        {
            Point target = GetFieldInDirection(agent.BoardLogicComponent.Position, direction);
            return OnBoard(target, agent.BoardLogicComponent.BoardSize) &&
                !InGoalArea(agent.StartGameComponent.Team == TeamId.Red ? TeamId.Blue : TeamId.Red, target, agent.BoardLogicComponent.BoardSize, agent.BoardLogicComponent.GoalAreaSize) &&
                OldTime(agent.BoardLogicComponent.Board[target.Y, target.X].deniedMove, shortTime, agent.StartGameComponent.AverageTime);
        }

        public static Direction GetGoalDirection(Agent agent, int shortTime, out bool shouldComeBack)
        {
            if (agent.StartGameComponent.Team == TeamId.Red)
            {
                foreach (var direction in new[] { Direction.North, Direction.West, Direction.East })
                    if (CouldMove(agent, direction, shortTime))
                    {
                        shouldComeBack = false;
                        return direction;
                    }
                shouldComeBack = true;
                return Direction.North;
            }
            else
            {
                foreach (var direction in new[] { Direction.South, Direction.West, Direction.East })
                    if (CouldMove(agent, direction, shortTime))
                    {
                        shouldComeBack = false;
                        return direction;
                    }
                shouldComeBack = true;
                return Direction.South;
            }
        }

        public static Direction GetOwnGoalDirection(Agent agent, int shortTime)
        {
            int desiredY = agent.StartGameComponent.Team == TeamId.Red ?
                Math.Min(agent.StartGameComponent.OwnGoalArea.Item1.Y, agent.StartGameComponent.OwnGoalArea.Item2.Y) :
                Math.Max(agent.StartGameComponent.OwnGoalArea.Item1.Y, agent.StartGameComponent.OwnGoalArea.Item2.Y);
            int minDesiredX = Math.Min(agent.StartGameComponent.OwnGoalArea.Item1.X, agent.StartGameComponent.OwnGoalArea.Item2.X);
            int maxDesiredX = Math.Max(agent.StartGameComponent.OwnGoalArea.Item1.X, agent.StartGameComponent.OwnGoalArea.Item2.X);
            if (desiredY > agent.BoardLogicComponent.Position.Y && CouldMove(agent, Direction.North, shortTime))
                return Direction.North;
            if (desiredY < agent.BoardLogicComponent.Position.Y && CouldMove(agent, Direction.South, shortTime))
               return Direction.South;
            if (minDesiredX > agent.BoardLogicComponent.Position.X && CouldMove(agent, Direction.East, shortTime))
                return Direction.East;
            if (maxDesiredX < agent.BoardLogicComponent.Position.X && CouldMove(agent, Direction.West, shortTime))
               return Direction.West;
            if (CouldMove(agent, Direction.East, shortTime))
                return Direction.East;
            if (CouldMove(agent, Direction.West, shortTime))
                return Direction.West;
            if (desiredY > agent.BoardLogicComponent.Position.Y)
                return Direction.North;
            if (desiredY < agent.BoardLogicComponent.Position.Y)
                return Direction.South;
            return GetGoalDirection(agent, shortTime, out _);
        }

        public static Direction GetRandomDirection()
        {
            long number = DateTime.Now.Ticks % 4;
            switch (number)
            {
                case 0:
                    return Direction.North;
                case 1:
                    return Direction.East;
                case 2:
                    return Direction.South;
                case 3:
                    return Direction.West;
            }
            return Direction.North;
        }

        // deprecated, do not use
        public static Direction StayInGoalArea(Agent agent, int shortTime, int stayInLineCount)
        {
            if (stayInLineCount > agent.BoardLogicComponent.BoardSize.X) return GetGoalDirection(agent, shortTime, out _);
            if (CouldMove(agent, Direction.East, shortTime)) return Direction.East;
            if (CouldMove(agent, Direction.West, shortTime)) return Direction.West;
            return GetGoalDirection(agent, shortTime, out _);
        }

        public static Direction StayInRectangle(Agent agent, int shortTime, int stayInLineCount, Direction directionEastWest, out bool shouldComeBack)
        {
            if (stayInLineCount >= Math.Abs(agent.StartGameComponent.OwnGoalArea.Item2.X - agent.StartGameComponent.OwnGoalArea.Item1.X))
            {
                Direction direction = GetGoalDirection(agent, shortTime, out bool should);
                if (!should && InRectangle(GetFieldInDirection(agent.BoardLogicComponent.Position, direction), agent.StartGameComponent.OwnGoalArea))
                {
                    shouldComeBack = false;
                    return direction;
                }
            }
            List<Direction> directionsToCheck = directionEastWest == Direction.East ?
                new List<Direction>() { Direction.East, Direction.West } :
                new List<Direction>() { Direction.West, Direction.East };
            foreach (var direction in directionsToCheck)
            {
                if (InRectangle(GetFieldInDirection(agent.BoardLogicComponent.Position, direction), agent.StartGameComponent.OwnGoalArea) && CouldMove(agent, direction, shortTime))
                {
                    shouldComeBack = false;
                    return direction;
                }
            }
            return GetGoalDirection(agent, shortTime, out shouldComeBack);
        }

        public static bool IsBack(Agent agent)
        {
            return agent.StartGameComponent.Team == TeamId.Red?
                agent.BoardLogicComponent.Position.Y == Math.Min(agent.StartGameComponent.OwnGoalArea.Item1.Y, agent.StartGameComponent.OwnGoalArea.Item2.Y) :
                agent.BoardLogicComponent.Position.Y == Math.Max(agent.StartGameComponent.OwnGoalArea.Item1.Y, agent.StartGameComponent.OwnGoalArea.Item2.Y);
        }

        public static bool IsDirectionGoalDirection(Direction direction)
        {
            return direction == Direction.North || direction == Direction.South;
        }

        public static bool DoesAgentKnowGoalInfo(Agent agent)
        {
            return agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].goalInfo != GoalInformation.NoInformation;
        }

        public static int FindClosest(Agent agent, int shortTime, out Direction direction)
        {
            int shortest = int.MaxValue;
            direction = Direction.North;
            for (int i = agent.BoardLogicComponent.Position.X - 1; i <= agent.BoardLogicComponent.Position.X + 1; i++)
                for (int j = agent.BoardLogicComponent.Position.Y - 1; j <= agent.BoardLogicComponent.Position.Y + 1; j++)
                    if ((i != agent.BoardLogicComponent.Position.X || j != agent.BoardLogicComponent.Position.Y) &&
                        OnBoard(new Point(i, j), agent.BoardLogicComponent.BoardSize) &&
                        !OldTime(agent.BoardLogicComponent.Board[j, i].distLearned, shortTime, agent.StartGameComponent.AverageTime) &&
                        agent.BoardLogicComponent.Board[j, i].distToPiece < Math.Min(shortest, agent.BoardLogicComponent.Board[agent.BoardLogicComponent.Position.Y, agent.BoardLogicComponent.Position.X].distToPiece))
                    {
                        shortest = agent.BoardLogicComponent.Board[j, i].distToPiece;
                        if (j > agent.BoardLogicComponent.Position.Y) direction = Direction.North;
                        else if (j < agent.BoardLogicComponent.Position.Y) direction = Direction.South;
                        else if (i < agent.BoardLogicComponent.Position.X) direction = Direction.West;
                        else direction = Direction.East;
                    }
            return shortest;
        }

        public static int CountUndiscoveredFields(Agent agent, int shortTime)
        {
            int count = 0;
            for (int i = agent.BoardLogicComponent.Position.X - 1; i <= agent.BoardLogicComponent.Position.X + 1; i++)
                for (int j = agent.BoardLogicComponent.Position.Y - 1; j <= agent.BoardLogicComponent.Position.Y + 1; j++)
                    if (OnBoard(new Point(i, j), agent.BoardLogicComponent.BoardSize) &&
                        OldTime(agent.BoardLogicComponent.Board[j, i].distLearned, shortTime, agent.StartGameComponent.AverageTime))
                        count++;
            return count;
        }

        public static bool OldTime(DateTime time, int multiply, TimeSpan compare)
        {
            TimeSpan passed = DateTime.Now - time;
            return TimeSpan.FromTicks(passed.Ticks * multiply) > compare;
        }
    }
}
