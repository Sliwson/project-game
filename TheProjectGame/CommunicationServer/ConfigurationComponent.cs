using System;
using System.Collections.Generic;
using System.Text;

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
                LoadConfigFromFile(configFileName);
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

        private void LoadConfigFromFile(string configFileName)
        {

        }
    }
}
