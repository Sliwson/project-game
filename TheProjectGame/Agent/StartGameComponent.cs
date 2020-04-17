using Messaging.Contracts.GameMaster;
using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Agent
{
    public class StartGameComponent
    {
        private Agent agent;

        private static NLog.Logger logger;
        public int[] teamMates { get; private set; }

        public Dictionary<ActionType, TimeSpan> penalties { get; private set; }

        public int averageTime { get; private set; }

        public float shamPieceProbability { get; private set; }

        public TeamId team { get; private set; }

        public bool isLeader { get; private set; }

        public StartGameComponent(Agent agent, TeamId teamId)
        {
            this.agent = agent;
            team = teamId;
            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public void Initialize(StartGamePayload startGamePayload)
        {
            isLeader = agent.id == startGamePayload.LeaderId ? true : false;
            team = startGamePayload.TeamId;
            teamMates = new int[startGamePayload.AlliesIds.Length];
            teamMates = startGamePayload.AlliesIds;
            penalties = startGamePayload.Penalties;
            averageTime = startGamePayload.Penalties.Count > 0 ? (int)startGamePayload.Penalties.Values.Max().TotalMilliseconds : 500;
            shamPieceProbability = startGamePayload.ShamPieceProbability;
            logger.Info("Initialize: Agent initialized" + " AgentID: " + agent.id.ToString());
            agent.BoardLogicComponent = new BoardLogicComponent(agent, startGamePayload.BoardSize, startGamePayload.GoalAreaHeight, startGamePayload.Position);
            agent.ProcessMessages = new ProcessMessages(agent);
        }
    }
}
