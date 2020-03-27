using Messaging.Enumerators;
using System;

namespace Messaging.Contracts.Errors
{
    public class IgnoredDelayError : IErrorPayload
    {
        public MessageId GetMessageId() => MessageId.IgnoredDelayError;

        public DateTime WaitUntil { get; private set; }

        public IgnoredDelayError(DateTime waitUntil)
        {
            WaitUntil = waitUntil;
        }
    }
}
