using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.Errors;
using Messaging.Contracts.GameMaster;
using System;

namespace Messaging.Enumerators
{
    public enum MessageId
    {
        // Agent's Messages
        CheckShamRequest =                   001,
        DestroyPieceRequest =                002,
        DiscoverRequest =                    003,
        ExchangeInformationResponse =        004,
        ExchangeInformationRequest =         005,
        JoinRequest =                        006,
        MoveRequest =                        007,
        PickUpPieceRequest =                 008,
        PutDownPieceRequest =                009,

        // Game Master's Messages:
        CheckShamResponse =                  101,
        DestroyPieceResponse =               102,
        DiscoverResponse =                   103,
        EndGameMessage =                     104,
        StartGameMessage =                   105,
        ExchangeInformationMessage =         106,
        JoinResponse =                       107,
        MoveResponse =                       108,
        PickUpPieceResponse =                109,
        PutDownPieceResponse =               110,
        ExchangeInformationResponseMessage = 111,

        // Error Messages:
        MoveError =                          901,
        PickUpPieceError =                   902,
        PutDownPieceError =                  903,
        IgnoredDelayError =                  904,
        UndefinedError =                     905
    }

    public static class MessageIdExtensions
    {
        public static Type GetCorrespondingMessageType(this MessageId messageId)
        {
            switch (messageId)
            {
                case MessageId.CheckShamRequest:
                    return typeof(Message<CheckShamRequest>);
                case MessageId.DestroyPieceRequest:
                    return typeof(Message<DestroyPieceRequest>);
                case MessageId.DiscoverRequest:
                    return typeof(Message<DiscoverRequest>);
                case MessageId.ExchangeInformationResponse:
                    return typeof(Message<ExchangeInformationResponse>);
                case MessageId.ExchangeInformationRequest:
                    return typeof(Message<ExchangeInformationRequest>);
                case MessageId.JoinRequest:
                    return typeof(Message<JoinRequest>);
                case MessageId.MoveRequest:
                    return typeof(Message<MoveRequest>);
                case MessageId.PickUpPieceRequest:
                    return typeof(Message<PickUpPieceRequest>);
                case MessageId.PutDownPieceRequest:
                    return typeof(Message<PutDownPieceRequest>);
                case MessageId.CheckShamResponse:
                    return typeof(Message<CheckShamResponse>);
                case MessageId.DestroyPieceResponse:
                    return typeof(Message<DestroyPieceResponse>);
                case MessageId.DiscoverResponse:
                    return typeof(Message<DiscoverResponse>);
                case MessageId.EndGameMessage:
                    return typeof(Message<EndGamePayload>);
                case MessageId.StartGameMessage:
                    return typeof(Message<StartGamePayload>);
                case MessageId.ExchangeInformationMessage:
                    return typeof(Message<ExchangeInformationPayload>);
                case MessageId.JoinResponse:
                    return typeof(Message<JoinResponse>);
                case MessageId.MoveResponse:
                    return typeof(Message<MoveResponse>);
                case MessageId.PickUpPieceResponse:
                    return typeof(Message<PickUpPieceResponse>);
                case MessageId.PutDownPieceResponse:
                    return typeof(Message<PutDownPieceResponse>);
                case MessageId.MoveError:
                    return typeof(Message<MoveError>);
                case MessageId.PickUpPieceError:
                    return typeof(Message<PickUpPieceError>);
                case MessageId.PutDownPieceError:
                    return typeof(Message<PutDownPieceError>);
                case MessageId.IgnoredDelayError:
                    return typeof(Message<IgnoredDelayError>);
                case MessageId.UndefinedError:
                    return typeof(Message<UndefinedError>);
                default:
                    throw new ArgumentException($"No payload type corresponding to message: {messageId.ToString()}");
            }
        }
    }
}
