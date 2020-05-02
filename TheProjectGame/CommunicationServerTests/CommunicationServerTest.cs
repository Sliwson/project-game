using CommunicationServer;
using NUnit.Framework;

namespace CommunicationServerTests
{
    public class CommunicationServerTest
    {
        CommunicationServer.CommunicationServer server;

        [SetUp]
        public void Setup()
        {
            server = new CommunicationServer.CommunicationServer();
        }

        // TODO (#IO-57): Implement injecting messages in CS without connecting clients
        [Test, Ignore("Mocked network component for CS needs to be implemented first")]
        public void ProcessMessage_ShouldIgnoreMessageWhenInvalid()
        {
            Assert.Pass();
        }

        // TODO (#IO-57): Implement injecting messages in CS without connecting clients
        [Test, Ignore("Mocked network component for CS needs to be implemented first")]
        public void ProcessMessage_ShouldIgnoreMessageWhenGameMasterNotConnected()
        {
            Assert.Pass();
        }

        // TODO (#IO-57): Implement injecting messages in CS without connecting clients
        [Test, Ignore("Mocked network component for CS needs to be implemented first")]
        public void ProcessMessage_ShouldIgnoreMessageToAgentIfNotAvaliable()
        {
            Assert.Pass();
        }

        // TODO (#IO-57): Implement injecting messages in CS without connecting clients
        [Test, Ignore("Mocked network component for CS needs to be implemented first")]
        public void ProcessMessage_ShouldThrowIfGameMasterNotAvaliable()
        {
            Assert.Pass();
        }
    }
}