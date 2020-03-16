using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Contracts.Agent
{
    public class PickUpPieceRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.PickUpPieceRequest;
    }
}
