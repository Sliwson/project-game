using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Agent
{
    public class BoardLogicComponent
    {
        private Agent agent;

        public Field[,] board { get; private set; }

        public Point boardSize { get; private set; }

        public int goalAreaSize { get; private set; }

        public BoardLogicComponent(Agent agent, Point BoardSize, int GoalAreaHeight)
        {
            this.agent = agent;
            boardSize = BoardSize;
            board = new Field[BoardSize.Y, BoardSize.X];
            for (int i = 0; i < BoardSize.Y; i++)
            {
                for (int j = 0; j < BoardSize.X; j++)
                {
                    board[i, j] = new Field();
                }
            }
            goalAreaSize = GoalAreaHeight;
        }

        public int[,] GetDistances()
        {
            int[,] distances = new int[boardSize.Y, boardSize.X];
            for (int i = 0; i < boardSize.Y; i++)
            {
                for (int j = 0; j < boardSize.X; j++)
                {
                    distances[i, j] = board[i, j].distToPiece;
                }
            }
            return distances;
        }

        public GoalInformation[,] GetBlueTeamGoalAreaInformation()
        {
            GoalInformation[,] goalAreaInformation = new GoalInformation[goalAreaSize, boardSize.X];
            for (int i = 0; i < goalAreaSize; i++)
            {
                for (int j = 0; j < boardSize.X; j++)
                {
                    goalAreaInformation[i, j] = board[i, j].goalInfo;
                }
            }
            return goalAreaInformation;
        }

        public GoalInformation[,] GetRedTeamGoalAreaInformation()
        {
            GoalInformation[,] goalAreaInformation = new GoalInformation[goalAreaSize, boardSize.X];
            for (int i = boardSize.Y - goalAreaSize + 1; i < boardSize.Y; i++)
            {
                for (int j = 0; j < boardSize.X; j++)
                {
                    goalAreaInformation[i - boardSize.Y + goalAreaSize, j] = board[i, j].goalInfo;
                }
            }
            return goalAreaInformation;
        }

        public void UpdateDistances(int[,] distances)
        {
            //TODO: update only when distLearned old
            for (int i = 0; i < boardSize.Y; i++)
            {
                for (int j = 0; j < boardSize.X; j++)
                {
                    board[i, j].distToPiece = distances[i, j];
                }
            }
        }

        public void UpdateBlueTeamGoalAreaInformation(GoalInformation[,] goalAreaInformation)
        {
            for (int i = 0; i < goalAreaSize; i++)
            {
                for (int j = 0; j < boardSize.X; j++)
                {
                    if (board[i, j].goalInfo == GoalInformation.NoInformation) board[i, j].goalInfo = goalAreaInformation[i, j];
                }
            }
        }

        public void UpdateRedTeamGoalAreaInformation(GoalInformation[,] goalAreaInformation)
        {
            for (int i = boardSize.Y - goalAreaSize + 1; i < boardSize.Y; i++)
            {
                for (int j = 0; j < boardSize.X; j++)
                {
                    board[i, j].goalInfo = goalAreaInformation[i - boardSize.Y + goalAreaSize, j];
                }
            }
        }
    }
}
