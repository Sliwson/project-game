using Messaging.Enumerators;
using Newtonsoft.Json;

namespace Messaging.Contracts.Agent
{
    public class ExchangeInformationResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.ExchangeInformationResponse;

        [JsonRequired]
        [JsonProperty(PropertyName = "respondToID")]
        public int RespondToId { get; private set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "distances")]
        public int[,] Distances { get; private set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "redTeamGoalAreaInformations")]
        public GoalInformation[,] RedTeamGoalAreaInformation { get; private set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "blueTeamGoalAreaInformations")]
        public GoalInformation[,] BlueTeamGoalAreaInformation { get; private set; }

        public ExchangeInformationResponse(int respondToId, int[,] distances, GoalInformation[,] redTeamGoalAreaInformation, GoalInformation[,] blueTeamGoalAreaInformation)
        {
            RespondToId = respondToId;
            //Distances = distances;
            Distances = new int[1, 1];
            RedTeamGoalAreaInformation = redTeamGoalAreaInformation;
            BlueTeamGoalAreaInformation = blueTeamGoalAreaInformation;
        }
    }
}
