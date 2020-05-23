using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.Errors;
using Messaging.Contracts.GameMaster;
using Messaging.Enumerators;
using Messaging.Implementation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace MessagingTests
{
    public static class MessagingTestHelper
    {
        // Keep this method up to date with contracts
        public static List<BaseMessage> CreateMessagesOfAllTypes()
        {
            return new List<BaseMessage>
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
                MessageFactory.GetMessage(new DiscoverResponse(new int[,] { { 1, 0, 1 }, { -1, 1, 2 }, { 3, 2, -1 } })),
                MessageFactory.GetMessage(new EndGamePayload(TeamId.Red)),
                MessageFactory.GetMessage(new ExchangeInformationRequestForward(666, false, TeamId.Blue)),
                MessageFactory.GetMessage(new JoinResponse(false, 333)),
                MessageFactory.GetMessage(new MoveResponse(false, new Point(3,3), 2)),
                MessageFactory.GetMessage(new PickUpPieceResponse()),
                MessageFactory.GetMessage(new PutDownPieceResponse(PutDownPieceResult.ShamOnGoalArea)),
                MessageFactory.GetMessage(new ExchangeInformationResponseForward(
                                                    333,
                                                    new int[,]{ { 1, 2 }, { 3, 4 } },
                                                    new GoalInformation[,]{ { GoalInformation.Goal, GoalInformation.NoGoal }, { GoalInformation.NoInformation, GoalInformation.NoInformation } },
                                                    new GoalInformation[,]{ { GoalInformation.Goal, GoalInformation.NoGoal }, { GoalInformation.NoInformation, GoalInformation.NoInformation } })),
                MessageFactory.GetMessage(new StartGamePayload(
                                                    333,
                                                    new int[] { 666 },
                                                    666,
                                                    new int[] { 42 },
                                                    TeamId.Blue,
                                                    new Point(8,10),
                                                    3,
                                                    2,
                                                    1,
                                                    5,
                                                    5,
                                                    new Dictionary<ActionType, TimeSpan>(),
                                                    0.1f,
                                                    new Point(3,3))),

                // Error messages
                MessageFactory.GetMessage(new MoveError(new Point(3,3))),
                MessageFactory.GetMessage(new PickUpPieceError(PickUpPieceErrorSubtype.NothingThere)),
                MessageFactory.GetMessage(new PutDownPieceError(PutDownPieceErrorSubtype.AgentNotHolding)),
                MessageFactory.GetMessage(new IgnoredDelayError(TimeSpan.FromSeconds(5.0))),
                MessageFactory.GetMessage(new UndefinedError(new Point(3,3), false))
            };
        }
  
        public static bool IsMessagePayloadDerived<T>(Message<T> message) where T : IPayload
        {
            return message != null;
        }

        public static bool IsMessagePayloadDerived(BaseMessage message)
        {
            return false;
        }

        public static bool IsMessagePayloadError<T>(Message<T> message) where T : IPayload
        {
            return message != null && message.Payload is IErrorPayload;
        }

        public static string SerializeWithoutProperties(BaseMessage message, params string[] propertyNames)
        {
            var jObject = JObject.FromObject(message);
            foreach(var propertyName in propertyNames)
                jObject.Remove(propertyName);

            return jObject.ToString();
        }
    }
}
