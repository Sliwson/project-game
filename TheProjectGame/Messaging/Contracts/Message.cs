using Messaging.Enumerators;

namespace Messaging.Contracts
{
    public class Message<T> where T: IPayload
    {
        public MessageId MessageId { get; internal set; }
        public int AgentId { get; internal set; }
        public T Payload { get; internal set; }
    }
}
