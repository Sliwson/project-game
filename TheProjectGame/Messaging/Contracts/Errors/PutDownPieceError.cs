using Messaging.Enumerators;

namespace Messaging.Contracts.Errors
{
    public class PutDownPieceError : IErrorPayload
    {
        public MessageId GetMessageId() => MessageId.PutDownPieceError;

        public PutDownPieceErrorSubtype ErrorSubtype { get; private set; }

        public PutDownPieceError(PutDownPieceErrorSubtype errorSubtype)
        {
            ErrorSubtype = errorSubtype;
        }
    }
}
