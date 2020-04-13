using Messaging.Enumerators;
using Newtonsoft.Json;

namespace Messaging.Contracts.GameMaster
{
    public class ExchangeInformationPayload : IPayload
    {
        public MessageId GetMessageId() => MessageId.ExchangeInformationMessage;

        [JsonRequired]
        [JsonProperty(PropertyName = "askingID")]
        public int AskingAgentId { get; private set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "leader")]
        public bool Leader { get; private set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "teamID")]
        public TeamId TeamId { get; private set; }

        public ExchangeInformationPayload(int askingAgentId, bool isLeader, TeamId teamId)
        {
            AskingAgentId = askingAgentId;
            Leader = isLeader;
            TeamId = teamId;
        }
    }
}
