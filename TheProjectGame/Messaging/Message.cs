using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging
{
    public class Message
    {
        public MessageId MessageId { get; private set; }
        public int AgentId { get; private set; }
        internal IPayload Payload { get; private set; }
    }
}
