using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace CommunicationServer
{
    internal class ClientStateObject
    {
        internal readonly int BufferSize = 1 << 13;

        internal Socket WorkSocket { get; private set; }
        internal byte[] Buffer { get; private set; }
        internal ClientType ClientType { get; private set; }

        internal ClientStateObject(ref Socket workSocket, ClientType clientType)
        {
            WorkSocket = workSocket;
            Buffer = new byte[BufferSize];
            ClientType = clientType;
        }

        internal void SetReadCallback(AsyncCallback callback)
        {
            WorkSocket.BeginReceive(Buffer, 0, BufferSize, SocketFlags.None, callback, this);
        }
    }
}
