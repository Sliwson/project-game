using Messaging.Contracts;
using Messaging.Enumerators;
using System;
using System.Drawing;

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

        private void AssignToWholeTaskArea()
        {
            TeamMatesToAsk = new int[agent.StartGameComponent.TeamMates.Length];
            for (int i = 0; i < agent.StartGameComponent.TeamMates.Length; i++)
                TeamMatesToAsk[i] = agent.StartGameComponent.TeamMates[i];
            OwnGoalArea = agent.StartGameComponent.Team == TeamId.Red ?
                    (new Point(0, agent.BoardLogicComponent.BoardSize.Y - agent.BoardLogicComponent.GoalAreaSize), new Point(agent.BoardLogicComponent.BoardSize.X - 1, agent.BoardLogicComponent.BoardSize.Y - 1)) :
                    (new Point(0, agent.BoardLogicComponent.GoalAreaSize - 1), new Point(agent.BoardLogicComponent.BoardSize.X - 1, 0));
            LastAskedTeammate = 0;
        }

        public void UpdateAssignment()
        {
            int rowDirection, firstRow, lastRow, lastRowPlusOne;
            if (agent.StartGameComponent.Team == TeamId.Red)
            {
                rowDirection = 1;
                firstRow = Math.Min(OwnGoalArea.Item1.Y, OwnGoalArea.Item2.Y);
                lastRow = Math.Max(OwnGoalArea.Item1.Y, OwnGoalArea.Item2.Y);
                lastRowPlusOne = lastRow + 1;
            }
            else
            {
                rowDirection = -1;
                firstRow = Math.Max(OwnGoalArea.Item1.Y, OwnGoalArea.Item2.Y);
                lastRow = Math.Min(OwnGoalArea.Item1.Y, OwnGoalArea.Item2.Y);
                lastRowPlusOne = lastRow - 1;
            }
            int firstColumn = Math.Min(OwnGoalArea.Item1.X, OwnGoalArea.Item2.X);
            int lastColumn = Math.Max(OwnGoalArea.Item1.X, OwnGoalArea.Item2.X);
            int rowToCheck = firstRow;
            while (rowToCheck != lastRowPlusOne)
            {
                bool knowsAll = true;
                for (int columnToCheck = firstColumn; columnToCheck <= lastColumn; columnToCheck++)
                {
                    if (agent.BoardLogicComponent.Board[rowToCheck, columnToCheck].goalInfo == GoalInformation.NoInformation)
                    {
                        knowsAll = false;
                        break;
                    }
                }
                if (knowsAll)
                {
                    firstRow += rowDirection;
                }
                rowToCheck += rowDirection;
            }
            if (firstRow == lastRowPlusOne)
            {
                AssignToWholeTaskArea();
            }
            else
            {
                OwnGoalArea = (new Point(firstColumn, firstRow), new Point(lastColumn, lastRow));
            }
        }

        public void DeniedMove(bool denied)
        {
            DeniedLastMove = denied;
            DeniedMoves[LastMoveResponse] = denied;
            LastMoveResponse = (LastMoveResponse + 1) % DeniedMoves.Length;
        }
    }
}
