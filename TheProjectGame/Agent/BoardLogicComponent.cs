using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agent
{
    public class BoardLogicComponent
    {
        private Agent agent;

        public BoardLogicComponent(Agent agent)
        {
            this.agent = agent;
        }

        public int[,] GetDistances()
        {
            int[,] distances = new int[agent.boardSize.Y, agent.boardSize.X];
            for (int i = 0; i < agent.boardSize.Y; i++)
            {
                for (int j = 0; j < agent.boardSize.X; j++)
                {
                    distances[i, j] = agent.board[i, j].distToPiece;
                }
            }
            return distances;
        }

        public GoalInformation[,] GetBlueTeamGoalAreaInformation()
        {
            GoalInformation[,] goalAreaInformation = new GoalInformation[agent.goalAreaSize, agent.boardSize.X];
            for (int i = 0; i < agent.goalAreaSize; i++)
            {
                for (int j = 0; j < agent.boardSize.X; j++)
                {
                    goalAreaInformation[i, j] = agent.board[i, j].goalInfo;
                }
            }
            return goalAreaInformation;
        }

        public GoalInformation[,] GetRedTeamGoalAreaInformation()
        {
            GoalInformation[,] goalAreaInformation = new GoalInformation[agent.goalAreaSize, agent.boardSize.X];
            for (int i = agent.boardSize.Y - agent.goalAreaSize + 1; i < agent.boardSize.Y; i++)
            {
                for (int j = 0; j < agent.boardSize.X; j++)
                {
                    goalAreaInformation[i - agent.boardSize.Y + agent.goalAreaSize, j] = agent.board[i, j].goalInfo;
                }
            }
            return goalAreaInformation;
        }

        public void UpdateDistances(int[,] distances)
        {
            //TODO: update only when distLearned old
            for (int i = 0; i < agent.boardSize.Y; i++)
            {
                for (int j = 0; j < agent.boardSize.X; j++)
                {
                    agent.board[i, j].distToPiece = distances[i, j];
                }
            }
        }

        public void UpdateBlueTeamGoalAreaInformation(GoalInformation[,] goalAreaInformation)
        {
            for (int i = 0; i < agent.goalAreaSize; i++)
            {
                for (int j = 0; j < agent.boardSize.X; j++)
                {
                    if (agent.board[i, j].goalInfo == GoalInformation.NoInformation) agent.board[i, j].goalInfo = goalAreaInformation[i, j];
                }
            }
        }

        public void UpdateRedTeamGoalAreaInformation(GoalInformation[,] goalAreaInformation)
        {
            for (int i = agent.boardSize.Y - agent.goalAreaSize + 1; i < agent.boardSize.Y; i++)
            {
                for (int j = 0; j < agent.boardSize.X; j++)
                {
                    agent.board[i, j].goalInfo = goalAreaInformation[i - agent.boardSize.Y + agent.goalAreaSize, j];
                }
            }
        }
    }
}
