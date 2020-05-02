using CommunicationServer;
using Messaging.Communication;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace CommunicationServerTests
{
    public class HostMappingTest
    {
        HostMapping hostMapping;
        IPEndPoint endpoint;

        [SetUp]
        public void Setup()
        {
            hostMapping = new HostMapping();
            endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 54321);
        }

        [Test]
        public void GetHostIdForSocket_ShouldReturnIdForAgentIfRegistered()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var registeredId = hostMapping.AddClientToMapping(ClientType.Agent, socket);
            var returnedId = hostMapping.GetHostIdForSocket(socket);

            Assert.AreEqual(registeredId, returnedId);
        }

        [Test]
        public void GetHostIdForSocket_ShouldReturnIdForGameMasterIfRegistered()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var registeredId = hostMapping.AddClientToMapping(ClientType.GameMaster, socket);
            var returnedId = hostMapping.GetHostIdForSocket(socket);

            Assert.AreEqual(registeredId, returnedId);
        }

        [Test]
        public void GetHostIdForSocket_ShouldReturnDifferentIdsForDifferentSockets()
        {
            var clientType = ClientType.GameMaster;
            List<Socket> sockets = new List<Socket>();
            for(int i = 0; i < 10; i++)
            {
                sockets.Add(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));

                hostMapping.AddClientToMapping(clientType, sockets[i]);
                clientType = ClientType.Agent;

                if (i > 0)
                {
                    var previousHostId = hostMapping.GetHostIdForSocket(sockets[i - 1]);
                    var newHostId = hostMapping.GetHostIdForSocket(sockets[i]);

                    Assert.AreNotEqual(previousHostId, newHostId);
                }
            }
        }

        [Test]
        public void GetHostIdForSocket_ShouldRegisterAndReturnIdIfNotRegistered()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var registeredId = hostMapping.GetHostIdForSocket(socket);
            var returnedId = hostMapping.GetHostIdForSocket(socket);

            Assert.AreEqual(registeredId, returnedId);
        }

        [Test]
        public void GetSocketForHostId_ShouldThrowIfHostNotRegistered()
        {
            int id = 0;
            var exception = Assert.Throws<CommunicationErrorException>(() => hostMapping.GetSocketForHostId(id));
            Assert.AreEqual(CommunicationExceptionType.NoClient, exception.Type);
            Assert.AreNotEqual("", exception.Message);
        }

        [Test]
        public void GetSocketForHostId_ShouldReturnSocketIfRegistered()
        {
            var registeredSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var registeredId = hostMapping.AddClientToMapping(ClientType.Agent, registeredSocket);

            var returnedSocket = hostMapping.GetSocketForHostId(registeredId);
            Assert.AreEqual(registeredSocket, returnedSocket);
        }

        [Test]
        public void GetSocketForHostId_ShouldReturnDifferentSocketsForDifferentIDs()
        {
            var clientType = ClientType.GameMaster;
            List<int> ids = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                ids.Add(hostMapping.AddClientToMapping(clientType, socket));
                clientType = ClientType.Agent;

                if (i > 0)
                {
                    var previousSocket = hostMapping.GetSocketForHostId(ids[i - 1]);
                    var newSocket = hostMapping.GetSocketForHostId(ids[i]);

                    Assert.AreNotEqual(previousSocket, newSocket);
                }
            }
        }

        [Test]
        public void IsHostGameMaster_ShouldReturnTrueForGameMaster()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var gmId = hostMapping.AddClientToMapping(ClientType.GameMaster, socket);

            Assert.IsTrue(hostMapping.IsHostGameMaster(gmId));
        }

        [Test]
        public void IsHostGameMaster_ShouldReturnFalseForAgent()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var agentId = hostMapping.AddClientToMapping(ClientType.Agent, socket);

            Assert.IsFalse(hostMapping.IsHostGameMaster(agentId));
        }

        [Test]
        public void GetGameMasterHostId_ShouldThrowIfGameMasterNotRegistered()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var agentId = hostMapping.AddClientToMapping(ClientType.Agent, socket);

            var exception = Assert.Throws<CommunicationErrorException>(() => hostMapping.GetGameMasterHostId());
            Assert.AreEqual(CommunicationExceptionType.NoGameMaster, exception.Type);
            Assert.AreNotEqual("", exception.Message);
        }

        [Test]
        public void GetGameMasterHostId_ShouldReturnGmHostIdIfRegistered()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var gmId = hostMapping.AddClientToMapping(ClientType.GameMaster, socket);
            var returnedId = hostMapping.GetGameMasterHostId();

            Assert.AreEqual(gmId, returnedId);
        }

        [Test]
        public void AddClientToMapping_ShouldThrowForAgentIfSocketIsNull()
        {
            Socket socket = null;

            var exception = Assert.Throws<CommunicationErrorException>(() => hostMapping.AddClientToMapping(ClientType.Agent, socket));
            Assert.AreEqual(CommunicationExceptionType.InvalidSocket, exception.Type);
            Assert.AreNotEqual("", exception.Message);
        }

        [Test]
        public void AddClientToMapping_ShouldThrowForGameMasterIfSocketIsNull()
        {
            Socket socket = null;

            var exception = Assert.Throws<CommunicationErrorException>(() => hostMapping.AddClientToMapping(ClientType.GameMaster, socket));
            Assert.AreEqual(CommunicationExceptionType.InvalidSocket, exception.Type);
            Assert.AreNotEqual("", exception.Message);
        }

        [Test]
        public void AddClientToMapping_ShouldThrowIfSecondGameMasterIsRequested()
        {
            var actualSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var duplicatedSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var registeredId = hostMapping.AddClientToMapping(ClientType.GameMaster, actualSocket);

            var exception = Assert.Throws<CommunicationErrorException>(() => hostMapping.AddClientToMapping(ClientType.GameMaster, duplicatedSocket));
            Assert.AreEqual(CommunicationExceptionType.DuplicatedGameMaster, exception.Type);
            Assert.AreNotEqual("", exception.Message);

            // Make sure GetHostIdForSocket do not find duplicatedSocket
            exception = Assert.Throws<CommunicationErrorException>(() => hostMapping.GetHostIdForSocket(duplicatedSocket));
        }

        [Test]
        public void AddClientToMapping_ShouldThrowIfSocketAlreadyRegistered()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            hostMapping.AddClientToMapping(ClientType.Agent, socket);
            var exception = Assert.Throws<CommunicationErrorException>(() => hostMapping.AddClientToMapping(ClientType.GameMaster, socket));
        }
    }
}