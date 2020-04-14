using Messaging.Enumerators;
using Newtonsoft.Json;

namespace Messaging.Contracts.Agent
{
    public class ExchangeInformationRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.ExchangeInformationRequest;

        [JsonRequired]
        [JsonProperty(PropertyName = "askedAgentID")]
        public int AskedAgentId { get; private set; }

        public ExchangeInformationRequest(int askedAgentId)
        {
            AskedAgentId = askedAgentId;
        }
    }
}
