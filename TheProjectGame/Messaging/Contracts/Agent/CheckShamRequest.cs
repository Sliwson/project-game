using Messaging.Enumerators;

namespace Messaging.Contracts.Agent
{
    public class CheckShamRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.CheckShamRequest;
    }
}
