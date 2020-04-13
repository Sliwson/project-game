using Messaging.Enumerators;
using Messaging.Serialization.JsonConverters;
using Newtonsoft.Json;
using System.Drawing;

namespace Messaging.Contracts.GameMaster
{
    public class MoveResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.MoveResponse;

        [JsonRequired]
        [JsonProperty(PropertyName = "madeMove")]
        public bool MadeMove { get; private set; }

        [JsonRequired]
        [JsonConverter(typeof(PointJsonConverter))]
        [JsonProperty(PropertyName = "currentPosition")]
        public Point CurrentPosition { get; private set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "closestPiece")]
        public int ClosestPiece { get; private set; }

        public MoveResponse(bool madeMove, Point currentPosition, int closestPiece)
        {
            MadeMove = madeMove;
            CurrentPosition = currentPosition;
            ClosestPiece = closestPiece;
        }
    }
}
