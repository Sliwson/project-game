using Messaging.Enumerators;

namespace Messaging.Contracts.GameMaster
{
    public class PutDownPieceResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.PutDownPieceResponse;
    }
}
