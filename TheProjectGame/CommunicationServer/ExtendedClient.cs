using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace CommunicationServer
{
    internal class ExtendedClient
    {
        internal TcpClient Client { get; private set; }
        internal ClientType ClientType { get; private set; }

        internal ExtendedClient(TcpClient client, ClientType clientType)
        {
            Client = client;
            ClientType = clientType;
        }
    }
}
