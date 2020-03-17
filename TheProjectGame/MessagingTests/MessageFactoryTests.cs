using NUnit.Framework;
using Messaging.Implementation;
using Messaging.Contracts;
using Messaging.Contracts.Agent;
using System.Collections.Generic;
using Messaging.Enumerators;
using System;

namespace MessagingTests
{
    public class MessageFactoryTests
    {
        List<BaseMessage> messages;

        [SetUp]
        public void SetUp()
        {
            CreateMessagesOfAllTypes();
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

        // Keep this method up to date with contracts
        private void CreateMessagesOfAllTypes()
        {
            messages = new List<BaseMessage>
            {
                // Agent's messages
                MessageFactory.GetMessage(new CheckShamRequest()),
                MessageFactory.GetMessage(new DestroyPieceRequest()),
                MessageFactory.GetMessage(new DiscoverRequest()),
                MessageFactory.GetMessage(new ExchangeInformationRequest(666)),
                MessageFactory.GetMessage(new ExchangeInformationResponse(333,
                                          new int[,]{ { 1, 2 }, { 3, 4 } },
                                          new GoalInformation[,]{ { GoalInformation.Goal, GoalInformation.NoGoal }, { GoalInformation.NoInformation, GoalInformation.NoInformation } },
                                          new GoalInformation[,]{ { GoalInformation.Goal, GoalInformation.NoGoal }, { GoalInformation.NoInformation, GoalInformation.NoInformation } })),
                MessageFactory.GetMessage(new JoinRequest(TeamId.Blue)),
                MessageFactory.GetMessage(new MoveRequest(Direction.North)),
                MessageFactory.GetMessage(new PickUpPieceRequest()),
                MessageFactory.GetMessage(new PutDownPieceRequest())
            };
        }

        private bool IsMessagePayloadDerived<T>(Message<T> message) where T:IPayload
        {
            return message != null;
        }

        private bool IsMessagePayloadDerived(BaseMessage message)
        {
            return false;
        }
    }
}