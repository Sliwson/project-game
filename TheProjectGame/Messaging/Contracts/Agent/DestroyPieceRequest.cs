using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Contracts.Agent
{
    public class DestroyPieceRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.DestroyPieceRequest;
    }
}
