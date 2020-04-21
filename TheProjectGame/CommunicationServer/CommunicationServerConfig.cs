using Newtonsoft.Json;

namespace CommunicationServer
{
    internal class CommunicationServerConfig
    {
        [JsonProperty(PropertyName = "AgentsPort")]
        internal int AgentPort { get; set; }

        [JsonProperty(PropertyName = "GmPort")]
        internal int GameMasterPort { get; set; }

        [JsonConstructor]
        internal CommunicationServerConfig(int agentPort, int gameMasterPort)
        {
            AgentPort = agentPort;
            GameMasterPort = gameMasterPort;
        }
    }
}
