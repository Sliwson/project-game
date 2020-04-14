using Messaging.Enumerators;
using Messaging.Serialization.JsonConverters;
using Newtonsoft.Json;

namespace Messaging.Contracts.GameMaster
{
    [JsonConverter(typeof(DiscoverResponseJsonConverter))]
    public class DiscoverResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.DiscoverResponse;

        [JsonIgnore]
        public int[,] Distances { get; private set; }

        public DiscoverResponse(int[,] distances)
        {
            Distances = distances;
        }
    }
}
