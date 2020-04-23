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

        public int[] TeamMates { get; private set; }

        public Dictionary<ActionType, TimeSpan> Penalties { get; private set; }

        public TimeSpan AverageTime { get; private set; }

        public float ShamPieceProbability { get; private set; }

        public TeamId Team { get; private set; }

        public bool IsLeader { get; private set; }

        public StartGameComponent(Agent agent, TeamId teamId)
        {
            this.agent = agent;
            Team = teamId;
            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public void Initialize(StartGamePayload startGamePayload)
        {
            IsLeader = agent.id == startGamePayload.LeaderId ? true : false;
            Team = startGamePayload.TeamId;
            TeamMates = startGamePayload.AlliesIds;
            Penalties = startGamePayload.Penalties;
            AverageTime = startGamePayload.Penalties.Count > 0 ? startGamePayload.Penalties.Values.Max() : TimeSpan.FromSeconds(1.0);
            ShamPieceProbability = startGamePayload.ShamPieceProbability;
            logger.Info("Initialize: Agent initialized" + " AgentID: " + agent.id.ToString());
            agent.BoardLogicComponent = new BoardLogicComponent(agent, startGamePayload.BoardSize, startGamePayload.GoalAreaHeight, startGamePayload.Position);
            agent.ProcessMessages = new ProcessMessages(agent);
            int closestHigherId = 0, minId = 0;
            int minDist = int.MaxValue;
            for (int i = 0; i < TeamMates.Length; i++)
            {
                if (TeamMates[i] < TeamMates[minId])
                {
                    minId = i;
                }
                if (TeamMates[i] - agent.id > 0 && TeamMates[i] - agent.id < minDist)
                {
                    minDist = TeamMates[i] - agent.id;
                    closestHigherId = i;
                }
            }
            agent.AgentInformationsComponent.LastAskedTeammate = minDist == int.MaxValue ? minId : closestHigherId;
        }
    }
}
