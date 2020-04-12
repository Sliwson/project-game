using Messaging.Contracts.GameMaster;
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

        public void Initialize(StartGamePayload startGamePayload)
        {
            agent.isLeader = agent.id == startGamePayload.LeaderId ? true : false;
            agent.team = startGamePayload.TeamId;
            agent.position = startGamePayload.Position;
            agent.teamMates = new int[startGamePayload.AlliesIds.Length];
            agent.teamMates = startGamePayload.AlliesIds;
            agent.penalties = startGamePayload.Penalties;
            agent.averageTime = startGamePayload.Penalties.Count > 0 ? (int)startGamePayload.Penalties.Values.Max().TotalMilliseconds : 500;
            agent.shamPieceProbability = startGamePayload.ShamPieceProbability;
            logger.Info("Initialize: Agent initialized" + " AgentID: " + agent.id.ToString());
            agent.boardLogicComponent = new BoardLogicComponent(agent, startGamePayload.BoardSize, startGamePayload.GoalAreaHeight);
            agent.processMessages = new ProcessMessages(agent);
        }
    }
}
