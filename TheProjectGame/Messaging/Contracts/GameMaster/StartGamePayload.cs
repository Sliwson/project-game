using Messaging.Enumerators;
using System.Collections.Generic;
using System.Drawing;

namespace Messaging.Contracts.GameMaster
{
    public class StartGamePayload : IPayload
    {
        public MessageId GetMessageId() => MessageId.StartGameMessage;

        public int AgentId { get; set; }
        public int[] AlliesIds { get; set; }
        public int LeaderId { get; set; }
        public int[] EnemiesIds { get; set; }
        public TeamId TeamId { get; set; }
        public Point BoardSize { get; set; }
        public int GoalAreaHeight { get; set; }
        public int NumberOfAllies { get; set; }
        public int NumberOfEnemies { get; set; }
        public int NumberOfPieces { get; set; }
        public int NumberOfGoals { get; set; }
        public Dictionary<ActionType, decimal> Penalties { get; set; }
        public decimal ShamPieceProbability { get; set; }
        public Point Position { get; set; }

        public StartGamePayload(int agentId, 
                                int[] alliesIds, 
                                int leaderId, 
                                int[] enemiesIds, 
                                TeamId teamId, 
                                Point boardSize, 
                                int goalAreaHeight, 
                                int numberOfAllies, 
                                int numberOfEnemies, 
                                int numberOfPieces, 
                                int numberOfGoals, 
                                Dictionary<ActionType, decimal> penalties, 
                                decimal shamPieceProbability, 
                                Point position)
        {
            AgentId = agentId;
            AlliesIds = alliesIds;
            LeaderId = leaderId;
            EnemiesIds = enemiesIds;
            TeamId = teamId;
            BoardSize = boardSize;
            GoalAreaHeight = goalAreaHeight;
            NumberOfAllies = numberOfAllies;
            NumberOfEnemies = numberOfEnemies;
            NumberOfPieces = numberOfPieces;
            NumberOfGoals = numberOfGoals;
            Penalties = penalties;
            ShamPieceProbability = shamPieceProbability;
            Position = position;
        }
    }
}
