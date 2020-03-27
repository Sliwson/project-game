using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster
{
    public class GameMasterConfiguration
    {
        public TimeSpan MovePenalty { get; set; }
        public TimeSpan AskPenalty { get; set; }
        public TimeSpan DiscoveryPenalty { get; set; }
        public TimeSpan PutPenalty { get; set; }
        public TimeSpan CheckForShamPenalty { get; set; }
        public TimeSpan ResponsePenalty { get; set; }
        public int BoardX { get; set; }
        public int BoardY { get; set; }
        public int GoalAreaHeight { get; set; }
        public int NumberOfGoals { get; set; }
        public int NumberOfPieces { get; set; }
        
        //TODO: check fields below in issues
        public TimeSpan DestroyPiecePenalty { get; set; }
        public TimeSpan GeneratePieceDelay { get; set; }
        public int AgentsLimit { get; set; }
        public float ShamProbability { get; set; }
        public int NumberOfFakeGoals { get; set; }
        // csIP
        // csPort

        private Dictionary<ActionType, TimeSpan> agentTimeouts = null;

        public Dictionary<ActionType, TimeSpan> GetTimeouts()
        {
            if (agentTimeouts != null)
                return agentTimeouts;

            agentTimeouts = new Dictionary<ActionType, TimeSpan>();
            agentTimeouts.Add(ActionType.CheckForSham, CheckForShamPenalty);
            agentTimeouts.Add(ActionType.DestroyPiece, DestroyPiecePenalty);
            agentTimeouts.Add(ActionType.Discovery, DiscoveryPenalty);
            agentTimeouts.Add(ActionType.InformationRequest, AskPenalty);
            agentTimeouts.Add(ActionType.InformationResponse, ResponsePenalty);
            agentTimeouts.Add(ActionType.Move, MovePenalty);
            agentTimeouts.Add(ActionType.PutPiece, PutPenalty);
            return agentTimeouts;
        }
    }
}
