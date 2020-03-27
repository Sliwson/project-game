using Messaging.Enumerators;

namespace Messaging.Contracts.Agent
{
    public class DestroyPieceRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.DestroyPieceRequest;
    }
}
