using Messaging.Enumerators;
using Newtonsoft.Json;
using System;

namespace Messaging.Contracts.Errors
{
    public class IgnoredDelayError : IErrorPayload
    {
        public MessageId GetMessageId() => MessageId.IgnoredDelayError;

        [JsonIgnore]
        public TimeSpan RemainingDelay { get; private set; }

        public IgnoredDelayError(TimeSpan remainingDelay)
        {
            RemainingDelay = remainingDelay;
        }

        // TODO: Wait for official specifiaction
        // Only for serialization
        [JsonRequired]
        [JsonProperty(PropertyName = "delayInMiliseconds")]
        private int DelayInMiliseconds 
        { 
            get { return (int)RemainingDelay.TotalMilliseconds; }
            set { RemainingDelay = TimeSpan.FromMilliseconds(value); }
        }
    }
}
