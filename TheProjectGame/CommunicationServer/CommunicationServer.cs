using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using Messaging.Serialization;
using Messaging.Communication;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CommunicationServerTests")]

namespace CommunicationServer
{
    public class CommunicationServer
    {
        public CommunicationServerConfiguration Configuration { get; private set; }
        internal NetworkComponent NetworkComponent { get; private set; }

        internal IPAddress IPAddress { get; private set; }
        internal HostMapping HostMapping { get; private set; }

        private ConcurrentQueue<ReceivedMessage> messageQueue;
        private ManualResetEvent shouldProcessMessage;

        private Socket gameMasterListener;
        private Socket agentListener;

        private bool shouldTerminate = false;
        private Exception internalException;

        public CommunicationServer(CommunicationServerConfiguration configuration)
        {
            Configuration = configuration;
            NetworkComponent = new NetworkComponent(this);

            HostMapping = new HostMapping();

            messageQueue = new ConcurrentQueue<ReceivedMessage>();
            shouldProcessMessage = new ManualResetEvent(false);
        }

        public void Run()
        {
            try
            {
                IPAddress = NetworkComponent.GetLocalIPAddress();

                gameMasterListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                gameMasterListener.NoDelay = true;

                agentListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                agentListener.NoDelay = true;
            }
            catch (SocketException ex)
            {
                throw new CommunicationErrorException(CommunicationExceptionType.SocketNotCreated, ex);
            }

            NetworkComponent.StartListening(gameMasterListener, agentListener);

            ProcessMessages();
        }

        public void OnDestroy()
        {
            NetworkComponent?.Disconnect();
        }

        // Call this method from other threads
        internal void AddMessage(ReceivedMessage receivedMessage)
        {
            messageQueue.Enqueue(receivedMessage);
            shouldProcessMessage.Set();
        }

        // Call this method from other threads
        internal bool CheckIfClientDisconnected(Socket socket)
        {
            if(socket.Poll(100, SelectMode.SelectWrite) && socket.Available == 0)
            {
                var disconectedId = HostMapping.GetHostIdForSocket(socket);
                if(HostMapping.IsHostGameMaster(disconectedId))
                {
                    RaiseException(new CommunicationErrorException(CommunicationExceptionType.GameMasterDisconnected));
                }

                return true;
            }

            return false;
        }

        // Call this method from other threads
        internal void RaiseException(Exception exception)
        {
            internalException = exception;
            shouldTerminate = true;
            shouldProcessMessage.Set();
        }

        private void ProcessMessages()
        {
            ReceivedMessage message;

            while(true)
            {
                shouldProcessMessage.WaitOne();
                if (shouldTerminate && internalException != null)
                    throw internalException;

                while (messageQueue.TryDequeue(out message))
                {
                    ProcessMessage(message);
                    shouldProcessMessage.Reset();
                }
            }
        }

        private void ProcessMessage(ReceivedMessage receivedMessage)
        {
            try
            {
                var senderHostId = HostMapping.GetHostIdForSocket(receivedMessage.SenderSocket);
                int receipentHostId;

                Console.WriteLine($"Received message from host with id = {senderHostId}");
                var deserializedMessage = MessageSerializer.DeserializeMessage(receivedMessage.SerializedMessage);
                Console.WriteLine($"Message type = {deserializedMessage.MessageId} (id = {(int)deserializedMessage.MessageId})");

                if (!HostMapping.IsHostGameMaster(senderHostId))
                {
                    // TODO: Decide what should be done if GameMaster has not connected yet (NoGameMaster)
                    receipentHostId = HostMapping.GetGameMasterHostId();
                    deserializedMessage.SetAgentId(senderHostId);
                }
                else
                {
                    receipentHostId = deserializedMessage.AgentId;
                }

                Console.WriteLine($"\nForwarding to host with id = {receipentHostId}");

                var receipentSocket = HostMapping.GetSocketForHostId(receipentHostId);

                NetworkComponent.SendMessage(receipentSocket, deserializedMessage);
            }
            catch(JsonSerializationException e)
            {
                Console.WriteLine(e.Message);
            }
            catch(CommunicationErrorException e)
            {
                Console.WriteLine(e.Message);

                // Socket has been closed or is invalid
                if(e.Type == CommunicationExceptionType.InvalidSocket)
                {
                    if (receivedMessage.SenderSocket == null)
                        return;

                    var senderHostId = HostMapping.GetHostIdForSocket(receivedMessage.SenderSocket);

                    // Message should be sent to GameMaster but error occured - abort
                    if (!HostMapping.IsHostGameMaster(senderHostId))
                        throw;
                }
            }
        }
    }
}
