using Messaging.Enumerators;
using Newtonsoft.Json;

namespace Messaging.Contracts.Agent
{
    public class JoinRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.JoinRequest;

        [JsonRequired]
        [JsonProperty(PropertyName = "teamID")]
        public TeamId TeamId { get; private set; }

        public JoinRequest(TeamId teamId)
        {
            TeamId = teamId;
        }
    }
}
