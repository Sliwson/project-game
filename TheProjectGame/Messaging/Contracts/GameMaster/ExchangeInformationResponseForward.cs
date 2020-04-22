using Messaging.Contracts.Agent;
using Messaging.Enumerators;
using Newtonsoft.Json;

namespace Messaging.Contracts.GameMaster
{
    public class ExchangeInformationResponseForward : IPayload
    {
        public MessageId GetMessageId() => MessageId.ExchangeInformationResponseForward;
        
        [JsonRequired]
        [JsonProperty(PropertyName = "respondingID")]
        public int RespondingId { get; private set; }

        // TODO: Make sure how to format those arrays into one-dimensional
        [JsonRequired]
        [JsonProperty(PropertyName = "distances")]
        public int[,] Distances { get; private set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "redTeamGoalAreaInformations")]
        public GoalInformation[,] RedTeamGoalAreaInformation { get; private set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "blueTeamGoalAreaInformations")]
        public GoalInformation[,] BlueTeamGoalAreaInformation { get; private set; }

        [JsonConstructor]
        public ExchangeInformationResponseForward(int respondingId, int[,] distances, GoalInformation[,] redTeamGoalAreaInformation, GoalInformation[,] blueTeamGoalAreaInformation)
        {
            RespondingId = respondingId;
            Distances = distances;
            RedTeamGoalAreaInformation = redTeamGoalAreaInformation;
            BlueTeamGoalAreaInformation = blueTeamGoalAreaInformation;
        }

        public ExchangeInformationResponseForward(Message<ExchangeInformationResponse> message) : this (
            message.AgentId,
            message.Payload.Distances,
            message.Payload.RedTeamGoalAreaInformation,
            message.Payload.BlueTeamGoalAreaInformation
        ) { }
    }
}
