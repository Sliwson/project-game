using Messaging.Enumerators;
using Newtonsoft.Json;
using System;

namespace Messaging.Contracts
{
    public abstract class BaseMessage
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "messageID", Order = 0)]
        public MessageId MessageId { get; protected set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "agentID", Order = 1)]
        public int AgentId { get; protected set; }

        [JsonIgnore]
        public IPayload Payload { get; protected set; }
        
        [JsonIgnore]
        public Type PayloadType { get; }
    }

    public class Message<T> : BaseMessage where T : IPayload
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "payload", Order = 2)]
        new public T Payload { get; }

        [JsonIgnore]
        new public Type PayloadType => typeof(T);

        public Message(MessageId messageId, int agentId, T payload)
        {
            MessageId = messageId;
            AgentId = agentId;
            Payload = payload;
        }
    }
}
