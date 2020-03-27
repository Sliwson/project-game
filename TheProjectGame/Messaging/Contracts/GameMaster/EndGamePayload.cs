using Messaging.Enumerators;

namespace Messaging.Contracts.GameMaster
{
    public class EndGamePayload : IPayload
    {
        public MessageId GetMessageId() => MessageId.EndGameMessage;

        public TeamId Winner { get; private set; }

        public EndGamePayload(TeamId winner)
        {
            Winner = winner;
        }
    }
}
