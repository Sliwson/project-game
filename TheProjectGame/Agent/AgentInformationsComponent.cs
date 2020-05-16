using Messaging.Contracts;
using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Agent
{
    public class AgentInformationsComponent
    {
        private Agent agent;

        public int LastAskedTeammate;
        public Direction LastDirection { get; set; }
        public BaseMessage LastRequest { get; set; }
        public double LastRequestPenalty { get; set; }
        public bool DeniedLastMove { get; set; }
        public bool DeniedLastRequest { get; set; }
        public bool Discovered { get; set; }
        public bool IsComingBack { get; set; }
        public int DidNotAskCount { get; set; }
        public bool IsWaiting { get; set; }
        public Direction DirectionEastWest { get; set; }
        public int StayInLineCount { get; set; }
        public TimeSpan SkipTime { get; set; }
        public double RemainingPenalty { get; set; }
        public int[] TeamMatesToAsk { get; set; }
        public (Point, Point) OwnGoalArea { get; set; }
        public bool[] DeniedMoves { get; set; }
        private int LastMoveResponse { get; set; }

        public AgentInformationsComponent(Agent agent)
        {
            this.agent = agent;
            LastAskedTeammate = 0;
            LastRequestPenalty = 0.0;
            DeniedLastMove = false;
            DeniedLastRequest = false;
            Discovered = false;
            IsComingBack = false;
            DidNotAskCount = 0;
            IsWaiting = false;
            DirectionEastWest = Direction.East;
            StayInLineCount = 0;
            SkipTime = TimeSpan.Zero;
            RemainingPenalty = 0.0;
            DeniedMoves = new bool[agent.MoveResponsesCount];
            for (int i = 0; i < DeniedMoves.Length; i++)
                DeniedMoves[i] = false;
            LastMoveResponse = 0;
        }

        public void AssignToWholeTaskArea()
        {
            TeamMatesToAsk = new int[agent.StartGameComponent.TeamMates.Length];
            for (int i = 0; i < agent.StartGameComponent.TeamMates.Length; i++)
                TeamMatesToAsk[i] = agent.StartGameComponent.TeamMates[i];
            OwnGoalArea = agent.StartGameComponent.Team == TeamId.Red ?
                    (new Point(0, agent.BoardLogicComponent.BoardSize.Y - agent.BoardLogicComponent.GoalAreaSize), new Point(agent.BoardLogicComponent.BoardSize.X - 1, agent.BoardLogicComponent.BoardSize.Y - 1)) :
                    (new Point(0, agent.BoardLogicComponent.GoalAreaSize - 1), new Point(agent.BoardLogicComponent.BoardSize.X - 1, 0));
            LastAskedTeammate = 0;
        }

        public void DeniedMove(bool denied)
        {
            DeniedLastMove = denied;
            DeniedMoves[LastMoveResponse] = denied;
            LastMoveResponse = (LastMoveResponse + 1) % DeniedMoves.Length;
        }
    }
}
