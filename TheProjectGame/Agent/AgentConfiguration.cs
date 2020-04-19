using Messaging.Enumerators;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Agent
{
    // TODO: Improve configuration for tests
    public class AgentConfiguration
    {
        public string CsIP { get; set; }
        public int CsPort { get; set; }
        public string TeamID { get; set; }
        public int Strategy { get; set; }
        public bool WantsToBeTeamLeader { get; set; }
        
        public AgentConfiguration()
        {
            CsIP = "127.0.0.1";
            CsPort = 54321;
        }

        public static AgentConfiguration GetConfiguration(string fileName = null)
        {
            if (fileName == null)
            {
                fileName = "..\\..\\..\\Configuration\\agentConfiguration.json";
            }
            AgentConfiguration agentConfiguration = JsonConvert.DeserializeObject<AgentConfiguration>(File.ReadAllText(@fileName));
            return agentConfiguration;
        }
    }
}
