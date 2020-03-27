using Messaging.Enumerators;

namespace Messaging.Contracts.Agent
{
    public class DiscoverRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.DiscoverRequest;
    }
}
