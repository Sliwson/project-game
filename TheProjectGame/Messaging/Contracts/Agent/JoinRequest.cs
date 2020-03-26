using Messaging.Enumerators;

namespace Messaging.Contracts.Agent
{
    public class JoinRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.JoinRequest;

        public TeamId TeamId { get; private set; }
        public bool IsTeamLeader { get; private set; }

        public JoinRequest(TeamId teamId, bool isTeamLeader)
        {
            TeamId = teamId;
            IsTeamLeader = isTeamLeader;
        }
    }
}
