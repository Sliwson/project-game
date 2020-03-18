using Messaging.Enumerators;

namespace Messaging.Contracts.GameMaster
{
    public class JoinResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.JoinResponse;

        public bool Accepted { get; private set; }
        public int AgentId { get; private set; }

        public JoinResponse(bool accepted, int agentId)
        {
            Accepted = accepted;
            AgentId = agentId;
        }
    }
}
