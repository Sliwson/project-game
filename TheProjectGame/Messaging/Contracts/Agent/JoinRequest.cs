using Messaging.Enumerators;

namespace Messaging.Contracts.Agent
{
    public class JoinRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.JoinRequest;

        public TeamId TeamId { get; set; }

        public JoinRequest(TeamId teamId)
        {
            TeamId = teamId;
        }
    }
}
