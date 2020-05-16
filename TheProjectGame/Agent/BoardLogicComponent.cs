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

        public Field[,] Board { get; private set; }

        public Point BoardSize { get; private set; }

        public int GoalAreaSize { get; private set; }

        public Point Position { get; set; }

        public BoardLogicComponent(Agent agent, Point boardSize, int goalAreaHeight, Point position)
        {
            this.agent = agent;
            BoardSize = boardSize;
            Position = position;
            Board = new Field[BoardSize.Y, BoardSize.X];
            for (int i = 0; i < BoardSize.Y; i++)
            {
                for (int j = 0; j < BoardSize.X; j++)
                {
                    Board[i, j] = new Field();
                }
            }
            GoalAreaSize = goalAreaHeight;
        }

        public int[,] GetDistances()
        {
            int[,] distances = new int[BoardSize.Y, BoardSize.X];
            for (int i = 0; i < BoardSize.Y; i++)
            {
                for (int j = 0; j < BoardSize.X; j++)
                {
                    distances[i, j] = Board[i, j].distToPiece;
                }
            }
            return distances;
        }

        public GoalInformation[,] GetBlueTeamGoalAreaInformation()
        {
            GoalInformation[,] goalAreaInformation = new GoalInformation[GoalAreaSize, BoardSize.X];
            for (int i = 0; i < GoalAreaSize; i++)
            {
                for (int j = 0; j < BoardSize.X; j++)
                {
                    goalAreaInformation[i, j] = Board[i, j].goalInfo;
                }
            }
            return goalAreaInformation;
        }

        public GoalInformation[,] GetRedTeamGoalAreaInformation()
        {
            GoalInformation[,] goalAreaInformation = new GoalInformation[GoalAreaSize, BoardSize.X];
            for (int i = BoardSize.Y - GoalAreaSize + 1; i < BoardSize.Y; i++)
            {
                for (int j = 0; j < BoardSize.X; j++)
                {
                    goalAreaInformation[i - BoardSize.Y + GoalAreaSize, j] = Board[i, j].goalInfo;
                }
            }
            return goalAreaInformation;
        }

        public void UpdateDistances(int[,] distances)
        {
            for (int i = 0; i < BoardSize.Y; i++)
            {
                for (int j = 0; j < BoardSize.X; j++)
                {
                    if (Board[i, j].distToPiece == int.MaxValue)
                        Board[i, j].distToPiece = distances[i, j];
                }
            }
        }

        public void UpdateBlueTeamGoalAreaInformation(GoalInformation[,] goalAreaInformation)
        {
            for (int i = 0; i < GoalAreaSize; i++)
            {
                for (int j = 0; j < BoardSize.X; j++)
                {
                    if (Board[i, j].goalInfo == GoalInformation.NoInformation) Board[i, j].goalInfo = goalAreaInformation[i, j];
                }
            }
        }

        public void UpdateRedTeamGoalAreaInformation(GoalInformation[,] goalAreaInformation)
        {
            for (int i = BoardSize.Y - GoalAreaSize + 1; i < BoardSize.Y; i++)
            {
                for (int j = 0; j < BoardSize.X; j++)
                {
                    Board[i, j].goalInfo = goalAreaInformation[i - BoardSize.Y + GoalAreaSize, j];
                }
            }
        }
    }
}
