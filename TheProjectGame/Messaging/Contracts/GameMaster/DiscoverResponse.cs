using Messaging.Enumerators;

namespace Messaging.Contracts.GameMaster
{
    public class DiscoverResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.DiscoverResponse;

        public int[,] Distances { get; private set; }

        public DiscoverResponse(int[,] distances)
        {
            Distances = distances;
        }
    }
}
