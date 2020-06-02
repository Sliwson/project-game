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

            AssignToOwnGoalArea(startGamePayload);
            FindFirstTeamMateToAsk();

            logger.Info("[Agent {id}] Initialized", agent.Id);
        }

        private void AssignToOwnGoalArea(StartGamePayload startGamePayload)
        {
            if (!agent.DivideAgents || TeamMates.Length == 0 || Math.Max(startGamePayload.BoardSize.X, startGamePayload.GoalAreaHeight) <= 0)
            {
                agent.AgentInformationsComponent.TeamMatesToAsk = new int[TeamMates.Length];
                for (int i = 0; i < TeamMates.Length; i++)
                    agent.AgentInformationsComponent.TeamMatesToAsk[i] = TeamMates[i];
                agent.AgentInformationsComponent.OwnGoalArea = Team == TeamId.Red ?
                        (new Point(0, startGamePayload.BoardSize.Y - startGamePayload.GoalAreaHeight), new Point(startGamePayload.BoardSize.X - 1, startGamePayload.BoardSize.Y - 1)) :
                        (new Point(0, startGamePayload.GoalAreaHeight - 1), new Point(startGamePayload.BoardSize.X - 1, 0));
            }
            else
            {
                int[] allIds = new int[startGamePayload.AlliesIds.Length + 1];
                for (int i = 0; i < startGamePayload.AlliesIds.Length; i++)
                    allIds[i] = startGamePayload.AlliesIds[i];
                allIds[allIds.Length - 1] = agent.Id;
                Array.Sort(allIds);
                int ownId = Array.IndexOf(allIds, agent.Id);
                bool divideWidth = startGamePayload.BoardSize.X >= startGamePayload.GoalAreaHeight;
                int lengthToDivide = divideWidth ? startGamePayload.BoardSize.X : startGamePayload.GoalAreaHeight;
                int groupSize = Math.Max(1, allIds.Length / lengthToDivide);
                int numberOfGroups = allIds.Length / groupSize;
                int lengthOnBoard = lengthToDivide / numberOfGroups;
                int groupId = Math.Min(ownId / groupSize, numberOfGroups - 1);
                int biggerBoard = groupId == numberOfGroups - 1 ?
                    lengthToDivide - numberOfGroups * lengthOnBoard : 0;
                int biggerMates = groupId == numberOfGroups - 1 ?
                    allIds.Length - numberOfGroups * groupSize : 0;
                int beginBoard = groupId * lengthOnBoard, endBoard = Math.Min((groupId + 1) * lengthOnBoard + biggerBoard, lengthToDivide) - 1;
                int beginMates = groupId * groupSize, endMates = Math.Min((groupId + 1) * groupSize + biggerMates, allIds.Length) - 1;
                agent.AgentInformationsComponent.OwnGoalArea = divideWidth ?
                    (Team == TeamId.Red ?
                        (new Point(beginBoard, startGamePayload.BoardSize.Y - startGamePayload.GoalAreaHeight), new Point(endBoard, startGamePayload.BoardSize.Y - 1)) :
                        (new Point(beginBoard, startGamePayload.GoalAreaHeight - 1), new Point(endBoard, 0))) :
                    (Team == TeamId.Red ?
                        (new Point(0, beginBoard), new Point(startGamePayload.BoardSize.X - 1, endBoard)) :
                        (new Point(0, endBoard), new Point(startGamePayload.BoardSize.X - 1, beginBoard)));
                agent.AgentInformationsComponent.TeamMatesToAsk = new int[endMates - beginMates];
                int mate = 0;
                for (int i = beginMates; i <= endMates; i++)
                {
                    if (allIds[i] == agent.Id) continue;
                    agent.AgentInformationsComponent.TeamMatesToAsk[mate] = allIds[i];
                    mate++;
                }
            }
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
