using Messaging.Contracts;
using Messaging.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Messaging.Communication
{
    // TODO (#IO-45): Add exception handling and logging (GM and Agent) 
    public class ClientNetworkComponent : INetworkComponent
    {
        private ConcurrentQueue<BaseMessage> messageQueue;

        private IPEndPoint communicationServerEndpoint;
        private Socket socket;

        private ManualResetEvent connectDone;

        public ClientNetworkComponent(string serverIPAddress, int serverPort)
        {
            messageQueue = new ConcurrentQueue<BaseMessage>();

            var ipAddress = IPAddress.Parse(serverIPAddress);
            communicationServerEndpoint = new IPEndPoint(ipAddress, serverPort);

            connectDone = new ManualResetEvent(false);
        }

        public bool Connect(ClientType clientType)
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.NoDelay = true;
                socket.BeginConnect(communicationServerEndpoint, new AsyncCallback(ConnectCallback), socket);
                connectDone.WaitOne();

                var state = new StateObject(ref socket, clientType);
                state.SetReceiveCallback(new AsyncCallback(ReceiveCallback));

                return true;
            }
            catch (Exception e)
            {
                //Console.WriteLine("Exception: {0}", e);
                return false;
            }
        }

        public bool Disconnect()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                //Console.WriteLine("Closed");
                return true;
            }
            catch (Exception e)
            {
                //Console.WriteLine("Unable to disconnect: {0}", e);
                return false;
            }
        }

        public void SendMessage(BaseMessage message)
        {
            var wrappedMessage = MessageSerializer.SerializeAndWrapMessage(message);

            Send(socket, wrappedMessage);
        }

        public IEnumerable<BaseMessage> GetIncomingMessages()
        {
            var result = messageQueue.ToArray();
            messageQueue.Clear();
            return result;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                client.EndConnect(ar);

                //Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

                connectDone.Set();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, byte[] message)
        {
            client.Send(message, message.Length, SocketFlags.None);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var client = state.WorkSocket;

            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 2)
            {
                try
                {
                    foreach(var message in MessageSerializer.UnwrapAndDeserializeMessages(state.Buffer, bytesRead))
                    {
                        messageQueue.Enqueue(message);
                    }
                }
                catch (Exception e)
                {
                    //if (e is ArgumentOutOfRangeException)
                        //Console.WriteLine(e.Message);
                    //else
                        //throw;
                }
                state.SetReceiveCallback(new AsyncCallback(ReceiveCallback));
            }
            else if (bytesRead > 0)
            {
                //Console.WriteLine("Received message was too short (expected more than 2 bytes)");
                state.SetReceiveCallback(new AsyncCallback(ReceiveCallback));
            }
        }
    }
}
