using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GameMasterPresentation
{
    public partial class MainWindow
    {
        

        //private void InitPresentation()
        //{
            //gameMaster.MockMessageSendFunction = OnMessageSentFromGameMaster;

            //gameMaster.ApplyConfiguration();

            //var agentsCount = 6;// gameMaster.Configuration.AgentsLimit;
            //agents = new Agent.Agent[agentsCount];

            //var teamLimit = agentsCount / 2;
            //for (int i = 0; i < agentsCount; i++)
            //{
            //    var agent = new Agent.Agent(i % teamLimit == 0);
            //    agent.team = i < teamLimit ? TeamId.Blue : TeamId.Red;
            //    agent.MockMessageSendFunction = OnMessageSentFromAgent;
            //    agents[i] = agent;

            //    var t = new Thread(new ThreadStart(agent.JoinTheGame));
            //    threads.Add(t);
            //    t.Start();
            //    logger.Debug("Agent " + (i + 1).ToString() + " started game");

            //    Thread.Sleep(100);
            //    gameMaster.Update(0);
            //}

            //Thread.Sleep(1000);
            //gameMaster.Update(0);
        //}

        //private void OnMessageSentFromAgent(Agent.Agent agent, BaseMessage message)
        //{
        //    //get idx
        //    int i = 0;
        //    for (i = 0; i < agents.Length; i++)
        //        if (agents[i] == agent)
        //            break;

        //    message.AgentId = i;
        //    logger.Debug("Message sent from agent " + i.ToString());
        //    gameMaster.InjectMessage(message);
        //}

        //private void OnMessageSentFromGameMaster(BaseMessage message)
        //{
        //    logger.Debug("Message sent from game master");
        //    agents[message.AgentId].InjectMessage(message);
        //}

       

        

        //private void UpdateBoard()
        //{
        //    UpdateAgents();
        //    var field = gameMaster.BoardLogic.GetFields();
        //    for (int y = 0; y < field.GetLength(0); y++)
        //    {
        //        for (int x = 0; x < field.GetLength(1); x++)
        //        {
        //            SetSingleBoardField(field[y, x], BoardFields[y, x]);
        //        }
        //    }
        //    SetScore();
        //}

        
    }
}
