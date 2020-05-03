using CommunicationServer;
using NUnit.Framework;

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

        // TODO (#IO-56): Allow  mocking configuration
        [Test, Ignore("Test-specific mock configuration needs to be implemented first")]
        public void StartListening_ShouldThrowExceptionWhenAgentAndGameMasterOnSamePort()
        {
            Assert.Pass();
        }

        [Test]
        public void GetLocalIPAddress_ShouldNotThrowException()
        {
            var ip = server.NetworkComponent.GetLocalIPAddress();

            Assert.NotNull(ip);
        }

        [Test]
        public void SendMessage_ShouldThrowExceptionWhenInvalidMessage()
        {
            
        }

        [Test]
        public void SendMessage_ShouldThrowExceptionWhenSocketIsClosed()
        {

        }

        [Test]
        public void NetworkComponent_ShouldIgnoreSecondGameMaster()
        {

        }

    }
}