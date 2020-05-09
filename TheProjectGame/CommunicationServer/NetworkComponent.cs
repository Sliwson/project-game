using Messaging.Communication;
using Messaging.Contracts;
using Messaging.Serialization;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationServer
{
    internal class NetworkComponent
    {
        private CommunicationServer server;

        private ManualResetEvent gmAccepted = new ManualResetEvent(false);
        private ManualResetEvent agentAccepted = new ManualResetEvent(false);

        private Socket gameMasterSocket;
        private Socket agentSocket;

        private static Logger logger = LogManager.GetCurrentClassLogger(); 

        internal NetworkComponent(CommunicationServer communicationServer)
        {
            server = communicationServer;
        }

        internal void StartListening(Socket gameMasterListener, Socket agentListener)
        {
            gameMasterSocket = gameMasterListener;
            agentSocket = agentListener;

            var gameMasterPort = server.Configuration.GameMasterPort;
            var agentPort = server.Configuration.AgentPort;

            var gameMasterEndpoint = new IPEndPoint(server.IPAddress, gameMasterPort);
            var agentEndpoint = new IPEndPoint(server.IPAddress, agentPort);

            var extendedGameMasterListener = new ExtendedListener(gameMasterListener, ClientType.GameMaster, ref gmAccepted);
            var extendedAgentListener = new ExtendedListener(agentListener, ClientType.Agent, ref agentAccepted);

            try
            {
                gameMasterListener.Bind(gameMasterEndpoint);
                agentListener.Bind(agentEndpoint);

                var gameMasterTask = new Task(StartListener, extendedGameMasterListener);
                gameMasterTask.Start();

                var agentsTask = new Task(StartListener, extendedAgentListener);
                agentsTask.Start();
            }
            catch (ArgumentNullException e)
            {
                throw new CommunicationErrorException(CommunicationExceptionType.InvalidEndpoint, e);
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new CommunicationErrorException(CommunicationExceptionType.InvalidEndpoint, e);
            }
            catch (SocketException e)
            {
                throw new CommunicationErrorException(CommunicationExceptionType.SocketNotCreated, e);
            }
            catch (ObjectDisposedException e)
            {
                throw new CommunicationErrorException(CommunicationExceptionType.SocketNotCreated, e);
            }
        }

        internal void SendMessage(Socket handler, BaseMessage message)
        {
            var messageData = MessageSerializer.SerializeAndWrapMessage(message);

            try
            {
                handler.BeginSend(messageData, 0, messageData.Length, SocketFlags.None, new AsyncCallback(SendCallback), handler);
            }
            catch (Exception e)
            {
                throw new CommunicationErrorException(CommunicationExceptionType.InvalidSocket, e);
            }
        }

        internal void Disconnect()
        {
            Disconnect(ClientType.GameMaster);
            Disconnect(ClientType.Agent);
        }

        internal void Disconnect(ClientType clientType)
        {
            var socket = clientType == ClientType.GameMaster ? gameMasterSocket : agentSocket;

            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (ObjectDisposedException)
            {
                // disposed earlier, that's fine
            }
            catch (Exception ex)
            {
                logger.Error("[NetworkComponent] {message}", ex.Message);
            }
            finally
            {
                logger.Info("[NetworkComponent] Socket for {clientType} has been closed", clientType);
            }
        }

        private void StartListener(object obj)
        {
            ExtendedListener listener = (ExtendedListener)obj;

            try
            {
                listener.Listener.Listen(100);
                logger.Info("[NetworkComponent] Server for {type} was started with IP: {ip}", listener.ClientType, listener.Listener.LocalEndPoint);

                while (true)
                {
                    listener.Barrier.Reset();
                    listener.Listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    listener.Barrier.WaitOne();

                    if (listener.ClientType == ClientType.GameMaster)
                        break;
                }
            }
            catch (SocketException e)
            {
                server.RaiseException(new CommunicationErrorException(CommunicationExceptionType.SocketNotCreated, e));
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            var listener = (ExtendedListener)ar.AsyncState;

            listener.Barrier.Set();
            try
            {
                var handler = listener.Listener.EndAccept(ar);
                handler.NoDelay = true;

                var hostId = server.HostMapping.AddClientToMapping(listener.ClientType, handler);

                var state = new StateObject(ref handler, listener.ClientType);
                state.SetReceiveCallback(new AsyncCallback(ReceiveCallback));

                logger.Info("[NetworkComponent] {type} connected", listener.ClientType);
            }
            catch (Exception e)
            {
                server.RaiseException(new CommunicationErrorException(CommunicationExceptionType.InvalidSocket, e));
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            Socket handler = state.WorkSocket;
            int bytesRead = 0;

            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (Exception e)
            {
                server.CheckIfClientDisconnected(handler);
                return;
            }

            if (bytesRead > 2)
            {
                try
                {
                    foreach (var message in MessageSerializer.UnwrapMessages(state.Buffer, bytesRead))
                    {
                        var receivedMessage = new ReceivedMessage(handler, message);

                        server.AddMessage(receivedMessage);
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
                logger.Warn("[NetworkComponent] Received message was too short (expected more than 2 bytes)");
                state.SetReceiveCallback(new AsyncCallback(ReceiveCallback));
            }
            else if (!server.CheckIfClientDisconnected(handler))
            {
                state.SetReceiveCallback(new AsyncCallback(ReceiveCallback));
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                logger.Debug("[NetworkComponent] Sent {bytesSent} bytes to client\n", bytesSent);
            }
            catch (Exception e)
            {
                server.RaiseException(new CommunicationErrorException(CommunicationExceptionType.InvalidSocket, e));
            }
        }

        internal IPAddress GetLocalIPAddress()
        {
            return IPAddress.Parse("127.0.0.1");

            //// Skip Virtual Machines' IP addresses
            //// https://stackoverflow.com/questions/8089685/c-sharp-finding-my-machines-local-ip-address-and-not-the-vms
            ////
            //foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            //{
            //    var addr = ni.GetIPProperties().GatewayAddresses.FirstOrDefault();
            //    if (addr != null && !addr.Address.ToString().Equals("0.0.0.0"))
            //    {
            //        if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 
            //            || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            //        {
            //            foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
            //            {
            //                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
            //                {
            //                    return ip.Address;
            //                }
            //            }
            //        }
            //    }
            //}
            //throw new CommunicationErrorException(CommunicationExceptionType.NoIpAddress);
        }
    }
}