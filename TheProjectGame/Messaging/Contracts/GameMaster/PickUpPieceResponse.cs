using Messaging.Enumerators;

namespace Messaging.Contracts.GameMaster
{
    public class PickUpPieceResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.PickUpPieceResponse;
    }
}
