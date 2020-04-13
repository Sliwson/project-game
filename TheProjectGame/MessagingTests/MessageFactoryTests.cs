using NUnit.Framework;
using Messaging.Contracts;
using System.Collections.Generic;
using Messaging.Enumerators;
using System;
using Messaging.Contracts.Errors;
using System.Linq;

namespace MessagingTests
{
    public class MessageFactoryTests
    {
        List<BaseMessage> messages;
        
        // Update it if new error message has been added to messages
        readonly int errorMessagesCount = 5;

        [SetUp]
        public void SetUp()
        {
            messages = MessagingTestHelper.CreateMessagesOfAllTypes();
        }

        [Test]
        public void AllMessageIds_ShouldHaveCorrespondingMessage()
        {
            foreach(MessageId messageId in Enum.GetValues(typeof(MessageId)))
            {
                Assert.True(messages.Exists(message => message.MessageId == messageId));
            }
        }

        [Test]
        public void AllMessages_ShouldBeCastedToDerviedType()
        {
            foreach (var message in messages)
            {
                dynamic dynamicMessage = message;
                Assert.IsTrue(IsMessagePayloadDerived(dynamicMessage));
            }
        }

        [Test]
        public void ErrorMessage_ShouldHaveErrorPayload()
        {
            foreach (var message in messages.TakeLast(errorMessagesCount))
            {
                dynamic dynamicMessage = message;
                Assert.IsTrue(IsMessagePayloadError(dynamicMessage));
            }
        }

        [Test]
        public void NotErrorMessage_ShouldNotHaveErrorPayload()
        {
            foreach (var message in messages.SkipLast(errorMessagesCount))
            {
                dynamic dynamicMessage = message;
                Assert.IsFalse(IsMessagePayloadError(dynamicMessage));
            }
        }

        private bool IsMessagePayloadDerived<T>(Message<T> message) where T:IPayload
        {
            return message != null;
        }

        private bool IsMessagePayloadDerived(BaseMessage message)
        {
            return false;
        }

        private bool IsMessagePayloadError<T>(Message<T> message) where T:IPayload
        {
            return message.Payload is IErrorPayload && message != null;
        }
    }
}