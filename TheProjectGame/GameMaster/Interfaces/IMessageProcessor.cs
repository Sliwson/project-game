using Messaging.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster.Interfaces
{
    interface IMessageProcessor
    {
        public BaseMessage ProcessMessage(BaseMessage message);
    }
}
