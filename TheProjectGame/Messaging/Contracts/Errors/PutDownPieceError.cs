using Messaging.Enumerators;
using Newtonsoft.Json;

namespace Messaging.Contracts.Errors
{
    public class PutDownPieceError : IErrorPayload
    {
        public MessageId GetMessageId() => MessageId.PutDownPieceError;

        [JsonRequired]
        [JsonProperty(PropertyName = "errorSubtype")]
        public PutDownPieceErrorSubtype ErrorSubtype { get; private set; }

        public PutDownPieceError(PutDownPieceErrorSubtype errorSubtype)
        {
            ErrorSubtype = errorSubtype;
        }
    }
}
