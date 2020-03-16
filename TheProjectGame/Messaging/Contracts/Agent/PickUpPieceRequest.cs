using Messaging.Enumerators;

namespace Messaging.Contracts.Agent
{
    public class PickUpPieceRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.PickUpPieceRequest;
    }
}
