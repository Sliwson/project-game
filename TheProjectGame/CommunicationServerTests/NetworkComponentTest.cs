using CommunicationServer;
using Messaging.Communication;
using Messaging.Contracts.Agent;
using Messaging.Enumerators;
using Messaging.Implementation;
using NUnit.Framework;
using System.Net.Sockets;

namespace CommunicationServerTests
{
    public class NetworkComponentTest
    {
        CommunicationServer.CommunicationServer server;

        [SetUp]
        public void Setup()
        {
            var config = CommunicationServerConfiguration.GetDefault();
            server = new CommunicationServer.CommunicationServer(config);
        }

        [Test]
        public void StartListening_ShouldThrowExceptionWhenAgentAndGameMasterOnSamePort()
        {
            var config = CommunicationServerConfiguration.GetDefault();
            config.AgentPort = 55555;
            config.GameMasterPort = 55555;

            server = new CommunicationServer.CommunicationServer(config);
            var exception = Assert.Throws<CommunicationErrorException>(() => server.Run());
            Assert.IsNotNull(exception);
            Assert.AreEqual(CommunicationExceptionType.SocketNotCreated, exception.Type);
            Assert.AreNotEqual("", exception.Message);
        }

        [Test]
        public void GetLocalIPAddress_ShouldNotThrowException()
        {
            var ip = server.NetworkComponent.GetLocalIPAddress();

            Assert.NotNull(ip);
        }

        [Test]
        public void SendMessage_ShouldThrowExceptionWhenSocketIsClosed()
        {
            var gmSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var gmId = server.HostMapping.AddClientToMapping(ClientType.GameMaster, gmSocket);

            var agentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var agentId = server.HostMapping.AddClientToMapping(ClientType.Agent, agentSocket);

            var testMessage = MessageFactory.GetMessage(new JoinRequest(TeamId.Red), agentId);

            var exception = Assert.Throws<CommunicationErrorException>(() => server.NetworkComponent.SendMessage(gmSocket, testMessage));
            Assert.IsNotNull(exception);
            Assert.AreEqual(CommunicationExceptionType.InvalidSocket, exception.Type);
            Assert.AreNotEqual("", exception.Message);
        }
    }
}