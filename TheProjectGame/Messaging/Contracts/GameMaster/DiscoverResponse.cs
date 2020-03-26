using Messaging.Enumerators;

namespace Messaging.Contracts.GameMaster
{
    public class DiscoverResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.DiscoverResponse;

        public Distances Distances { get; private set; }

        public DiscoverResponse(Distances distances)
        {
            Distances = distances;
        }
    }
}
