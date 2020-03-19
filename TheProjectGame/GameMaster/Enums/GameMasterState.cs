using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster
{
    public enum GameMasterState
    {
        Configuration,
        ConnectingAgents,
        InGame,
        Paused,
        Summary
    }
}
