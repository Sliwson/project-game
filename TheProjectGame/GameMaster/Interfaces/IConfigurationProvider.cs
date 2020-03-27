using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster.Interfaces
{
    public interface IConfigurationProvider
    {
        GameMasterConfiguration GetConfiguration();
    }
}
