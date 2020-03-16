using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Contracts.Agent
{
    public class DiscoverRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.DiscoverRequest;
    }
}
