using Agent;
using Messaging.Enumerators;
using NUnit.Framework;

namespace AgentTests
{
    public class CommonTest
    {
        [Test]
        public void GetGoalDirectionDoesNotReturnOppositeDirection()
        {
            int shortTime = 4;
            Agent.Agent agentRed = new Agent.Agent(false);
            agentRed.initializeComponent.Initialize(1, Messaging.Enumerators.TeamId.Red, new System.Drawing.Point(5, 5), 1, new System.Drawing.Point(0, 0), new int[] { 2, 3, 4 }, new System.Collections.Generic.Dictionary<ActionType, System.TimeSpan>(), 0.5f);
            Assert.AreNotEqual(Common.GetGoalDirection(agentRed, shortTime), Direction.South);
            Agent.Agent agentBlue = new Agent.Agent(false);
            agentBlue.initializeComponent.Initialize(1, Messaging.Enumerators.TeamId.Blue, new System.Drawing.Point(5, 5), 1, new System.Drawing.Point(0, 0), new int[] { 2, 3, 4 }, new System.Collections.Generic.Dictionary<ActionType, System.TimeSpan>(), 0.5f);
            Assert.AreNotEqual(Common.GetGoalDirection(agentBlue, shortTime), Direction.North);
        }
    }
}
