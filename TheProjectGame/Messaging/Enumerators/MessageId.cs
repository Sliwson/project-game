namespace Messaging.Enumerators
{
    public enum MessageId
    {
        // Agent's Messages
        CheckShamRequest =              001,
        DestroyPieceRequest =           002,
        DiscoverRequest =               003,
        ExchangeInformationResponse =   004,
        ExchangeInformationRequest =    005,
        JoinRequest =                   006,
        MoveRequest =                   007,
        PickUpPieceRequest =            008,
        PutDownPieceRequest =           009,

        // Game Master's Messages:
        CheckShamResponse =             101,
        DestroyPieceResponse =          102,
        DiscoverResponse =              103,
        EndGameMessage =                104,
        StartGameMessage =              105,
        ExchangeInformationMessage =    106,
        JoinResponse =                  107,
        MoveResponse =                  108,
        PickUpPieceResponse =           109,
        PutDownPieceResponse =          110,

        // Error Messages:
        MoveError =                     901,
        PutDownPieceError =             902, // TODO: Create issue to ensure these types are properly defined
        PickUpPieceError =              903, //
        IgnoredDelayError =             904,
        UndefinedError =                905
    }
}
