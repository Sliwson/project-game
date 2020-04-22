using System;
using System.Collections.Generic;
using System.Text;

namespace Agent.Interfaces
{
    interface IConfigurationProvider
    {
        AgentConfiguration GetConfiguration();
    }
}
