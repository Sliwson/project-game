﻿using Messaging.Contracts.GameMaster;
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

        public int[] TeamMatesToAsk { get; set; }

        public (Point, Point) OwnGoalArea { get; set; }

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
            AssignToOwnGoalArea(startGamePayload);
            FindFirstTeamMateToAsk();
        }

        private void AssignToOwnGoalArea(StartGamePayload startGamePayload)
        {
            if (!agent.DivideAgents)
            {
                TeamMatesToAsk = new int[TeamMates.Length];
                for (int i = 0; i < TeamMates.Length; i++)
                    TeamMatesToAsk[i] = TeamMates[i];
                OwnGoalArea = Team == TeamId.Red ?
                        (new Point(0, startGamePayload.BoardSize.Y - startGamePayload.GoalAreaHeight), new Point(startGamePayload.BoardSize.X - 1, startGamePayload.BoardSize.Y - 1)) :
                        (new Point(0, startGamePayload.GoalAreaHeight - 1), new Point(startGamePayload.BoardSize.X - 1, 0));
            }
            else
            {
                int[] allIds = new int[startGamePayload.AlliesIds.Length + 1];
                for (int i = 0; i < startGamePayload.AlliesIds.Length; i++)
                    allIds[i] = startGamePayload.AlliesIds[i];
                allIds[allIds.Length - 1] = agent.id;
                Array.Sort(allIds);
                int ownId = Array.IndexOf(allIds, agent.id);
                if (startGamePayload.BoardSize.X >= startGamePayload.GoalAreaHeight)
                {
                    int groupSize = 2;
                    int numberOfGroups = allIds.Length / groupSize;
                    int widthOnBoard = startGamePayload.BoardSize.X / numberOfGroups;
                    int groupId = ownId * numberOfGroups / allIds.Length;
                    int beginBoard = groupId * widthOnBoard, endBoard = Math.Min((groupId + 1) * widthOnBoard, startGamePayload.BoardSize.X) - 1;
                    int beginMates = groupId * groupSize, endMates = Math.Min((groupId + 1) * groupSize, allIds.Length) - 1;
                    OwnGoalArea = Team == TeamId.Red ?
                        (new Point(beginBoard, startGamePayload.BoardSize.Y - startGamePayload.GoalAreaHeight), new Point(endBoard, startGamePayload.BoardSize.Y - 1)) :
                        (new Point(beginBoard, startGamePayload.GoalAreaHeight - 1), new Point(endBoard, 0));
                    TeamMatesToAsk = new int[endMates - beginMates];
                    int mate = 0;
                    for (int i = beginMates; i <= endMates; i++)
                    {
                        if (allIds[i] == agent.id) continue;
                        TeamMatesToAsk[mate] = allIds[i];
                        mate++;
                    }
                    
                }
                else
                {
                    int groupSize = 2;
                    int numberOfGroups = allIds.Length / groupSize;
                    int heighthOnBoard = startGamePayload.GoalAreaHeight / numberOfGroups;
                    int groupId = ownId * numberOfGroups / allIds.Length;
                    int beginBoard = groupId * heighthOnBoard, endBoard = Math.Min((groupId + 1) * heighthOnBoard, startGamePayload.GoalAreaHeight) - 1;
                    int beginMates = groupId * groupSize, endMates = Math.Min((groupId + 1) * groupSize, allIds.Length) - 1;
                    OwnGoalArea = Team == TeamId.Red ?
                        (new Point(0, beginBoard), new Point(startGamePayload.BoardSize.X - 1, endBoard)) :
                        (new Point(0, endBoard), new Point(startGamePayload.BoardSize.X - 1, beginBoard));
                    TeamMatesToAsk = new int[endMates - beginMates];
                    int mate = 0;
                    for (int i = beginMates; i <= endMates; i++)
                    {
                        if (allIds[i] == agent.id) continue;
                        TeamMatesToAsk[mate] = allIds[i];
                        mate++;
                    }
                }
            }
        }
        
        private void FindFirstTeamMateToAsk()
        {
            int closestHigherId = 0, minId = 0;
            int minDist = int.MaxValue;
            for (int i = 0; i < TeamMatesToAsk.Length; i++)
            {
                if (TeamMatesToAsk[i] < TeamMatesToAsk[minId])
                {
                    minId = i;
                }
                if (TeamMatesToAsk[i] - agent.id > 0 && TeamMatesToAsk[i] - agent.id < minDist)
                {
                    minDist = TeamMatesToAsk[i] - agent.id;
                    closestHigherId = i;
                }
            }
            agent.AgentInformationsComponent.LastAskedTeammate = minDist == int.MaxValue ? minId : closestHigherId;
        }
    }
}
