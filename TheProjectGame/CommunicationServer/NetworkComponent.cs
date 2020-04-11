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

        internal NetworkComponent(CommunicationServer communicationServer)
        {
            server = communicationServer;
        }

        internal string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new ArgumentNullException("No network adapters with an IPv4 address in the system!");
        }

        internal void StartListening(TcpListener gameMasterListener, TcpListener agentListener)
        {
            var extendedGameMasterListener = new ExtendedListener(gameMasterListener, ClientType.GameMaster);
            var extendedAgentListener = new ExtendedListener(agentListener, ClientType.Agent);

            gameMasterListener.Start();
            Thread gameMasterThread = new Thread(new ParameterizedThreadStart(StartListener));
            gameMasterThread.Start(extendedGameMasterListener);
            Console.WriteLine($"Server for agent was started with IP: {server.IPAddress}:{server.ConfigComponent.GetAgentPort()}");

            agentListener.Start();
            Thread agentsThread = new Thread(new ParameterizedThreadStart(StartListener));
            agentsThread.Start(extendedAgentListener);
            Console.WriteLine($"Server for GameMaster was started with IP: {server.IPAddress}:{server.ConfigComponent.GetGameMasterPort()}");
        }

        private void StartListener(object obj)
        {
            ExtendedListener listener = (ExtendedListener)obj;

            try
            {
                while (true)
                {
                    TcpClient client = listener.Listener.AcceptTcpClient();
                    Console.WriteLine($"{listener.ClientType} Connected!");

                    var extendedClient = new ExtendedClient(client, listener.ClientType);

                    Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                    t.Start(extendedClient);

                    if (listener.ClientType == ClientType.GameMaster)
                        break;
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                listener.Listener.Stop();
            }
        }

        // TODO: Modify and finish this method
        private void HandleClient(object obj)
        {
            var client = (ExtendedClient)obj;

            var messageStream = client.Client.GetStream();
            string data;

            var bytes = new byte[1<<13];
            int i;
            try
            {
                while ((i = messageStream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    string hex = BitConverter.ToString(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("{1}: Received: {0}", data, Thread.CurrentThread.ManagedThreadId);
                    string str = "Hey Device!";
                    Byte[] reply = System.Text.Encoding.ASCII.GetBytes(str);
                    messageStream.Write(reply, 0, reply.Length);
                    Console.WriteLine("{1}: Sent: {0}", str, Thread.CurrentThread.ManagedThreadId);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
                client.Client.Close();
            }

        }
    }
}
