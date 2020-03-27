﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Agent
{
    public enum AgentState
    {
        Created,
        WaitingForJoinResponse,
        WaitingForStart,
        InGame,
        Paused
    }
}
