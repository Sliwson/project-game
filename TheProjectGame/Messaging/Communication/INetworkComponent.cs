using Messaging.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Communication
{
    public interface INetworkComponent
    {
        public bool Connect();
        public bool Disconnect();
        public void SendMessage(BaseMessage message);
        public IEnumerable<BaseMessage> GetIncomingMessages();
    }
}
