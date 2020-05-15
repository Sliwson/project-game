using Messaging.Enumerators;
using Messaging.Serialization.Extensions;
using Newtonsoft.Json;

namespace Messaging.Contracts.Agent
{
    public class ExchangeInformationResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.ExchangeInformationResponse;

        [JsonRequired]
        [JsonProperty(PropertyName = "respondToID")]
        public int RespondToId { get; private set; }

        // TODO: Make sure how to format those arrays into one-dimensional
        [JsonRequired]
        [JsonProperty(PropertyName = "distances")]
        public int[] Distances { get; private set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "redTeamGoalAreaInformations")]
        public GoalInformation[] RedTeamGoalAreaInformation { get; private set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "blueTeamGoalAreaInformations")]
        public GoalInformation[] BlueTeamGoalAreaInformation { get; private set; }

        [JsonConstructor]
        public ExchangeInformationResponse(int respondToId, int[] distances, GoalInformation[] redTeamGoalAreaInformation, GoalInformation[] blueTeamGoalAreaInformation)
        {
            RespondToId = respondToId;
            Distances = distances;
            RedTeamGoalAreaInformation = redTeamGoalAreaInformation;
            BlueTeamGoalAreaInformation = blueTeamGoalAreaInformation;
        }

        public ExchangeInformationResponse(int respondToId, int[,] distances, GoalInformation[,] redTeamGoalAreaInformation, GoalInformation[,] blueTeamGoalAreaInformation) : this (
            respondToId, 
            distances.ToOneDimensionalArray(), 
            redTeamGoalAreaInformation.ToOneDimensionalArray(), 
            blueTeamGoalAreaInformation.ToOneDimensionalArray()
        ) { }
    }
}
