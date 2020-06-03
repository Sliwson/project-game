using Messaging.Enumerators;
using Messaging.Serialization.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Messaging.Contracts.GameMaster
{
    public class StartGamePayload : IPayload
    {
        public MessageId GetMessageId() => MessageId.StartGameMessage;

        [JsonRequired]
        [JsonProperty(PropertyName = "agentID")]
        public int AgentId { get; set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "alliesIDs")]
        public int[] AlliesIds { get; set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "leaderID")]
        public int LeaderId { get; set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "enemiesIDs")]
        public int[] EnemiesIds { get; set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "teamID")]
        public TeamId TeamId { get; set; }

        [JsonRequired]
        [JsonConverter(typeof(PointJsonConverter))]
        [JsonProperty(PropertyName = "boardSize")]
        public Point BoardSize { get; set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "goalAreaSize")]
        public int GoalAreaHeight { get; set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "numberOfPlayers")]
        public NumberOfPlayers NumberOfPlayers { get; set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "numberOfPieces")]
        public int NumberOfPieces { get; set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "numberOfGoals")]
        public int NumberOfGoals { get; set; }

        [JsonRequired]
        [JsonConverter(typeof(ActionPenaltiesJsonConverter))]
        [JsonProperty(PropertyName = "penalties")]
        public Dictionary<ActionType, TimeSpan> Penalties { get; set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "shamPieceProbability")]
        public float ShamPieceProbability { get; set; }

        [JsonRequired]
        [JsonConverter(typeof(PointJsonConverter))]
        [JsonProperty(PropertyName = "position")]
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
                                Dictionary<ActionType, TimeSpan> penalties, 
                                float shamPieceProbability, 
                                Point position)
        {
            NumberOfPlayers = new NumberOfPlayers();

            AgentId = agentId;
            AlliesIds = alliesIds;
            LeaderId = leaderId;
            EnemiesIds = enemiesIds;
            TeamId = teamId;
            BoardSize = boardSize;
            GoalAreaHeight = goalAreaHeight;
            NumberOfPlayers.Allies = numberOfAllies;
            NumberOfPlayers.Enemies = numberOfEnemies;
            NumberOfPieces = numberOfPieces;
            NumberOfGoals = numberOfGoals;
            Penalties = penalties;
            ShamPieceProbability = shamPieceProbability;
            Position = position;
        }
    }
}
