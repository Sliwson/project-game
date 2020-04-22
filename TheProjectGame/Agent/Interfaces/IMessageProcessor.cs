using Messaging.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agent
{
    interface IMessageProcessor
    {
        public ActionResult AcceptMessage(BaseMessage message);
        public void SendMessage(BaseMessage message);
    }
}
