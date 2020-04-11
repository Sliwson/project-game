using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace CommunicationServer
{
    internal class ExtendedListener
    {
        internal TcpListener Listener { get; private set; }
        internal ClientType ClientType { get; private set; }

        internal ExtendedListener(TcpListener listener, ClientType clientType)
        {
            Listener = listener;
            ClientType = clientType;
        }
    }
}
