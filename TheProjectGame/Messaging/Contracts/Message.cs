using Messaging.Enumerators;
using Newtonsoft.Json;

namespace Messaging.Contracts
{
    public abstract class BaseMessage
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "messageID", Order = 0)]
        public MessageId MessageId { get; protected set; }

        [JsonProperty(PropertyName = "agentID", Order = 1)]
        public int AgentId { get; protected set; }

        [JsonProperty(PropertyName = "correlationID", Order = 2, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? CorrelationId { get; protected set; }

        [JsonIgnore]
        public IPayload Payload { get; protected set; }

        public void SetAgentId(int newId)
        {
            AgentId = newId;
        }

        public void SetCorrelationId(int? newId)
        {
            CorrelationId = newId;
        }
    }

    public class Message<T> : BaseMessage where T : IPayload
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "payload", Order = 3)]
        new public T Payload { get; }

        public Message(MessageId messageId, T payload, int? correlationID = null, int agentId = 0)
        {
            MessageId = messageId;
            AgentId = agentId;
            CorrelationId = correlationID;
            Payload = payload;
        }
    }
}
