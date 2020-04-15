using GameMaster;
using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.Errors;
using Messaging.Contracts.GameMaster;
using Messaging.Enumerators;
using Messaging.Implementation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

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
            gameMaster = new GameMaster.GameMaster();
            connectionLogic = gameMaster.ConnectionLogic;
            configuration = gameMaster.Configuration;
        }

        [Test]
        public void ProcessMessage_ShouldRejectAndBanMessagesDifferentThanJoinRequest()
        {
            connectionLogic.FlushLobby();

            var message = MessageFactory.GetMessage(new MoveRequest(Direction.East), 0);
            dynamic response = connectionLogic.ProcessMessage(message);

            Assert.AreEqual(message.AgentId, response.AgentId);
            Assert.IsTrue(response.Payload is IErrorPayload);

            var joinMessage = MessageFactory.GetMessage(new JoinRequest(TeamId.Blue, false), 0);
            response = connectionLogic.ProcessMessage(joinMessage);

            Assert.AreEqual(joinMessage.AgentId, response.AgentId);
            Assert.IsTrue(response.Payload is IErrorPayload);
        }

        [Test]
        public void ConnectionLogic_ShouldAcceptAgent()
        {
            connectionLogic.FlushLobby();
            var message = MessageFactory.GetMessage(new JoinRequest(TeamId.Blue, true), 0);
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
            var message = MessageFactory.GetMessage(new JoinRequest(TeamId.Blue, true), 0);
            connectionLogic.ProcessMessage(message);

            message = MessageFactory.GetMessage(new JoinRequest(TeamId.Blue, true), 1);
            dynamic response = connectionLogic.ProcessMessage(message);

            Assert.AreEqual(message.AgentId, response.AgentId);
            Assert.IsTrue(response.Payload is JoinResponse);

            var castedResponse = response as Message<JoinResponse>;
            Assert.IsFalse(castedResponse.Payload.Accepted);
            Assert.AreEqual(message.AgentId, castedResponse.Payload.AgentId);
            Assert.IsTrue(connectionLogic.FlushLobby().Count == 1);
        }
    }
}
