using Messaging.Enumerators;
using Newtonsoft.Json;

namespace Messaging.Contracts.GameMaster
{
    public class PutDownPieceResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.PutDownPieceResponse;

        // TODO: Wait for official specifiaction
        [JsonIgnore]
        public PutDownPieceResult Result { get; set; }

        public PutDownPieceResponse(PutDownPieceResult result)
        {
            Result = result;
        }
    }
}
