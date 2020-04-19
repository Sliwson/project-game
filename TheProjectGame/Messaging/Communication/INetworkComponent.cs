using Messaging.Contracts;
using System.Collections.Generic;

namespace Messaging.Communication
{
    public interface INetworkComponent
    {
        public bool Connect(ClientType clientType);
        public bool Disconnect();
        public void SendMessage(BaseMessage message);
        public IEnumerable<BaseMessage> GetIncomingMessages();
    }
}
