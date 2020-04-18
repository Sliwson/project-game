using NUnit.Framework;
using GameMaster;
using System.Drawing;
using System.Collections.Generic;
using System;
using Messaging.Contracts;
using Messaging.Implementation;
using Messaging.Contracts.Agent;
using Messaging.Contracts.Errors;
using Messaging.Enumerators;
using Messaging.Contracts.GameMaster;

namespace GameMasterTests
{
    public class NetworkComponentTest
    {
        private GameMaster.GameMaster gameMaster;
        private NetworkComponent networkComponent;

        private List<BaseMessage> messages;

        [SetUp]
        public void Setup()
        {
            gameMaster = new GameMaster.GameMaster();
            networkComponent = gameMaster.NetworkComponent;

            messages = CreateMessagesOfAllTypes();
        }

        [Test]
        public void SendLotsOfMessages()
        {
            foreach(var message in messages)
            {
                networkComponent.SendMessage(message);

            }
        }


        private List<BaseMessage> CreateMessagesOfAllTypes()
        {
            return new List<BaseMessage>
            {
                // Agent's messages
                MessageFactory.GetMessage(new CheckShamRequest(), 0),
                MessageFactory.GetMessage(new DestroyPieceRequest(), 0),
                MessageFactory.GetMessage(new DiscoverRequest(), 0),
                MessageFactory.GetMessage(new ExchangeInformationRequest(666), 0),
                MessageFactory.GetMessage(new ExchangeInformationResponse(
                                                    333,
                                                    new int[,]{ { 1, 2 }, { 3, 4 } },
                                                    new GoalInformation[,]{ { GoalInformation.Goal, GoalInformation.NoGoal }, { GoalInformation.NoInformation, GoalInformation.NoInformation } },
                                                    new GoalInformation[,]{ { GoalInformation.Goal, GoalInformation.NoGoal }, { GoalInformation.NoInformation, GoalInformation.NoInformation } }), 0),
                MessageFactory.GetMessage(new JoinRequest(TeamId.Blue, false), 0),
                MessageFactory.GetMessage(new MoveRequest(Direction.North), 0),
                MessageFactory.GetMessage(new PickUpPieceRequest(), 0),
                MessageFactory.GetMessage(new PutDownPieceRequest(), 0),

                // GameMaster's messages
                MessageFactory.GetMessage(new CheckShamResponse(true), 0),
                MessageFactory.GetMessage(new DestroyPieceResponse(), 0),
                MessageFactory.GetMessage(new DiscoverResponse(new int[,] { { 1, 0, 1 }, { -1, 1, 2 }, { 3, 2, -1 } }), 0),
                MessageFactory.GetMessage(new EndGamePayload(TeamId.Red), 0),
                MessageFactory.GetMessage(new ExchangeInformationRequestForward(666, false, TeamId.Blue), 0),
                MessageFactory.GetMessage(new JoinResponse(false, 333), 0),
                MessageFactory.GetMessage(new MoveResponse(false, new Point(3,3), 2), 0),
                MessageFactory.GetMessage(new PickUpPieceResponse(), 0),
                MessageFactory.GetMessage(new PutDownPieceResponse(PutDownPieceResult.ShamOnGoalArea), 0),
                MessageFactory.GetMessage(new ExchangeInformationResponseForward(
                                                    333,
                                                    new int[,]{ { 1, 2 }, { 3, 4 } },
                                                    new GoalInformation[,]{ { GoalInformation.Goal, GoalInformation.NoGoal }, { GoalInformation.NoInformation, GoalInformation.NoInformation } },
                                                    new GoalInformation[,]{ { GoalInformation.Goal, GoalInformation.NoGoal }, { GoalInformation.NoInformation, GoalInformation.NoInformation } }), 0),
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
                                                    new Point(3,3)), 0),

                // Error messages
                MessageFactory.GetMessage(new MoveError(new Point(3,3)), 0),
                MessageFactory.GetMessage(new PickUpPieceError(PickUpPieceErrorSubtype.NothingThere), 0),
                MessageFactory.GetMessage(new PutDownPieceError(PutDownPieceErrorSubtype.AgentNotHolding), 0),
                MessageFactory.GetMessage(new IgnoredDelayError(TimeSpan.FromSeconds(5.0)), 0),
                MessageFactory.GetMessage(new UndefinedError(new Point(3,3), false), 0)
            };
        }

    }
}