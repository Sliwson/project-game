using Messaging.Enumerators;

namespace Messaging.Contracts
{
    public interface IPayload
    {
        MessageId GetMessageId();
    }
}
