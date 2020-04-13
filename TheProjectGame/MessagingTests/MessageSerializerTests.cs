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
        public void TestSerialization()
        {
            foreach (var message in messages)
            {
                var serialized = MessageSerializer.SerializeMessage(message);
            }

            Assert.Pass();
        }
    }
}
