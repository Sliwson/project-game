using System;
using System.Collections.Generic;
using System.Text;

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
                case MessageId.PickUpPieceRequest:
                    return ActionType.PickUpPiece;
                case MessageId.PutDownPieceRequest:
                    return ActionType.PutPiece;
                case MessageId.ExchangeInformationRequest:
                    return ActionType.InformationRequest;
                case MessageId.ExchangeInformationResponse:
                    return ActionType.InformationResponse;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
