using Messaging.Contracts;
using Messaging.Serialization;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Messaging.Communication
{
    public class ClientNetworkComponent : INetworkComponent
    {
        public Exception Exception { get; private set; } = null;

        private ConcurrentQueue<BaseMessage> messageQueue;

        private IPEndPoint communicationServerEndpoint;
        private Socket socket;
        private ManualResetEvent connectDone;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ClientNetworkComponent(string serverIPAddress, int serverPort)
        {
            messageQueue = new ConcurrentQueue<BaseMessage>();
            connectDone = new ManualResetEvent(false);

            try
            {
                var ipAddress = IPAddress.Parse(serverIPAddress);
                communicationServerEndpoint = new IPEndPoint(ipAddress, serverPort);
            }
            catch (Exception e)
            {
                logger.Error("[NetworkComponent] {message}", e.Message);
                throw new CommunicationErrorException(CommunicationExceptionType.InvalidEndpoint, e);
            }
        }

        public bool Connect(ClientType clientType)
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.NoDelay = true;
                socket.BeginConnect(communicationServerEndpoint, new AsyncCallback(ConnectCallback), socket);
                connectDone.WaitOne();
                if (Exception != null)
                    return false;

                var state = new StateObject(ref socket, clientType);
                state.SetReceiveCallback(new AsyncCallback(ReceiveCallback));

                return true;
            }
            catch (SocketException e)
            {
                logger.Error("[NetworkComponent] {message}", e.Message);
                if (socket == null)
                    throw new CommunicationErrorException(CommunicationExceptionType.SocketNotCreated, e);

                throw new CommunicationErrorException(CommunicationExceptionType.InvalidSocket, e);
            }
            catch (ObjectDisposedException e)
            {
                logger.Error("[NetworkComponent] {message}", e.Message);
                throw new CommunicationErrorException(CommunicationExceptionType.InvalidSocket, e);
            }
        }

        public bool Disconnect()
        {
            try
            {
                socket?.Shutdown(SocketShutdown.Both);
                socket?.Close();

                logger.Info("Connection with CommunicationServer has been closed");
                return true;
            }
            catch (ObjectDisposedException e)
            {
                return true;
            }
            catch (SocketException e)
            {
                logger.Error("[NetworkComponent] {message}", e.Message);
                throw new CommunicationErrorException(CommunicationExceptionType.InvalidSocket, e);
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
            }
            catch (Exception e)
            {
                logger.Error("[NetworkComponent] {message}", e.Message);
                Exception = new CommunicationErrorException(CommunicationExceptionType.InvalidSocket, e);
            }
            finally
            {
                connectDone.Set();
            }
        }

        private void Send(Socket client, byte[] message)
        {
            try
            {
                if (message != null && message.Length > 0)
                    client.Send(message, message.Length, SocketFlags.None);
                else
                    throw new CommunicationErrorException(CommunicationExceptionType.InvalidMessageSize);
            }
            catch (CommunicationErrorException e)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new CommunicationErrorException(CommunicationExceptionType.InvalidSocket, e);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var client = state.WorkSocket;
            int bytesRead = 0;

            try
            {
                bytesRead = client.EndReceive(ar);
            }
            catch (Exception e)
            {
                Exception = new CommunicationErrorException(CommunicationExceptionType.CommunicationServerDisconnected, e);
                return;
            }

            if (bytesRead > 2)
            {
                try
                {
                    foreach (var message in MessageSerializer.UnwrapAndDeserializeMessages(state.Buffer, bytesRead))
                    {
                        messageQueue.Enqueue(message);
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    logger.Warn("[NetworkComponent] {message}", e.Message);
                }
                state.SetReceiveCallback(new AsyncCallback(ReceiveCallback));
            }
            else if (bytesRead > 0)
            {
                logger.Warn("Received message was too short (expected more than 2 bytes)");
                state.SetReceiveCallback(new AsyncCallback(ReceiveCallback));
            }
            else if (socket.Poll(100, SelectMode.SelectWrite) && socket.Available == 0)
            {
                Exception = new CommunicationErrorException(CommunicationExceptionType.CommunicationServerDisconnected);
            }
            else
            {
                Exception = new CommunicationErrorException(CommunicationExceptionType.InvalidSocket);
            }
        }
    }
}
