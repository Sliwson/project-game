using System;
using System.Collections.Generic;
using System.Text;

namespace Agent.strategies
{
    class SimpleStrategy : IStrategy
    {
        private const int shortPieceDistance = 4;

        private const int shortTime = 4;

        private const int smallUndiscoveredNumber = 4;

        private int stayInLineCount = 0;

        private bool InGoalArea(Team team, (int, int) position, (int, int) boardSize, int goalAreaSize)
        {
            return team == Team.Red ? position.Item1 >= boardSize.Item1 + goalAreaSize : position.Item1 < goalAreaSize;
        }

        private bool OnBoard((int, int) position, (int, int) boardSize, int goalAreaSize)
        {
            return position.Item1 >= 0 && position.Item2 >= 0 && position.Item1 < boardSize.Item1 + 2 * goalAreaSize && position.Item2 < boardSize.Item2;
        }

        private bool CouldMove(Agent agent, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return OnBoard((agent.position.Item1, agent.position.Item2 + 1), agent.boardSize, agent.goalAreaSize) &&
                        DateTime.Now - agent.board[agent.position.Item1, agent.position.Item2 + 1].deniedMove > shortTime * TimeSpan.FromMilliseconds(agent.penaltyTime);
                case Direction.Right:
                    return OnBoard((agent.position.Item1 + 1, agent.position.Item2), agent.boardSize, agent.goalAreaSize) &&
                        DateTime.Now - agent.board[agent.position.Item1 + 1, agent.position.Item2].deniedMove > shortTime * TimeSpan.FromMilliseconds(agent.penaltyTime);
                case Direction.Down:
                    return OnBoard((agent.position.Item1, agent.position.Item2 - 1), agent.boardSize, agent.goalAreaSize) &&
                        DateTime.Now - agent.board[agent.position.Item1, agent.position.Item2 - 1].deniedMove > shortTime * TimeSpan.FromMilliseconds(agent.penaltyTime);
                case Direction.Left:
                    return OnBoard((agent.position.Item1 - 1, agent.position.Item2), agent.boardSize, agent.goalAreaSize) &&
                        DateTime.Now - agent.board[agent.position.Item1 - 1, agent.position.Item2].deniedMove > shortTime * TimeSpan.FromMilliseconds(agent.penaltyTime);
            }
            return false;
        }

        private Direction GetGoalDirection(Agent agent)
        {
            if (agent.team == Team.Red)
            {
                if (CouldMove(agent, Direction.Right)) return Direction.Right;
                if (CouldMove(agent, Direction.Up)) return Direction.Up;
                if (CouldMove(agent, Direction.Down)) return Direction.Down;
                return Direction.Right;
            }
            else
            {
                if (CouldMove(agent, Direction.Left)) return Direction.Left;
                if (CouldMove(agent, Direction.Up)) return Direction.Up;
                if (CouldMove(agent, Direction.Down)) return Direction.Down;
                return Direction.Left;
            }
        }

        private Direction StayInGoalArea(Agent agent)
        {
            Direction direction;
            if (stayInLineCount > agent.boardSize.Item2) direction = GetGoalDirection(agent);
            else if (CouldMove(agent, Direction.Up)) direction = Direction.Up;
            else if (CouldMove(agent, Direction.Down)) direction = Direction.Down;
            else direction = GetGoalDirection(agent);
            if (direction == Direction.Up || direction == Direction.Down) stayInLineCount++;
            else stayInLineCount = 0;
            return direction;
        }

        private int FindClosest(Agent agent, out Direction direction)
        {
            int shortest = int.MaxValue;
            direction = Direction.Up;
            for (int i = agent.position.Item1 - 1; i <= agent.position.Item1 + 1; i++)
                for (int j = agent.position.Item2 - 1; j <= agent.position.Item2 + 1; j++)
                    if ((i != agent.position.Item1 || j != agent.position.Item2) &&
                        OnBoard((i, j), agent.boardSize, agent.goalAreaSize) &&
                        DateTime.Now - agent.board[i, j].distLearned > TimeSpan.FromMilliseconds(shortTime * agent.penaltyTime) &&
                        agent.board[i, j].distToPiece < Math.Min(shortest, agent.board[agent.position.Item1, agent.position.Item2].distToPiece))
                    {
                        shortest = agent.board[i, j].distToPiece;
                        if (j > agent.position.Item2) direction = Direction.Up;
                        else if (j < agent.position.Item2) direction = Direction.Down;
                        else if (i < agent.position.Item1) direction = Direction.Left;
                        else direction = Direction.Right;
                    }
            return shortest;
        }

        private int CountUndiscoveredFields(Agent agent)
        {
            int count = 0;
            for (int i = agent.position.Item1 - 1; i <= agent.position.Item1 + 1; i++)
                for (int j = agent.position.Item2 - 1; j <= agent.position.Item2 + 1; j++)
                    if (OnBoard((i, j), agent.boardSize, agent.goalAreaSize) &&
                        DateTime.Now - agent.board[i, j].distLearned > TimeSpan.FromMilliseconds(shortTime * agent.penaltyTime))
                        count++;
            return count;
        }

        public void MakeDecision(Agent agent)
        {
            if (!InGoalArea(agent.team, agent.position, agent.boardSize, agent.goalAreaSize)) stayInLineCount = 0;
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
                agent.board[agent.position.Item1, agent.position.Item2].goalInfo == GoalInfo.IDK &&
                InGoalArea(agent.team, agent.position, agent.boardSize, agent.goalAreaSize))
            {
                agent.Put();
                stayInLineCount = 0;
                return;
            }
            if (agent.piece != null && agent.piece.isDiscovered &&
                InGoalArea(agent.team, agent.position, agent.boardSize, agent.goalAreaSize))
            {
                agent.Move(StayInGoalArea(agent));
                return;
            }
            if (agent.piece != null && agent.piece.isDiscovered)
            {
                agent.Move(GetGoalDirection(agent));
                return;
            }
            if (FindClosest(agent, out Direction direction) <= shortPieceDistance)
            {
                agent.Move(direction);
                return;
            }
            if (CountUndiscoveredFields(agent) > smallUndiscoveredNumber)
            {
                agent.Discover();
                return;
            }
            agent.BegForInfo();
        }
    }
}
