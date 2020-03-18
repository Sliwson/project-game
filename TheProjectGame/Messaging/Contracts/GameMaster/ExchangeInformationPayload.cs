using Messaging.Enumerators;

namespace Messaging.Contracts.GameMaster
{
    public class ExchangeInformationPayload : IPayload
    {
        public MessageId GetMessageId() => MessageId.ExchangeInformationMessage;

        public int AskingAgentId { get; private set; }
        public bool Leader { get; private set; }
        public TeamId TeamId { get; private set; }

        public ExchangeInformationPayload(int askingAgentId, bool isLeader, TeamId teamId)
        {
            AskingAgentId = askingAgentId;
            Leader = isLeader;
            TeamId = teamId;
        }
    }
}
