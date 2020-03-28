using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Agent
{
    class Program
    {
        public AgentConfiguration Configuration { get; private set; }
        public List<Agent> agents { get; set; }

        static void Main(string[] args)
        {
        }
        public void MainAgentMethod()
        {
            LoadDefaultConfiguration();
            CreateAgents();
            AgentsWork();
        }

        private void LoadDefaultConfiguration()
        {
            var configurationProvider = new MockConfigurationProvider();
            Configuration = configurationProvider.GetConfiguration();
            agents = new List<Agent>();
        }

        private void CreateAgents()
        {
            for(int i = 0; i < Configuration.AgentsLimit * 2; i++)
            {
                var agent = new Agent(i % Configuration.AgentsLimit == 0);
                agents.Add(agent);
                agent.JoinTheGame();
            }
        }

        private void AgentsWork()
        {
            Parallel.ForEach(agents, (agent) =>
            {
                var message = agent.GetIncommingMessage();
                agent.AcceptMessage(message);
            });
        }
    }
}
