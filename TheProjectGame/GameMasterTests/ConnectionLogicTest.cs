using GameMaster;
using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.Errors;
using Messaging.Contracts.GameMaster;
using Messaging.Enumerators;
using Messaging.Implementation;
using NUnit.Framework;

namespace GameMasterTests
{
    class ConnectionLogicTest
    {
        private GameMaster.GameMaster gameMaster;
        private ConnectionLogicComponent connectionLogic;
        private GameMasterConfiguration configuration;

        [SetUp]
        public void Setup()
        {
            configuration = GameMasterConfiguration.GetDefault();
            gameMaster = new GameMaster.GameMaster(configuration);
            connectionLogic = gameMaster.ConnectionLogic;
        }

        [Test]
        public void ProcessMessage_ShouldRejectAndBanAgentsThatSendMessagesDifferentThanJoinRequest()
        {
            connectionLogic.FlushLobby();

            var message = MessageFactory.GetMessage(new MoveRequest(Direction.East), 0);
            dynamic response = connectionLogic.ProcessMessage(message);

            Assert.AreEqual(message.AgentId, response.AgentId);
            Assert.IsTrue(response.Payload is IErrorPayload);

            var joinMessage = MessageFactory.GetMessage(new JoinRequest(TeamId.Blue), 0);
            response = connectionLogic.ProcessMessage(joinMessage);

            Assert.AreEqual(joinMessage.AgentId, response.AgentId);
            Assert.IsTrue(response.Payload is IErrorPayload);
        }

        [Test]
        public void ConnectionLogic_ShouldAcceptAgent()
        {
            connectionLogic.FlushLobby();
            var message = MessageFactory.GetMessage(new JoinRequest(TeamId.Blue), 0);
            dynamic response = connectionLogic.ProcessMessage(message);

            Assert.AreEqual(message.AgentId, response.AgentId);
            Assert.IsTrue(response.Payload is JoinResponse);

            var castedResponse = response as Message<JoinResponse>;
            Assert.IsTrue(castedResponse.Payload.Accepted);
            Assert.AreEqual(message.AgentId, castedResponse.Payload.AgentId);
            Assert.IsTrue(connectionLogic.FlushLobby().Count == 1);
        }

        [Test]
        public void ConnectionLogic_ShouldAcceptOnlyOneTeamLeader()
        {
            connectionLogic.FlushLobby();
            var message0 = MessageFactory.GetMessage(new JoinRequest(TeamId.Blue), 0);
            dynamic response0 = connectionLogic.ProcessMessage(message0);

            var message1 = MessageFactory.GetMessage(new JoinRequest(TeamId.Blue), 1);
            dynamic response1 = connectionLogic.ProcessMessage(message1);

            Assert.AreEqual(message1.AgentId, response1.AgentId);
            Assert.IsTrue(response1.Payload is JoinResponse);

            var castedResponse = response0 as Message<JoinResponse>;
            Assert.IsTrue(castedResponse.Payload.Accepted);
            Assert.AreEqual(castedResponse.AgentId, castedResponse.Payload.AgentId);

            castedResponse = response1 as Message<JoinResponse>;
            Assert.IsTrue(castedResponse.Payload.Accepted);
            Assert.AreEqual(castedResponse.AgentId, castedResponse.Payload.AgentId);

            var agents = connectionLogic.FlushLobby();
            Assert.AreEqual(2, agents.Count);
            Assert.IsTrue(agents[0].IsTeamLeader);
            Assert.IsFalse(agents[1].IsTeamLeader);
        }

        [Test]
        public void ConnectionLogic_ShouldIgnoreSecondJoinRequestFromTheSameAgent()
        {
            connectionLogic.FlushLobby();
            var message = MessageFactory.GetMessage(new JoinRequest(TeamId.Blue), 0);
            connectionLogic.ProcessMessage(message);

            message = MessageFactory.GetMessage(new JoinRequest(TeamId.Blue), 0);
            dynamic response = connectionLogic.ProcessMessage(message);

            Assert.AreEqual(message.AgentId, response.AgentId);
            Assert.IsTrue(response.Payload is JoinResponse);

            var castedResponse = response as Message<JoinResponse>;
            Assert.IsTrue(castedResponse.Payload.Accepted);
            Assert.AreEqual(message.AgentId, castedResponse.Payload.AgentId);
            Assert.IsTrue(connectionLogic.FlushLobby().Count == 1);
        }

        [Test]
        public void ConnectionLogic_ShouldAcceptNoMoreAgentsThanItIsConfigured()
        {
            connectionLogic.FlushLobby();
            var teamLimit = configuration.TeamSize;

            for (int i = 0; i < teamLimit; i++)
            {
                dynamic response =  connectionLogic.ProcessMessage(MessageFactory.GetMessage(new JoinRequest(TeamId.Blue), i));
                Assert.AreEqual(i, response.AgentId);
                Assert.IsTrue(response.Payload is JoinResponse);
                var joinResponse = response.Payload as JoinResponse;
                Assert.IsTrue(joinResponse.Accepted);
            }

            dynamic rejectResponse = connectionLogic.ProcessMessage(MessageFactory.GetMessage(new JoinRequest(TeamId.Blue), teamLimit));
            Assert.AreEqual(teamLimit, rejectResponse.AgentId);
            var joinResponseReject = rejectResponse.Payload as JoinResponse;
            Assert.IsFalse(joinResponseReject.Accepted);
        }
    }
}
