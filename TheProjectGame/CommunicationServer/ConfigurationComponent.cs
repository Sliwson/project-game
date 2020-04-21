using Newtonsoft.Json;
using System;
using System.IO;

namespace CommunicationServer
{
    internal class ConfigurationComponent
    {
        private CommunicationServerConfig config;

        internal ConfigurationComponent(string configFileName = null)
        {
            if (configFileName == null)
                config = GetFakeConfig();

            else
                config = LoadConfigFromFile(configFileName);
        }

        internal int GetAgentPort()
        {
            if (config == null)
                throw new ArgumentNullException("Config has not been set");

            return config.AgentPort;
        }

        internal int GetGameMasterPort()
        {
            if (config == null)
                throw new ArgumentNullException("Config has not been set");

            return config.GameMasterPort;
        }

        private CommunicationServerConfig GetFakeConfig()
        {
            return new CommunicationServerConfig
            (
                agentPort: 54321,
                gameMasterPort: 12345
            );
        }

        private CommunicationServerConfig LoadConfigFromFile(string configFileName)
        {
            return JsonConvert.DeserializeObject<CommunicationServerConfig>(File.ReadAllText(configFileName));
        }
    }
}
