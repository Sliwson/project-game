using Messaging.Enumerators;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Agent
{
    public class AgentConfiguration
    {
        public string CsIP { get; set; }
        public int CsPort { get; set; }
        public string TeamID { get; set; }
        public int Strategy { get; set; }
       
        public static AgentConfiguration GetDefault()
        {
            return new AgentConfiguration
            {
                CsIP = "127.0.0.1",
                CsPort = 54321,
                TeamID = "blue",
                Strategy = 0,
            };
        }

        public static AgentConfiguration LoadFromFile(string filename)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            try
            {
                var text = File.ReadAllText(@filename);
                AgentConfiguration agentConfiguration = JsonConvert.DeserializeObject<AgentConfiguration>(text);
                logger.Info("[AgentConfiguration] Configuration loaded from {name}", filename);
                return agentConfiguration;
            }
            catch (Exception e)
            {
                logger.Error("[AgentConfiguration] Cannot load configuration!");
                logger.Error(e.Message);
            }

            return null;
        }
    }
}
