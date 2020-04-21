using Messaging.Enumerators;
using Newtonsoft.Json;

namespace Messaging.Contracts.GameMaster
{
    public class JoinResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.JoinResponse;
        
        [JsonRequired]
        [JsonProperty(PropertyName = "accepted")]
        public bool Accepted { get; private set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "agentID")]
        public int AgentId { get; private set; }

        public JoinResponse(bool accepted, int agentId)
        {
            Accepted = accepted;
            AgentId = agentId;
        }
    }
}
