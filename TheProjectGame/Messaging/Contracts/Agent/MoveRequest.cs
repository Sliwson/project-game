using Messaging.Enumerators;

namespace Messaging.Contracts.Agent
{
    public class MoveRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.MoveRequest;

        public Direction Direction { get; private set; }

        public MoveRequest(Direction direction)
        {
            Direction = direction;
        }
    }
}
