using Messaging.Enumerators;

namespace Messaging.Contracts.Errors
{
    public class PickUpPieceError : IErrorPayload
    {
        public MessageId GetMessageId() => MessageId.PickUpPieceError;

        public PickUpPieceErrorSubtype ErrorSubtype { get; private set; }

        public PickUpPieceError(PickUpPieceErrorSubtype errorSubtype)
        {
            ErrorSubtype = errorSubtype;
        }
    }
}
