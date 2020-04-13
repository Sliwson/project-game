using Messaging.Enumerators;
using Messaging.Serialization.JsonConverters;
using Newtonsoft.Json;
using System.Drawing;

namespace Messaging.Contracts.Errors
{
    public class MoveError : IErrorPayload
    {
        public MessageId GetMessageId() => MessageId.MoveError;

        [JsonRequired]
        [JsonConverter(typeof(PointJsonConverter))]
        [JsonProperty(PropertyName = "position")]
        public Point Position { get; private set; }

        public MoveError(Point position)
        {
            Position = position;
        }
    }
}
