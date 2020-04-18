using Messaging.Contracts;
using Messaging.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Agent
{
    public class NetworkComponent
    {
        private Agent agent;

        private IPEndPoint communicationServerEndpoint;
        private Socket socket;

        private ManualResetEvent connectDone;
        private ManualResetEvent sendDone;

        public NetworkComponent(Agent agent)
        {
            this.agent = agent;

            var ipAddress = IPAddress.Parse(agent.AgentConfiguration.CsIP);
            communicationServerEndpoint = new IPEndPoint(ipAddress, agent.AgentConfiguration.CsPort);

            connectDone = new ManualResetEvent(false);
            sendDone = new ManualResetEvent(false);
        }

        public bool Connect()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.BeginConnect(communicationServerEndpoint, new AsyncCallback(ConnectCallback), socket);
                connectDone.WaitOne();

                var state = new ServerStateObject(ref socket);
                state.SetReceiveCallback(new AsyncCallback(ReceiveCallback));

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
                return false;
            }
        }

        public bool Disconnect()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                Console.WriteLine("Closed");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to disconnect: {0}", e);
                return false;
            }
        }

        public void SendMessage(BaseMessage message)
        {
            var wrappedMessage = MessageSerializer.SerializeAndWrapMessage(message);

            Send(socket, wrappedMessage);
            sendDone.WaitOne();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, byte[] message)
        {
            client.BeginSend(message, 0, message.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var state = (ServerStateObject)ar.AsyncState;
            var client = state.WorkSocket;

            int bytesRead = client.EndReceive(ar);
            BaseMessage message;

            if (bytesRead > 2)
            {
                try
                {
                    message = MessageSerializer.UnwrapAndDeserializeMessage(state.Buffer);

                    agent.InjectMessage(message);
                }
                catch (Exception e)
                {
                    if (e is ArgumentOutOfRangeException)
                        Console.WriteLine(e.Message);
                    else
                        throw;
                }
                state.SetReceiveCallback(new AsyncCallback(ReceiveCallback));
            }
            else if (bytesRead > 0)
            {
                Console.WriteLine("Received message was too short (expected more than 2 bytes)");
                state.SetReceiveCallback(new AsyncCallback(ReceiveCallback));
            }
        }
    }
    }
