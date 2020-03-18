using Messaging.Enumerators;
using System.Drawing;

namespace Messaging.Contracts.Errors
{
    public class MoveError : IErrorPayload
    {
        public MessageId GetMessageId() => MessageId.MoveError;

        public Point Position { get; private set; }

        public MoveError(Point position)
        {
            Position = position;
        }
    }
}
