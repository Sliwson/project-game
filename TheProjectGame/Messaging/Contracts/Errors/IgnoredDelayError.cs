using Messaging.Enumerators;
using System;

namespace Messaging.Contracts.Errors
{
    public class IgnoredDelayError : IErrorPayload
    {
        public MessageId GetMessageId() => MessageId.IgnoredDelayError;

        public TimeSpan RemainingDelay { get; private set; }

        public IgnoredDelayError(TimeSpan remainingDelay)
        {
            RemainingDelay = remainingDelay;
        }
    }
}
