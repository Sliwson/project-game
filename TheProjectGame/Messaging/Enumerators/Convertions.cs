using System;

namespace Messaging.Enumerators
{
    public static class Convertions
    {
        public static ActionType ToActionType(this MessageId id)
        {
            switch (id)
            {
                case MessageId.CheckShamRequest:
                    return ActionType.CheckForSham;
                case MessageId.DestroyPieceRequest:
                    return ActionType.DestroyPiece;
                case MessageId.DiscoverRequest:
                    return ActionType.Discovery;
                case MessageId.MoveRequest:
                    return ActionType.Move;
                case MessageId.PutDownPieceRequest:
                    return ActionType.PutPiece;
                case MessageId.ExchangeInformationRequest:
                case MessageId.ExchangeInformationResponse:
                    return ActionType.InformationExchange;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
