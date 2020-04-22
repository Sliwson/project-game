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

        public TimeSpan averageTime { get; private set; }

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
            averageTime = startGamePayload.Penalties.Count > 0 ? startGamePayload.Penalties.Values.Max() : TimeSpan.FromSeconds(1.0);
            shamPieceProbability = startGamePayload.ShamPieceProbability;
            logger.Info("Initialize: Agent initialized" + " AgentID: " + agent.id.ToString());
            agent.BoardLogicComponent = new BoardLogicComponent(agent, startGamePayload.BoardSize, startGamePayload.GoalAreaHeight, startGamePayload.Position);
            agent.ProcessMessages = new ProcessMessages(agent);
            int closestHigherId = 0, minId = 0;
            int minDist = int.MaxValue;
            for (int i = 0; i < teamMates.Length; i++)
            {
                if (teamMates[i] < teamMates[minId])
                {
                    minId = i;
                }
                if (teamMates[i] - agent.id > 0 && teamMates[i] - agent.id < minDist)
                {
                    minDist = teamMates[i] - agent.id;
                    closestHigherId = i;
                }
            }
            agent.AgentInformationsComponent.LastAskedTeammate = minDist == int.MaxValue ? minId : closestHigherId;
        }
    }
}
