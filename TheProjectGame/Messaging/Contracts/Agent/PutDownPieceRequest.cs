using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Contracts.Agent
{
    public class PutDownPieceRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.PutDownPieceRequest;
    }
}
