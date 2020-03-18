using Messaging.Enumerators;
using System.Drawing;

namespace Messaging.Contracts.Errors
{
    public class UndefinedError : IErrorPayload
    {
        public MessageId GetMessageId() => MessageId.UndefinedError;

        public Point Position { get; private set; }
        public bool HoldingPiece { get; private set; }

        public UndefinedError(Point position, bool isHoldingPiece)
        {
            Position = position;
            HoldingPiece = isHoldingPiece;
        }
    }
}
