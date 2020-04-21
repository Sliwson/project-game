using Messaging.Enumerators;
using Messaging.Serialization.JsonConverters;
using Newtonsoft.Json;
using System.Drawing;

namespace Messaging.Contracts.Errors
{
    public class UndefinedError : IErrorPayload
    {
        public MessageId GetMessageId() => MessageId.UndefinedError;

        [JsonRequired]
        [JsonConverter(typeof(PointJsonConverter))]
        [JsonProperty(PropertyName = "position")]
        public Point Position { get; private set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "holdingPiece")]
        public bool HoldingPiece { get; private set; }

        public UndefinedError(Point position, bool isHoldingPiece)
        {
            Position = position;
            HoldingPiece = isHoldingPiece;
        }
    }
}
