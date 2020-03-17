using Messaging.Enumerators;
using System.Drawing;

namespace Messaging.Contracts.GameMaster
{
    public class MoveResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.MoveResponse;

        public bool MadeMove { get; private set; }
        public Point CurrentPosition { get; private set; }
        public int ClosestPoint { get; private set; }

        public MoveResponse(bool madeMove, Point currentPosition, int closestPoint)
        {
            MadeMove = madeMove;
            CurrentPosition = currentPosition;
            ClosestPoint = closestPoint;
        }
    }
}
