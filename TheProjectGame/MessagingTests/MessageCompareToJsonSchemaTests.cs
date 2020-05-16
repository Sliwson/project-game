﻿using NUnit.Framework;
using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.GameMaster;
using Messaging.Contracts.Errors;
using System.Collections.Generic;
using Messaging.Enumerators;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System.IO;
using Newtonsoft.Json.Linq;
using System;

namespace MessagingTests
{
    class MessageCompareToJsonSchemaTests
    {
        List<BaseMessage> messages;

        [SetUp]
        public void SetUp()
        {
            messages = MessagingTestHelper.CreateMessagesOfAllTypes();
        }

        [Test]
        public void CompareSerializedMessagesToJsonSchemaTest()
        {
            foreach (var message in messages)
            {
                string serialized = "";             
                string schema = "";
                switch (message.MessageId)
                {
                    // Agent
                    case MessageId.CheckShamRequest:
                        serialized = JsonConvert.SerializeObject((message as Message<CheckShamRequest>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\Agent\checkForScham.json");
                        break;
                    case MessageId.DestroyPieceRequest:
                        serialized = JsonConvert.SerializeObject((message as Message<DestroyPieceRequest>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\Agent\destroyPiece.json");
                        break;
                    case MessageId.DiscoverRequest:
                        serialized = JsonConvert.SerializeObject((message as Message<DiscoverRequest>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\Agent\discovery.json");
                        break;
                    case MessageId.ExchangeInformationResponse:
                        serialized = JsonConvert.SerializeObject((message as Message<ExchangeInformationResponse>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\Agent\informationExchangeRespond.json");
                        break;
                    case MessageId.ExchangeInformationRequest:
                        serialized = JsonConvert.SerializeObject((message as Message<ExchangeInformationRequest>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\Agent\informationExchangeAsk.json");
                        break;
                    case MessageId.JoinRequest:
                        serialized = JsonConvert.SerializeObject((message as Message<JoinRequest>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\Agent\joinGame.json");
                        break;
                    case MessageId.MoveRequest:
                        serialized = JsonConvert.SerializeObject((message as Message<MoveRequest>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\Agent\makeMove.json");
                        break;
                    case MessageId.PickUpPieceRequest:
                        serialized = JsonConvert.SerializeObject((message as Message<PickUpPieceRequest>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\Agent\pickPiece.json");
                        break;
                    case MessageId.PutDownPieceRequest:
                        serialized = JsonConvert.SerializeObject((message as Message<PutDownPieceRequest>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\Agent\putPiece.json");
                        break;

                    // GameMaster
                    case MessageId.CheckShamResponse:
                        serialized = JsonConvert.SerializeObject((message as Message<CheckShamResponse>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\GameMaster\checkForSchamResponse.json");
                        break;
                    case MessageId.DestroyPieceResponse:
                        serialized = JsonConvert.SerializeObject((message as Message<DestroyPieceResponse>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\GameMaster\destroyPieceResponse.json");
                        break;
                    case MessageId.DiscoverResponse:
                        serialized = JsonConvert.SerializeObject((message as Message<DiscoverResponse>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\GameMaster\discoveryResponse.json");
                        break;
                    case MessageId.EndGameMessage:
                        serialized = JsonConvert.SerializeObject((message as Message<EndGamePayload>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\GameMaster\gameEnd.json");
                        break;
                    case MessageId.ExchangeInformationRequestForward:
                        serialized = JsonConvert.SerializeObject((message as Message<ExchangeInformationRequestForward>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\GameMaster\informationExchangeAskForward.json");
                        break;
                    case MessageId.JoinResponse:
                        serialized = JsonConvert.SerializeObject((message as Message<JoinResponse>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\GameMaster\joinGameResponse.json");
                        break;
                    case MessageId.MoveResponse:
                        serialized = JsonConvert.SerializeObject((message as Message<MoveResponse>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\GameMaster\makeMoveResponse.json");
                        break;
                    case MessageId.PickUpPieceResponse:
                        serialized = JsonConvert.SerializeObject((message as Message<PickUpPieceResponse>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\GameMaster\pickPieceResponse.json");
                        break;
                    case MessageId.PutDownPieceResponse:
                        serialized = JsonConvert.SerializeObject((message as Message<PutDownPieceResponse>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\GameMaster\putPieceResponse.json");
                        break;
                    case MessageId.ExchangeInformationResponseForward:
                        serialized = JsonConvert.SerializeObject((message as Message<ExchangeInformationResponseForward>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\GameMaster\informationExchangeRespondForward.json");
                        break;
                    case MessageId.StartGameMessage:
                        serialized = JsonConvert.SerializeObject((message as Message<StartGamePayload>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\GameMaster\gameStart.json");
                        break;

                    // Errors
                    case MessageId.MoveError:
                        serialized = JsonConvert.SerializeObject((message as Message<MoveError>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\Errors\invalidMove.json");
                        break;
                    case MessageId.PickUpPieceError:
                        serialized = JsonConvert.SerializeObject((message as Message<PickUpPieceError>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\Errors\pickPiece.json");
                        break;
                    case MessageId.PutDownPieceError:
                        serialized = JsonConvert.SerializeObject((message as Message<PutDownPieceError>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\Errors\putPieceError.json");
                        break;
                    case MessageId.IgnoredDelayError:
                        serialized = JsonConvert.SerializeObject((message as Message<IgnoredDelayError>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\Errors\timePenaltyError.json");
                        break;
                    case MessageId.UndefinedError:
                        serialized = JsonConvert.SerializeObject((message as Message<UndefinedError>).Payload);
                        schema = File.ReadAllText(@"..\..\..\Schema\Errors\undefinedErrorMessage.json");
                        break;
                }

                JSchema jsonSchema = JSchema.Parse(schema);
                JObject serializedAsJsonObject = JObject.Parse(serialized);  
                Assert.AreEqual(true, serializedAsJsonObject.IsValid(jsonSchema));               
            }
        }

    }
}
