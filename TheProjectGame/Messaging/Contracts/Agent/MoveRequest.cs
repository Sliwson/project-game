using Messaging.Enumerators;
using Newtonsoft.Json;

namespace Messaging.Contracts.Agent
{
    public class MoveRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.MoveRequest;

        [JsonRequired]
        [JsonProperty(PropertyName = "direction")]
        public Direction Direction { get; private set; }

        public MoveRequest(Direction direction)
        {
            Direction = direction;
        }
    }
}
