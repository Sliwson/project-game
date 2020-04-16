using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

namespace CommunicationServer
{
    internal class CommunicationServer
    {
        internal IPAddress IPAddress { get; private set; }
        // TODO: Implement loading configuration from file
        internal ConfigurationComponent ConfigComponent { get; private set; }
        internal NetworkComponent NetworkComponent { get; private set; }

        private ConcurrentQueue<string> messageQueue;
        private ManualResetEvent shouldProcessMessage;

        private Socket gameMasterListener;
        private Socket agentListener;

        internal CommunicationServer()
        {
            ConfigComponent = new ConfigurationComponent(null);
            NetworkComponent = new NetworkComponent(this);

            messageQueue = new ConcurrentQueue<string>();
            shouldProcessMessage = new ManualResetEvent(false);
        }

        internal void Run()
        {
            try
            {
                IPAddress = NetworkComponent.GetLocalIPAddress();

                gameMasterListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);// ConfigComponent.GetGameMasterPort());
                agentListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine($"All ports have to be in range ({IPEndPoint.MinPort}, {IPEndPoint.MaxPort}), was: {ConfigComponent.GetAgentPort()} and {ConfigComponent.GetGameMasterPort()}");
                throw;
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to create socket");
                throw;
            }

            NetworkComponent.StartListening(gameMasterListener, agentListener);

            ProcessMessages();
        }

        // Call this method from other threads
        internal void AddMessage(string serializedMessage)
        {
            messageQueue.Enqueue(serializedMessage);
            shouldProcessMessage.Set();
        }

        private void ProcessMessages()
        {
            string message;

            while(true)
            {
                shouldProcessMessage.WaitOne();

                if(messageQueue.TryDequeue(out message))
                {
                    ProcessMessage(message);
                    shouldProcessMessage.Reset();
                }
            }
        }

        private void ProcessMessage(string serializedMessage)
        {
            // TODO: Implement forwarding message and checking agent in HostMapping
            Console.Write(serializedMessage);
        }
    }
}
