using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CommunicationServer
{
    internal class ExtendedListener
    {
        internal Socket Listener { get; private set; }
        internal ClientType ClientType { get; private set; }
        internal ManualResetEvent Barrier { get; private set; }

        internal ExtendedListener(Socket listener, ClientType clientType, ref ManualResetEvent barrier)
        {
            Listener = listener;
            ClientType = clientType;
            Barrier = barrier;
        }
    }
}
