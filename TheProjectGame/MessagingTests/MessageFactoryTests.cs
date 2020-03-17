using NUnit.Framework;
using Messaging.Implementation;
using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.GameMaster;
using System.Collections.Generic;
using Messaging.Enumerators;
using System;
using System.Drawing;
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
                MessageFactory.GetMessage(new ExchangeInformationResponse(
                                                    333,
                                                    new int[,]{ { 1, 2 }, { 3, 4 } },
                                                    new GoalInformation[,]{ { GoalInformation.Goal, GoalInformation.NoGoal }, { GoalInformation.NoInformation, GoalInformation.NoInformation } },
                                                    new GoalInformation[,]{ { GoalInformation.Goal, GoalInformation.NoGoal }, { GoalInformation.NoInformation, GoalInformation.NoInformation } })),
                MessageFactory.GetMessage(new JoinRequest(TeamId.Blue)),
                MessageFactory.GetMessage(new MoveRequest(Direction.North)),
                MessageFactory.GetMessage(new PickUpPieceRequest()),
                MessageFactory.GetMessage(new PutDownPieceRequest()),

                // GameMaster's messages
                MessageFactory.GetMessage(new CheckShamResponse(true)),
                MessageFactory.GetMessage(new DestroyPieceResponse()),
                MessageFactory.GetMessage(new DiscoverResponse(new int[,] { { 1, 0, 1 }, { 2, 1, 2 }, { 3, 2, 3 } })),
                MessageFactory.GetMessage(new EndGamePayload(TeamId.Red)),
                MessageFactory.GetMessage(new ExchangeInformationPayload(666, false, TeamId.Blue)),
                MessageFactory.GetMessage(new JoinResponse(false, 333)),
                MessageFactory.GetMessage(new MoveResponse(false, new Point(3,3), 2)),
                MessageFactory.GetMessage(new PickUpPieceResponse()),
                MessageFactory.GetMessage(new PutDownPieceResponse()),
                MessageFactory.GetMessage(new StartGamePayload(
                                                    333,
                                                    new int[] { 666 },
                                                    666,
                                                    new int[] { 42 },
                                                    TeamId.Blue,
                                                    new Point(8,10),
                                                    3,
                                                    1,
                                                    1,
                                                    5,
                                                    5,
                                                    new Dictionary<ActionType, decimal>(),
                                                    0.1m,
                                                    new Point(3,3))),

                // Error messages
                MessageFactory.GetMessage(new MoveError(new Point(3,3))),
                MessageFactory.GetMessage(new PickUpPieceError(PickUpPieceErrorSubtype.NothingThere)),
                MessageFactory.GetMessage(new PutDownPieceError(PutDownPieceErrorSubtype.AgentNotHolding)),
                MessageFactory.GetMessage(new IgnoredDelayError(DateTime.Now.AddSeconds(5.0))),
                MessageFactory.GetMessage(new UndefinedError(new Point(3,3), false))
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

        private bool IsMessagePayloadError<T>(Message<T> message) where T:IPayload
        {
            return message.Payload is IErrorPayload && message != null;
        }
    }
}