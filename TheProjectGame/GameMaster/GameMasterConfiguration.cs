using Messaging.Enumerators;
using System;
using System.Collections.Generic;

namespace GameMaster
{
    public class GameMasterConfiguration
    {
        //board

        public int BoardX { get; set; }
        public int BoardY { get; set; }
        public int GoalAreaHeight { get; set; }
        public int NumberOfGoals { get; set; }

        //game

        public int TeamSize { get; set; }
        public int NumberOfPieces { get; set; }
        public float ShamProbability { get; set; }

        //network

        public string CsIP { get; set; }
        public int CsPort { get; set; }

        //penalties

        public TimeSpan MovePenalty { get; set; }
        public TimeSpan InformationExchangePenalty { get; set; }
        public TimeSpan DiscoveryPenalty { get; set; }
        public TimeSpan PutPenalty { get; set; }
        public TimeSpan CheckForShamPenalty { get; set; }
        public TimeSpan DestroyPiecePenalty { get; set; }

        private Dictionary<ActionType, TimeSpan> agentTimeouts = null;

        public Dictionary<ActionType, TimeSpan> GetTimeouts()
        {
            if (agentTimeouts != null)
                return agentTimeouts;

            agentTimeouts = new Dictionary<ActionType, TimeSpan>();
            agentTimeouts.Add(ActionType.CheckForSham, CheckForShamPenalty);
            agentTimeouts.Add(ActionType.DestroyPiece, DestroyPiecePenalty);
            agentTimeouts.Add(ActionType.Discovery, DiscoveryPenalty);
            agentTimeouts.Add(ActionType.InformationExchange, InformationExchangePenalty);
            agentTimeouts.Add(ActionType.Move, MovePenalty);
            agentTimeouts.Add(ActionType.PutPiece, PutPenalty);
            return agentTimeouts;
        }

        public static GameMasterConfiguration GetDefault()
        {
            return new GameMasterConfiguration
            {
                MovePenalty = new TimeSpan(1500),
                InformationExchangePenalty = new TimeSpan(1000),
                DiscoveryPenalty = new TimeSpan(700),
                PutPenalty = new TimeSpan(500),
                CheckForShamPenalty = new TimeSpan(1000),
                DestroyPiecePenalty = new TimeSpan(700),
                BoardX = 40,
                BoardY = 40,
                GoalAreaHeight = 5,
                NumberOfGoals = 10,
                NumberOfPieces = 10,
                ShamProbability = 0.3f,
                TeamSize = 5,                
                CsIP = "127.0.0.1",
                CsPort = 54322
            };
        }
    }
}