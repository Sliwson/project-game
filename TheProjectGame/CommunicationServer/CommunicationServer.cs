using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace CommunicationServer
{
    internal class CommunicationServer
    {
        internal IPAddress IPAddress { get; private set; }
        // TODO: Implement loading configuration from file
        internal ConfigurationComponent ConfigComponent { get; private set; }
        internal NetworkComponent NetworkComponent { get; private set; }

        private TcpListener gameMasterListener;
        private TcpListener agentListener;

        internal CommunicationServer()
        {
            ConfigComponent = new ConfigurationComponent(null);
            NetworkComponent = new NetworkComponent(this);
        }

        internal void Run()
        {
            try
            {
                IPAddress = IPAddress.Parse(NetworkComponent.GetLocalIPAddress());

                agentListener = new TcpListener(IPAddress, ConfigComponent.GetAgentPort());
                gameMasterListener = new TcpListener(IPAddress, ConfigComponent.GetGameMasterPort());
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine($"All ports have to be in range ({IPEndPoint.MinPort}, {IPEndPoint.MaxPort}), was: {ConfigComponent.GetAgentPort()} and {ConfigComponent.GetGameMasterPort()}");
                throw;
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to find IP address for Communication Server");
                throw;
            }

            NetworkComponent.StartListening(gameMasterListener, agentListener);
        }
    }
}
