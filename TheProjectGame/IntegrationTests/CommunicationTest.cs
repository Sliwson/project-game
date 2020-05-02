using Agent;
using CommunicationServer;
using GameMaster;
using Messaging.Contracts.Agent;
using Messaging.Enumerators;
using Messaging.Implementation;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Linq;
using Messaging.Contracts.GameMaster;
using Newtonsoft.Json.Bson;
using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Messaging.Communication;

namespace IntegrationTests
{
    public class CommunicationTest
    {
        private GameMaster.GameMaster gameMaster;
        private const int gameMasterSleepMs = 16;
        private IntegrationTestsHelper.GameMasterTaskState gameMasterTaskState;

        private Agent.Agent agent;

        Task csTask;
        Task gmTask;

        [SetUp]
        public void Setup()
        {
            gameMaster = new GameMaster.GameMaster();
            gameMasterTaskState = new IntegrationTestsHelper.GameMasterTaskState { GameMaster = gameMaster, GameMasterSleepMs = gameMasterSleepMs };

            agent = new Agent.Agent(new AgentConfiguration
            {
                CsIP = "127.0.0.1",
                CsPort = 54321,
                TeamID = "red"
            });
        }

        [Test]
        public void WhenGameMasterIsConnected_ShouldGetResponse()
        {
            csTask = new Task(IntegrationTestsHelper.RunCommunicationServer);
            csTask.Start();
            gmTask = new Task(IntegrationTestsHelper.RunGameMaster, gameMasterTaskState);
            gmTask.Start();

            gameMaster.ApplyConfiguration();
            agent.ConnectToCommunicationServer();

            var messageToSend = MessageFactory.GetMessage(new JoinRequest(TeamId.Red, true));
            agent.SendMessage(messageToSend, false);

            gmTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, gmTask.Status);
            
            var receivedMessage = agent.NetworkComponent.GetIncomingMessages().FirstOrDefault();
            Assert.IsNotNull(receivedMessage);
            Assert.AreEqual(MessageId.JoinResponse, receivedMessage.MessageId);

            Assert_CommunicationServerSuccessfullyKilled();
        }

        [Test]
        public void WhenGameMasterIsNotYetConnected_MessageShouldBeIgnored()
        {
            var csTask = new Task(IntegrationTestsHelper.RunCommunicationServer);
            csTask.Start();

            agent.ConnectToCommunicationServer();

            var messageToSend = MessageFactory.GetMessage(new JoinRequest(TeamId.Red, true));
            agent.SendMessage(messageToSend, false);

            Thread.Sleep(100);
            var receivedMessage = agent.NetworkComponent.GetIncomingMessages().FirstOrDefault();
            Assert.IsNull(receivedMessage);

            //Make sure CS has not failed
            Thread.Sleep(100);
            Assert.AreEqual(TaskStatus.Running, csTask.Status);
        }

        [Test]
        public void WhenGameMasterIsClosed_ExceptionShouldBeThrown()
        {
            var csTask = new Task(IntegrationTestsHelper.RunCommunicationServer);
            csTask.Start();

            // Connect and then disconnect Game Master
            gameMaster.ApplyConfiguration();
            var disconnectResult = gameMaster.NetworkComponent.Disconnect();
            Assert.IsTrue(disconnectResult);

            agent.ConnectToCommunicationServer();

            // Need to send two times (one does not trigger exception)
            var messageToSend = MessageFactory.GetMessage(new JoinRequest(TeamId.Red, true));
            agent.SendMessage(messageToSend, false);
            agent.SendMessage(messageToSend, false);

            var receivedMessage = agent.NetworkComponent.GetIncomingMessages().FirstOrDefault();

            Assert.IsNull(receivedMessage);
            Thread.Sleep(100);

            Assert.AreEqual(TaskStatus.Faulted, csTask.Status);
            var exception = csTask.Exception.InnerException as CommunicationErrorException;
            Assert.IsNotNull(exception);
            Assert.AreEqual(CommunicationExceptionType.GameMasterDisconnected, exception.Type);
        }

        // Server should be gently killed after GameMaster termination
        private void Assert_CommunicationServerSuccessfullyKilled()
        {
            try
            {
                gameMaster.OnDestroy();
                csTask.Wait();
            }
            catch(AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, csTask.Status);
                var exception = ex.InnerException as CommunicationErrorException;
                Assert.IsNotNull(exception);
                Assert.AreEqual(CommunicationExceptionType.GameMasterDisconnected, exception.Type);
                
            }
        }
    }
}