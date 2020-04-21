using Messaging.Enumerators;
using Newtonsoft.Json;

namespace Messaging.Contracts.GameMaster
{
    public class EndGamePayload : IPayload
    {
        public MessageId GetMessageId() => MessageId.EndGameMessage;

        [JsonRequired]
        [JsonProperty(PropertyName = "winner")]
        public TeamId Winner { get; private set; }

        public EndGamePayload(TeamId winner)
        {
            Winner = winner;
        }
    }
}
