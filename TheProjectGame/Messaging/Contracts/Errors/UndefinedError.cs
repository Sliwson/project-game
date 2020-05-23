using Messaging.Enumerators;
using Messaging.Serialization.JsonConverters;
using Newtonsoft.Json;
using System.Drawing;

namespace Messaging.Contracts.Errors
{
    public class UndefinedError : IErrorPayload
    {
        public MessageId GetMessageId() => MessageId.UndefinedError;

        [JsonConverter(typeof(PointJsonConverter))]
        [JsonProperty(PropertyName = "position", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Point? Position { get; private set; }

        [JsonProperty(PropertyName = "holdingPiece", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? HoldingPiece { get; private set; }

        public UndefinedError(Point? position, bool? isHoldingPiece)
        {
            Position = position;
            HoldingPiece = isHoldingPiece;
        }
    }
}
