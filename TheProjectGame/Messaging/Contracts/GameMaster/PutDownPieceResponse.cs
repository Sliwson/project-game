using Messaging.Enumerators;
using Newtonsoft.Json;

namespace Messaging.Contracts.GameMaster
{
    public class PutDownPieceResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.PutDownPieceResponse;

        [JsonRequired]
        [JsonProperty(PropertyName = "putInformation")]
        public PutDownPieceResult Result { get; set; }

        public PutDownPieceResponse(PutDownPieceResult result)
        {
            Result = result;
        }
    }
}
