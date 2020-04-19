using System;
using System.Net.Sockets;

namespace Messaging.Communication
{
    public class StateObject
    {
        public readonly int BufferSize = 1 << 13;

        public Socket WorkSocket { get; private set; }
        public byte[] Buffer { get; private set; }
        public ClientType ClientType { get; private set; }

        public StateObject(ref Socket workSocket, ClientType clientType)
        {
            WorkSocket = workSocket;
            Buffer = new byte[BufferSize];
            ClientType = clientType;
        }

        public void SetReceiveCallback(AsyncCallback callback)
        {
            WorkSocket.BeginReceive(Buffer, 0, BufferSize, SocketFlags.None, callback, this);
        }
    }
}
