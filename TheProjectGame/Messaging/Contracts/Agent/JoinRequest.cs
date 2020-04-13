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

        // TODO: Create issue to decide whether to keep this field
        [JsonRequired]
        [JsonProperty(PropertyName = "isTeamLeader")]
        public bool IsTeamLeader { get; private set; }

        public JoinRequest(TeamId teamId, bool isTeamLeader)
        {
            TeamId = teamId;
            IsTeamLeader = isTeamLeader;
        }
    }
}
