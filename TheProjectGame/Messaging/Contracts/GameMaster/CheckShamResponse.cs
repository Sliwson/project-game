using Messaging.Enumerators;
using Newtonsoft.Json;

namespace Messaging.Contracts.GameMaster
{
    public class CheckShamResponse : IPayload
    {
        public MessageId GetMessageId() => MessageId.CheckShamResponse;

        [JsonRequired]
        [JsonProperty(PropertyName = "sham")]
        public bool Sham { get; private set; }

        public CheckShamResponse(bool isSham)
        {
            Sham = isSham;
        }
    }
}
