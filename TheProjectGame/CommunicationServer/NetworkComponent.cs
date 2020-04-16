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

        private void StartListener(object obj)
        {
            ExtendedListener listener = (ExtendedListener)obj;

            try
            {
                listener.Listener.Listen(100);

                while (true)
                {
                    listener.Barrier.Reset();
                    // TODO: DEBUG purposes, remove it
                    Console.WriteLine($"Waiting for {listener.ClientType} to connect...");

                    listener.Listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    listener.Barrier.WaitOne();

                    if (listener.ClientType == ClientType.GameMaster)
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

            var state = new ClientStateObject(ref handler, listener.ClientType);
            state.SetReadCallback(new AsyncCallback(ReadCallback));
            Console.WriteLine($"{listener.ClientType} connected!");
        }

        private void ReadCallback(IAsyncResult ar)
        {
            var state = (ClientStateObject)ar.AsyncState;
            Socket handler = state.WorkSocket;

            int bytesRead = handler.EndReceive(ar);
            int messageLength;
            string message;

            if(bytesRead > 2)
            {
                var littleEndianBytes = new byte[2];
                Array.Copy(state.Buffer, littleEndianBytes, 2);
                if(!BitConverter.IsLittleEndian)
                    Array.Reverse(littleEndianBytes);

                messageLength = BitConverter.ToInt16(littleEndianBytes, 0);

                if (messageLength <= state.BufferSize - 2)
                {
                    message = Encoding.UTF8.GetString(state.Buffer, 2, messageLength);
                    // TODO: Create class ReceivedMessage with sender, receipent and seralizedMessage
                    server.AddMessage(message);
                }
                else
                {
                    Console.WriteLine($"Received message was too long (expected maximum {state.BufferSize}, got {messageLength + 2})");
                }
            }
            else if(bytesRead > 0)
            {
                Console.WriteLine("Received message was too short (expected more than 2 bytes)");
            }

            state.SetReadCallback(new AsyncCallback(ReadCallback));
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