using Messaging.Enumerators;

namespace Messaging.Contracts.Agent
{
    public class ExchangeInformationRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.ExchangeInformationRequest;

        public int AskedAgentId { get; private set; }

        public ExchangeInformationRequest(int askedAgentId)
        {
            AskedAgentId = askedAgentId;
        }
    }
}
