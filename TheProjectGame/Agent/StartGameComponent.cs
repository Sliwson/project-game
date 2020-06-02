using Messaging.Contracts.GameMaster;
using Messaging.Enumerators;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Agent
{
    public class StartGameComponent
    {
        private Agent agent;

        private static Logger logger = LogManager.GetCurrentClassLogger();

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
        }

        public void Initialize(StartGamePayload startGamePayload)
        {
            IsLeader = agent.Id == startGamePayload.LeaderId ? true : false;
            Team = startGamePayload.TeamId;
            TeamMates = startGamePayload.AlliesIds;
            Penalties = startGamePayload.Penalties;
            AverageTime = startGamePayload.Penalties.Count > 0 ? startGamePayload.Penalties.Values.Max() : TimeSpan.FromSeconds(1.0);
            ShamPieceProbability = startGamePayload.ShamPieceProbability;
            agent.BoardLogicComponent = new BoardLogicComponent(agent, startGamePayload.BoardSize, startGamePayload.GoalAreaHeight, startGamePayload.Position);

            agent.AgentInformationsComponent.AssignToOwnGoalArea();
            FindFirstTeamMateToAsk();

            logger.Info("[Agent {id}] Initialized", agent.Id);
        }
        
        private void FindFirstTeamMateToAsk()
        {
            int closestHigherId = 0, minId = 0;
            int minDist = int.MaxValue;
            for (int i = 0; i < agent.AgentInformationsComponent.TeamMatesToAsk.Length; i++)
            {
                if (agent.AgentInformationsComponent.TeamMatesToAsk[i] < agent.AgentInformationsComponent.TeamMatesToAsk[minId])
                {
                    minId = i;
                }
                if (agent.AgentInformationsComponent.TeamMatesToAsk[i] - agent.Id > 0 && agent.AgentInformationsComponent.TeamMatesToAsk[i] - agent.Id < minDist)
                {
                    minDist = agent.AgentInformationsComponent.TeamMatesToAsk[i] - agent.Id;
                    closestHigherId = i;
                }
            }

            agent.AgentInformationsComponent.LastAskedTeammate = minDist == int.MaxValue ? minId : closestHigherId;
        }
    }
}
