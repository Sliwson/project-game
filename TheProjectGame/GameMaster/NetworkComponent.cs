using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GameMaster
{
    public class NetworkComponent
    {
        private GameMaster gameMaster;
        private IPEndPoint communicationServerEndpoint;

        private ManualResetEvent connectDone;
        private ManualResetEvent sendDone;

        public NetworkComponent(GameMaster gameMaster)
        {
            this.gameMaster = gameMaster;

            var ipAddress = IPAddress.Parse(gameMaster.Configuration.CsIP);
            communicationServerEndpoint = new IPEndPoint(ipAddress, gameMaster.Configuration.CsPort);

            connectDone = new ManualResetEvent(false);
            sendDone = new ManualResetEvent(false);
        }

        public void Connect()
        {
            
            var serializedMessage = "Test message";
            var byteMessage = Encoding.UTF8.GetBytes(serializedMessage);
            byte[] message = BitConverter.GetBytes((short)byteMessage.Length);
            Array.Resize(ref message, byteMessage.Length + 2);
            Array.Copy(byteMessage, 0, message, 2, byteMessage.Length);

            try
            {
                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                client.BeginConnect(communicationServerEndpoint, new AsyncCallback(ConnectCallback), client);

                connectDone.WaitOne();


                Send(client, message);
                sendDone.WaitOne();

                Send(client, message);
                sendDone.WaitOne();

                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
            Console.WriteLine("Closed");
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
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
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
