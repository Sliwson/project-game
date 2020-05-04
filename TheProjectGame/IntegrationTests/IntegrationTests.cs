using GameMaster;
using Messaging.Enumerators;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using CommunicationServer;
using Messaging.Communication;
using System;

namespace IntegrationTests
{
    public class IntegrationTests
    {
        int agentsInTeam = 1;
        const int agentSleepMs = 16;

        IntegrationTestsHelper.GameMasterTaskState gmTaskState;

        [SetUp]
        public void Setup()
        {
            var gameMaster = new GameMaster.GameMaster(GameMasterConfiguration.GetDefault());

            agentsInTeam = gameMaster.Configuration.TeamSize;
            gmTaskState = new IntegrationTestsHelper.GameMasterTaskState(gameMaster, 16);
        }

        [Test]
        public void ConnectingAgentsState_ShouldConnectAgent()
        {
            var csConfig = CommunicationServerConfiguration.GetDefault();
            var csTask = new Task(IntegrationTestsHelper.RunCommunicationServer, csConfig);
            csTask.Start();
            Thread.Sleep(100);

            gmTaskState.GameMaster.ApplyConfiguration();
            var gmTask = new Task(IntegrationTestsHelper.RunGameMaster, gmTaskState);
            gmTask.Start();

            var agentTaskStates = IntegrationTestsHelper.CreateAgents(agentsInTeam)
                .Select(agent => new IntegrationTestsHelper.AgentTaskState(agent, agentSleepMs)).ToList();

            var agentTasks = agentTaskStates
                .Select(agentTaskState => new Task(IntegrationTestsHelper.RunAgent, agentTaskState)).ToList();

            foreach (var agentTask in agentTasks)
            {
                agentTask.Start();
                Thread.Sleep(100);
            }

            gmTask.Wait();

            List<GameMaster.Agent> lobby = gmTaskState.GameMaster.ConnectionLogic.FlushLobby();

            Assert.AreEqual(agentsInTeam * 2, lobby.Count);
            Assert.AreEqual(2, lobby.Where(agent => agent.IsTeamLeader).Count());
            Assert.AreEqual(agentsInTeam, lobby.Where(agent => agent.Team == TeamId.Blue).Count());
            Assert.AreEqual(agentsInTeam, lobby.Where(agent => agent.Team == TeamId.Red).Count());

            // Cleanup
            gmTaskState.GameMaster.OnDestroy();
            try
            {
                gmTaskState.GameMaster.OnDestroy();
                csTask.Wait(200);
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, csTask.Status);
                var exception = ex.InnerException as CommunicationErrorException;
                Assert.IsNotNull(exception);
                Assert.AreEqual(CommunicationExceptionType.GameMasterDisconnected, exception.Type);

                for(int i = 0; i < agentTasks.Count; i++)
                { 
                    agentTasks[i].Wait(100);
                    agentTaskStates[i].Agent.OnDestroy();
                }
            }
        }
    }
}