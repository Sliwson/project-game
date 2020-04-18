using Agent;
using Messaging.Contracts.GameMaster;
using Messaging.Enumerators;
using NUnit.Framework;
using System;
using System.Drawing;

namespace AgentTests
{
    public class CommonTest
    {
        private StartGamePayload startGamePayload;

        [Test]
        public void GetGoalDirectionDoesNotReturnOppositeDirection()
        {
            int shortTime = 4;
            AgentConfiguration agentConfiguration = new AgentConfiguration();
            agentConfiguration.WantsToBeTeamLeader = false;
            agentConfiguration.TeamID = "Red";
            Agent.Agent agentRed = new Agent.Agent(agentConfiguration); 
            var teamMates = new int[3] { 2, 3, 4 };
            var enemiesIds = new int[3] { 5, 7, 6 };
            startGamePayload = new StartGamePayload(1, teamMates, 1, enemiesIds, TeamId.Red, new Point(5, 5), 1, 3, 3, 4, 4, new System.Collections.Generic.Dictionary<ActionType, TimeSpan>(), 0.5f, new Point(0, 0));
            agentRed.StartGameComponent.Initialize(startGamePayload);
            Assert.AreNotEqual(Common.GetGoalDirection(agentRed, shortTime), Direction.South);
            agentConfiguration.TeamID = "Blue";
            Agent.Agent agentBlue = new Agent.Agent(agentConfiguration);
            startGamePayload = new StartGamePayload(1, teamMates, 1, enemiesIds, TeamId.Blue, new Point(5, 5), 1, 3, 3, 4, 4, new System.Collections.Generic.Dictionary<ActionType, TimeSpan>(), 0.5f, new Point(0, 0));
            agentBlue.StartGameComponent.Initialize(startGamePayload);
            Assert.AreNotEqual(Common.GetGoalDirection(agentBlue, shortTime), Direction.North);
        }
    }
}
