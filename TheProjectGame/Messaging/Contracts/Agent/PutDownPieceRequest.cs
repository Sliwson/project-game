using Messaging.Enumerators;

namespace Messaging.Contracts.Agent
{
    public class PutDownPieceRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.PutDownPieceRequest;
    }
}
