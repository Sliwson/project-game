using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Contracts
{
    public interface IPayload
    {
        MessageId GetMessageId();
    }
}
