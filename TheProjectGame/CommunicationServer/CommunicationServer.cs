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
        // TODO: Implement loading configuration from file
        internal ConfigurationComponent ConfigComponent { get; private set; }
        internal NetworkComponent NetworkComponent { get; private set; }

        internal IPAddress IPAddress { get; private set; }
        internal HostMapping HostMapping { get; private set; }

        private ConcurrentQueue<ReceivedMessage> messageQueue;
        private ManualResetEvent shouldProcessMessage;

        private Socket gameMasterListener;
        private Socket agentListener;

        internal CommunicationServer()
        {
            ConfigComponent = new ConfigurationComponent(null);
            NetworkComponent = new NetworkComponent(this);

            HostMapping = new HostMapping();

            messageQueue = new ConcurrentQueue<ReceivedMessage>();
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
        internal void AddMessage(ReceivedMessage receivedMessage)
        {
            messageQueue.Enqueue(receivedMessage);
            shouldProcessMessage.Set();
        }

        private void ProcessMessages()
        {
            ReceivedMessage message;

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

        private void ProcessMessage(ReceivedMessage receivedMessage)
        {
            // TODO: Implement forwarding message and checking agent in HostMapping
            Console.WriteLine($"Received message from host with id = {HostMapping.GetHostIdForSocket(receivedMessage.SenderSocket)}");
            Console.WriteLine("Content: ");
            Console.WriteLine(receivedMessage.SerializedMessage);
        }
    }
}
