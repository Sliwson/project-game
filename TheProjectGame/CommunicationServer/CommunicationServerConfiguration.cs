using Newtonsoft.Json;
using System;
using System.IO;

namespace CommunicationServer
{
    public class CommunicationServerConfiguration
    {
        [JsonProperty(PropertyName = "AgentsPort")]
        internal int AgentPort { get; set; }

        [JsonProperty(PropertyName = "GmPort")]
        internal int GameMasterPort { get; set; }

        internal static CommunicationServerConfiguration GetDefault()
        {
            return new CommunicationServerConfiguration
            {
                AgentPort = 54321,
                GameMasterPort = 54322
            };
        }

        internal static CommunicationServerConfiguration LoadFromFile(string filename)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();

            try
            {
                var text = File.ReadAllText(@filename);
                CommunicationServerConfiguration csConfiguration = JsonConvert.DeserializeObject<CommunicationServerConfiguration>(text);
                logger.Info("[CommunicationServerConfiguration] Configuration loaded from {name}", filename);
                return csConfiguration;
            }
            catch (Exception e)
            {
                logger.Error("[CommunicationServerConfiguration] Cannot load configuration!");
                logger.Error(e.Message);
            }

            return null;
        }
    }
}
