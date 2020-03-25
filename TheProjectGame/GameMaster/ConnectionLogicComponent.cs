using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.Errors;
using Messaging.Implementation;
using Messaging.Enumerators;
using Messaging.Contracts.GameMaster;
using System;
using System.Collections.Generic;
using System.Text;
using GameMaster.Interfaces;

namespace GameMaster
{
    class ConnectionLogicComponent : IMessageProcessor
    {
        GameMaster gameMaster;

        public ConnectionLogicComponent(GameMaster gameMaster)
        {
            this.gameMaster = gameMaster;
        }

        public BaseMessage ProcessMessage(BaseMessage message)
        {
            return null;
        }

    }
}
