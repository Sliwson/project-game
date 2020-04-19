namespace CommunicationServer
{
    internal class CommunicationServerConfig
    {
        internal int AgentPort { get; set; }
        internal int GameMasterPort { get; set; }

        internal CommunicationServerConfig(int agentPort, int gameMasterPort)
        {
            AgentPort = agentPort;
            GameMasterPort = gameMasterPort;
        }
    }
}
