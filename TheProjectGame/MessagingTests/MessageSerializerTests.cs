using NUnit.Framework;
using Messaging.Contracts;
using System.Collections.Generic;
using Messaging.Serialization;

namespace MessagingTests
{
    public class MessageSerializerTests
    {
        List<BaseMessage> messages;

        [SetUp]
        public void SetUp()
        {
            messages = MessagingTestHelper.CreateMessagesOfAllTypes();
        }

        [Test]
        public void MessageSerializer_ShouldSerializeAllTypesCorrectly()
        {
            foreach (var message in messages)
            {
                var serialized = MessageSerializer.SerializeMessage(message);
            }

            Assert.Pass();
        }

        [Test]
        public void MessageSerializer_ShouldDeserializeAllTypesCorrectly()
        {
            foreach (var message in messages)
            {
                var serialized = MessageSerializer.SerializeMessage(message);
                var deserialized = MessageSerializer.DeserializeMessage(serialized);
            }

            Assert.Pass();
        }

        [Test]
        public void DeserializeMessage_ShouldNotChangeValue()
        {
            foreach (var message in messages)
            {
                var serialized = MessageSerializer.SerializeMessage(message);
                var deserialized = MessageSerializer.DeserializeMessage(serialized);
                var newSerialized = MessageSerializer.SerializeMessage(deserialized);

                Assert.AreEqual(serialized, newSerialized);
            }
        }

        [Test]
        public void DeserializeMessage_ShouldReturnDerivedTypeMessage()
        {
            foreach (var message in messages)
            {
                var serialized = MessageSerializer.SerializeMessage(message);
                dynamic deserialized = MessageSerializer.DeserializeMessage(serialized);

                Assert.IsTrue(MessagingTestHelper.IsMessagePayloadDerived(deserialized));
            }
        }

        [Test]
        public void DeserializeMessage_ShouldReturnDerivedTypeMessageAfterAddingToList()
        {
            var deserializedMessages = new List<BaseMessage>();

            foreach (var message in messages)
            {
                var serialized = MessageSerializer.SerializeMessage(message);
                deserializedMessages.Add(MessageSerializer.DeserializeMessage(serialized));
            }

            foreach(var deserializedMessage in deserializedMessages)
            {
                dynamic dynamicMessage = deserializedMessage;
                Assert.IsTrue(MessagingTestHelper.IsMessagePayloadDerived(dynamicMessage));
            }
        }
    }
}
