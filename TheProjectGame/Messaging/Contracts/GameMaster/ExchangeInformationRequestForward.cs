using Messaging.Enumerators;
using Newtonsoft.Json;

namespace Messaging.Contracts.GameMaster
{
    public class ExchangeInformationRequestForward : IPayload
    {
        public MessageId GetMessageId() => MessageId.ExchangeInformationRequestForward;

        [JsonRequired]
        [JsonProperty(PropertyName = "askingID")]
        public int AskingAgentId { get; private set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "leader")]
        public bool Leader { get; private set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "teamID")]
        public TeamId TeamId { get; private set; }

        public ExchangeInformationRequestForward(int askingAgentId, bool isLeader, TeamId teamId)
        {
            AskingAgentId = askingAgentId;
            Leader = isLeader;
            TeamId = teamId;
        }
    }
}
