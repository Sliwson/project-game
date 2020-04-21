using Messaging.Enumerators;
using Newtonsoft.Json;

namespace Messaging.Contracts.Errors
{
    public class PickUpPieceError : IErrorPayload
    {
        public MessageId GetMessageId() => MessageId.PickUpPieceError;

        [JsonRequired]
        [JsonProperty(PropertyName = "errorSubtype")]
        public PickUpPieceErrorSubtype ErrorSubtype { get; private set; }

        public PickUpPieceError(PickUpPieceErrorSubtype errorSubtype)
        {
            ErrorSubtype = errorSubtype;
        }
    }
}
