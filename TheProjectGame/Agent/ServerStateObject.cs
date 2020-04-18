using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Agent
{
    public class ServerStateObject
    {
        public readonly int BufferSize = 1 << 13;

        public Socket WorkSocket { get; private set; }
        public byte[] Buffer { get; private set; }

        public ServerStateObject(ref Socket workSocket)
        {
            WorkSocket = workSocket;
            Buffer = new byte[BufferSize];
        }

        public void SetReceiveCallback(AsyncCallback callback)
        {
            WorkSocket.BeginReceive(Buffer, 0, BufferSize, SocketFlags.None, callback, this);
        }
    }
}
