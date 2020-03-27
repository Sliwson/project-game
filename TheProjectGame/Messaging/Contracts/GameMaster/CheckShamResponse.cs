using Messaging.Enumerators;

namespace Messaging.Contracts.GameMaster
{
    public class CheckShamResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.CheckShamResponse;

        public bool Sham { get; private set; }

        public CheckShamResponse(bool isSham)
        {
            Sham = isSham;
        }
    }
}
