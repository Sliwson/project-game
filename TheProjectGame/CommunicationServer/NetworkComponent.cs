using Messaging.Communication;
using Messaging.Contracts;
using Messaging.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CommunicationServer
{
    internal class NetworkComponent
    {
        private CommunicationServer server;

        private ManualResetEvent gmAccepted = new ManualResetEvent(false);
        private ManualResetEvent agentAccepted = new ManualResetEvent(false);

        internal NetworkComponent(CommunicationServer communicationServer)
        {
            server = communicationServer;
        }

        internal void StartListening(Socket gameMasterListener, Socket agentListener)
        {
            var gameMasterEndpoint = new IPEndPoint(server.IPAddress, server.ConfigComponent.GetGameMasterPort());
            var agentEndpoint = new IPEndPoint(server.IPAddress, server.ConfigComponent.GetAgentPort());

            var extendedGameMasterListener = new ExtendedListener(gameMasterListener, ClientType.GameMaster, ref gmAccepted);
            var extendedAgentListener = new ExtendedListener(agentListener, ClientType.Agent, ref agentAccepted);

            try
            {
                gameMasterListener.Bind(gameMasterEndpoint);
                agentListener.Bind(agentEndpoint);

                Thread gameMasterThread = new Thread(new ParameterizedThreadStart(StartListener));
                gameMasterThread.Start(extendedGameMasterListener);
                Console.WriteLine($"Server for GameMaster was started with IP: {server.IPAddress}:{server.ConfigComponent.GetGameMasterPort()}");

                Thread agentsThread = new Thread(new ParameterizedThreadStart(StartListener));
                agentsThread.Start(extendedAgentListener);
                Console.WriteLine($"Server for Agent was started with IP: {server.IPAddress}:{server.ConfigComponent.GetAgentPort()}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        internal void SendMessage(Socket handler, BaseMessage message)
        {
            var messageData = MessageSerializer.SerializeAndWrapMessage(message);
            
            handler.BeginSend(messageData, 0, messageData.Length, SocketFlags.None, new AsyncCallback(SendCallback), handler);
        }

        private void StartListener(object obj)
        {
            ExtendedListener listener = (ExtendedListener)obj;

            try
            {
                listener.Listener.Listen(100);

                while (true)
                {
                    listener.Barrier.Reset();
                    listener.Listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    listener.Barrier.WaitOne();

                    if (listener.ClientType != ClientType.Agent)
                        break;
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                throw;
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            var listener = (ExtendedListener)ar.AsyncState;

            listener.Barrier.Set();
            var handler = listener.Listener.EndAccept(ar);
            var hostId = server.HostMapping.AddClientToMapping(listener.ClientType, handler);

            var state = new StateObject(ref handler, listener.ClientType);
            state.SetReceiveCallback(new AsyncCallback(ReceiveCallback));

            Console.WriteLine($"{listener.ClientType} connected!");
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            Socket handler = state.WorkSocket;

            int bytesRead = handler.EndReceive(ar);
            string message;

            if(bytesRead > 2)
            {
                try
                {
                    message = MessageSerializer.UnwrapMessage(state.Buffer);
                    var receivedMessage = new ReceivedMessage(handler, message);

                    server.AddMessage(receivedMessage);
                }
                catch(Exception e)
                {
                    if (e is ArgumentOutOfRangeException)
                        Console.WriteLine(e.Message);
                    else
                        throw;
                }
                state.SetReceiveCallback(new AsyncCallback(ReceiveCallback));
            }
            else if(bytesRead > 0)
            {
                Console.WriteLine("Received message was too short (expected more than 2 bytes)");
                state.SetReceiveCallback(new AsyncCallback(ReceiveCallback));
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                Console.WriteLine($"Sent {bytesSent} bytes to client");

                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        internal IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new ArgumentNullException("No network adapters with an IPv4 address in the system!");
        }
    }
}