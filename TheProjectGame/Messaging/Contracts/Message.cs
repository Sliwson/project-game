using Messaging.Enumerators;
using System;

namespace Messaging.Contracts
{
    public abstract class BaseMessage
    {
        public MessageId MessageId { get; protected set; }
        public int AgentId { get; protected set; }
        public IPayload Payload { get; protected set; }
        public Type PayloadType { get; }
    }

    public class Message<T> : BaseMessage where T : IPayload
    {
        new public T Payload { get; }
        new public Type PayloadType => typeof(T);

        public Message(MessageId messageId, int agentId, T payload)
        {
            MessageId = messageId;
            AgentId = agentId;
            Payload = payload;
        }
    }
}
