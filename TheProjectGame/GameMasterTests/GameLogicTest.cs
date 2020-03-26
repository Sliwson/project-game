using NUnit.Framework;
using GameMaster;
using System;
using System.Collections.Generic;
using System.Text;
using Messaging.Contracts;
using Messaging.Contracts.GameMaster;
using Messaging.Contracts.Agent;
using Messaging.Implementation;
using Messaging.Enumerators;

namespace GameMasterTests
{
    public class GameLogicTest
    {
        private GameMaster.GameMaster gameMaster;
        private GameLogicComponent gameLogicComponent;

        [SetUp]
        public void Setup()
        {
            gameMaster = new GameMaster.GameMaster();
            gameLogicComponent = gameMaster.GameLogic;
        }

        [Test]
        public void ProcessMessage_ShouldReturnUndefinedErrorMessageWhenAgentNotConnected()
        {
            var message = GetBaseMessage(new PutDownPieceRequest(), 666);

            var response = gameLogicComponent.ProcessMessage(message);
            Assert.AreEqual(MessageId.UndefinedError, response.MessageId);
        }


        // This method simulates normal situation where messages are stored in IEnumerable<BaseMessage>
        private BaseMessage GetBaseMessage<T>(T payload, int agentFromId) where T:IPayload
        {
            return MessageFactory.GetMessage(payload, agentFromId);
        }
    }
}
