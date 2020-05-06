using Agent;
using CommunicationServer;
using GameMaster;
using Messaging.Contracts.Agent;
using Messaging.Enumerators;
using Messaging.Implementation;
using NUnit.Framework;
using System.Threading;
using System.Linq;
using Messaging.Contracts.GameMaster;
using System;
using System.Threading.Tasks;
using Messaging.Communication;
using NUnit.Framework.Internal;

namespace IntegrationTests
{
    public class CommunicationTest
    {
        private class TestComponents
        {
            internal GameMaster.GameMaster GameMaster { get; set; }
            internal Agent.Agent Agent { get; set; }
            internal Task CsTask { get; set; }
        }

        private TestComponents InitializeTest(int testId)
        {
            var result = new TestComponents();

            GetConfigurationsForTest(testId, out AgentConfiguration agentConfig, out GameMasterConfiguration gmConfig, out CommunicationServerConfiguration csConfig);

            result.GameMaster = new GameMaster.GameMaster(gmConfig);
            result.Agent = new Agent.Agent(agentConfig);
            result.CsTask = new Task(IntegrationTestsHelper.RunCommunicationServer, csConfig);

            return result;
        }

        [NonParallelizable]
        [Test]
        public void WhenGameMasterIsConnected_MessageShouldBeSent()
        {
            var testComponents = InitializeTest(0);
            testComponents.CsTask.Start();
            Thread.Sleep(100);

            testComponents.GameMaster.ConnectToCommunicationServer();
            testComponents.Agent.ConnectToCommunicationServer();

            var messageToSend = MessageFactory.GetMessage(new JoinRequest(TeamId.Red, true));
            testComponents.Agent.SendMessage(messageToSend, false);

            Thread.Sleep(100);
            var receivedMessage = testComponents.GameMaster.NetworkComponent.GetIncomingMessages().FirstOrDefault();
            Assert.IsNotNull(receivedMessage);
            Assert.AreEqual(MessageId.JoinRequest, receivedMessage.MessageId);

            //Make sure CS has not failed
            Thread.Sleep(100);
            Assert.AreEqual(TaskStatus.Running, testComponents.CsTask.Status);

            Assert_CommunicationServerSuccessfullyKilled(testComponents);
        }

        [NonParallelizable]
        [Test]
        public void WhenGameMasterIsNotYetConnected_MessageShouldBeIgnored()
        {
            var testComponents = InitializeTest(1);
            testComponents.CsTask.Start();
            Thread.Sleep(100);

            testComponents.Agent.ConnectToCommunicationServer();

            var messageToSend = MessageFactory.GetMessage(new JoinRequest(TeamId.Red, true));
            testComponents.Agent.SendMessage(messageToSend, false);

            Thread.Sleep(100);
            var receivedMessage = testComponents.Agent.NetworkComponent.GetIncomingMessages().FirstOrDefault();
            Assert.IsNull(receivedMessage);

            //Make sure CS has not failed
            Thread.Sleep(100);
            Assert.AreEqual(TaskStatus.Running, testComponents.CsTask.Status);

            Assert_CommunicationServerSuccessfullyKilled(testComponents);
        }

        [NonParallelizable]
        [Test]
        public void WhenClientIsConnected_ResponseShouldBeDelivered()
        {
            var testComponents = InitializeTest(3);
            testComponents.CsTask.Start();
            Thread.Sleep(100);

            testComponents.GameMaster.ConnectToCommunicationServer();
            testComponents.Agent.ConnectToCommunicationServer();

            var messageFromAgent = MessageFactory.GetMessage(new JoinRequest(TeamId.Red, false));
            testComponents.Agent.SendMessage(messageFromAgent, false);
            Thread.Sleep(100);

            var messageFromGm = MessageFactory.GetMessage(new JoinResponse(true, 1), 1);
            testComponents.GameMaster.SendMessage(messageFromGm);
            Thread.Sleep(100);

            var receivedMessage = testComponents.Agent.NetworkComponent.GetIncomingMessages().FirstOrDefault();
            Assert.IsNotNull(receivedMessage);
            Assert.AreEqual(MessageId.JoinResponse, receivedMessage.MessageId);

            //Make sure CS has not failed
            Thread.Sleep(100);
            Assert.AreEqual(TaskStatus.Running, testComponents.CsTask.Status);

            Assert_CommunicationServerSuccessfullyKilled(testComponents);
        }

        [NonParallelizable]
        [Test]
        public void WhenClientIsClosed_ServerShouldNotTerminate()
        {
            var testComponents = InitializeTest(4);
            testComponents.CsTask.Start();
            Thread.Sleep(100);

            testComponents.GameMaster.ConnectToCommunicationServer();
            testComponents.Agent.ConnectToCommunicationServer();

            var messageFromAgent = MessageFactory.GetMessage(new JoinRequest(TeamId.Red, false));
            testComponents.Agent.SendMessage(messageFromAgent, false);
            testComponents.Agent.OnDestroy();
            Thread.Sleep(100);

            var messageFromGm = MessageFactory.GetMessage(new JoinResponse(true, 1), 1);
            testComponents.GameMaster.SendMessage(messageFromGm);
            Thread.Sleep(100);

            var receivedMessage = testComponents.Agent.NetworkComponent.GetIncomingMessages().FirstOrDefault();
            Assert.IsNull(receivedMessage);

            //Make sure CS has not failed
            Thread.Sleep(100);
            Assert.AreEqual(TaskStatus.Running, testComponents.CsTask.Status);

            Assert_CommunicationServerSuccessfullyKilled(testComponents);
        }

        [NonParallelizable]
        [Test]
        public void WhenClientIsNotYetConnected_MessageShouldBeIgnored()
        {
            var testComponents = InitializeTest(5);
            testComponents.CsTask.Start();
            Thread.Sleep(100);

            testComponents.GameMaster.ConnectToCommunicationServer();

            var messageToSend = MessageFactory.GetMessage(new JoinResponse(true, 1));
            testComponents.GameMaster.SendMessage(messageToSend);

            Thread.Sleep(100);
            var receivedMessage = testComponents.Agent.NetworkComponent.GetIncomingMessages().FirstOrDefault();

            Assert.IsNull(receivedMessage);

            //Make sure CS has not failed
            Thread.Sleep(100);
            Assert.AreEqual(TaskStatus.Running, testComponents.CsTask.Status);

            Assert_CommunicationServerSuccessfullyKilled(testComponents);
        }

        [NonParallelizable]
        [Test]
        public void MessagesFromSecondGameMaster_ShouldBeIgnored()
        {
            var testComponents = InitializeTest(6);
            testComponents.CsTask.Start();
            Thread.Sleep(100);

            testComponents.GameMaster.ConnectToCommunicationServer();
            testComponents.Agent.ConnectToCommunicationServer();

            var secondGameMaster = new GameMaster.GameMaster(testComponents.GameMaster.Configuration);
            secondGameMaster.ConnectToCommunicationServer();

            var messageToSend = MessageFactory.GetMessage(new JoinResponse(true, 1), 1);
            secondGameMaster.SendMessage(messageToSend);

            Thread.Sleep(100);
            var receivedMessage = testComponents.Agent.NetworkComponent.GetIncomingMessages().FirstOrDefault();

            Assert.IsNull(receivedMessage);

            //Make sure CS has not failed
            Thread.Sleep(100);
            Assert.AreEqual(TaskStatus.Running, testComponents.CsTask.Status);

            Assert_CommunicationServerSuccessfullyKilled(testComponents);
        }

        [NonParallelizable]
        [Test]
        public void SecondGameMaster_ShouldNotGetAnyMessages()
        {
            var testComponents = InitializeTest(7);
            testComponents.CsTask.Start();
            Thread.Sleep(100);

            testComponents.GameMaster.ConnectToCommunicationServer();
            testComponents.Agent.ConnectToCommunicationServer();

            var secondGameMaster = new GameMaster.GameMaster(testComponents.GameMaster.Configuration);
            secondGameMaster.ConnectToCommunicationServer();

            var messageToSend = MessageFactory.GetMessage(new JoinRequest(TeamId.Blue, false));
            testComponents.Agent.SendMessage(messageToSend, false);

            Thread.Sleep(100);
            var receivedMessage = secondGameMaster.NetworkComponent.GetIncomingMessages().FirstOrDefault();
            Assert.IsNull(receivedMessage);

            var actualReceivedMessage = testComponents.GameMaster.NetworkComponent.GetIncomingMessages().FirstOrDefault();
            Assert.IsNotNull(actualReceivedMessage);

            //Make sure CS has not failed
            Thread.Sleep(100);
            Assert.AreEqual(TaskStatus.Running, testComponents.CsTask.Status);

            Assert_CommunicationServerSuccessfullyKilled(testComponents);
        }

        [NonParallelizable]
        [Test]
        public void WhenGameMasterDisconnecting_CommunicationServerShouldThrow()
        {
            var testComponents = InitializeTest(8);
            testComponents.CsTask.Start();
            Thread.Sleep(100);

            // Connect to CS
            testComponents.GameMaster.ConnectToCommunicationServer();

            // And then disconnect
            testComponents.GameMaster.OnDestroy();

            Assert_CommunicationServerSuccessfullyKilled(testComponents);
        }

        // Server should be gently killed after GameMaster termination
        private void Assert_CommunicationServerSuccessfullyKilled(TestComponents testComponents)
        {
            try
            {
                testComponents?.Agent.OnDestroy();
                testComponents?.GameMaster.OnDestroy();
                testComponents.CsTask.Wait(200);
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, testComponents.CsTask.Status);
                var exception = ex.InnerException as CommunicationErrorException;
                Assert.IsNotNull(exception);
                Assert.AreEqual(CommunicationExceptionType.GameMasterDisconnected, exception.Type);
            }
        }

        private void GetConfigurationsForTest(int testId, out AgentConfiguration agentConfig, out GameMasterConfiguration gmConfig, out CommunicationServerConfiguration csConfig)
        {
            const int baseAgentPort = 50000;
            const int baseGmPort = 60000;

            var agentPortForTest = baseAgentPort + testId;
            var gmPortForTest = baseGmPort - testId;

            csConfig = CommunicationServerConfiguration.GetDefault();

            agentConfig = AgentConfiguration.GetDefault();
            agentConfig.CsPort = agentPortForTest;
            csConfig.AgentPort = agentPortForTest;

            gmConfig = GameMasterConfiguration.GetDefault();
            gmConfig.CsPort = gmPortForTest;
            csConfig.GameMasterPort = gmPortForTest;
        }
    }
}