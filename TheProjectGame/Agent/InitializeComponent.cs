using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Agent
{
    public class InitializeComponent
    {
        private Agent agent;
        private static NLog.Logger logger;

        public InitializeComponent(Agent agent)
        {
            this.agent = agent;
            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public void Initialize(int leaderId, TeamId teamId, Point boardSize, int goalAreaHeight, Point pos, int[] alliesIds, Dictionary<ActionType, TimeSpan> penalties, float shamPieceProbability)
        {
            agent.isLeader = agent.id == leaderId ? true : false;
            agent.team = teamId;
            agent.boardSize = boardSize;
            agent.board = new Field[boardSize.Y, boardSize.X];
            for (int i = 0; i < boardSize.Y; i++)
            {
                for (int j = 0; j < boardSize.X; j++)
                {
                    agent.board[i, j] = new Field();
                }
            }
            agent.position = pos;
            agent.teamMates = new int[alliesIds.Length];
            agent.teamMates = alliesIds;
            agent.goalAreaSize = goalAreaHeight;
            agent.penalties = penalties;
            agent.averageTime = penalties.Count > 0 ? (int)penalties.Values.Max().TotalMilliseconds : 500;
            agent.shamPieceProbability = shamPieceProbability;
            logger.Info("Initialize: Agent initialized" + " AgentID: " + agent.id.ToString());
            agent.boardLogicComponent = new BoardLogicComponent(agent);
            agent.processMessages = new ProcessMessages(agent);
        }
    }
}
