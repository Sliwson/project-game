using Messaging.Enumerators;

namespace Messaging.Contracts.GameMaster
{
    public class DestroyPieceResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.DestroyPieceResponse;
    }
}
