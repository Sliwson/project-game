using Messaging.Contracts;
using Messaging.Contracts.Agent;
using Messaging.Contracts.Errors;
using Messaging.Implementation;
using Messaging.Enumerators;
using Messaging.Contracts.GameMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameMaster.Interfaces;

namespace GameMaster
{
    public class ConnectionLogicComponent : IMessageProcessor
    {
        private GameMaster gameMaster;
        private List<int> bannedIds = new List<int>();
        private List<Agent> lobby = new List<Agent>();

        public ConnectionLogicComponent(GameMaster gameMaster)
        {
            this.gameMaster = gameMaster;
        }

        public List<Agent> FlushLobby()
        {
            var returnLobby = new List<Agent>();
            foreach (var a in lobby)
                returnLobby.Add(a);

            lobby.Clear();
            return returnLobby;
        }

        public BaseMessage ProcessMessage(BaseMessage message)
        {
            if (bannedIds.Contains(message.AgentId))
                return MessageFactory.GetMessage(new UndefinedError(new System.Drawing.Point(-1, -1), false), message.AgentId);

            if (message.MessageId != MessageId.JoinRequest)
            {
                bannedIds.Add(message.AgentId);
                return MessageFactory.GetMessage(new UndefinedError(new System.Drawing.Point(-1, -1), false), message.AgentId);
            }

            return Process(message as Message<JoinRequest>);
        }

        private BaseMessage Process(Message<JoinRequest> message)
        {
            var payload = message.Payload;

            //check limits
            if (!CanAddAgentForTeam(payload.TeamId))
                return MessageFactory.GetMessage(new JoinResponse(false, message.AgentId));

            if (payload.IsTeamLeader && !CanAddTeamLeader(payload.TeamId))
                return MessageFactory.GetMessage(new JoinResponse(false, message.AgentId));

            //create new agent
            var agent = new Agent(message.AgentId, payload.TeamId, gameMaster.BoardLogic.GetRandomPositionForAgent(payload.TeamId), payload.IsTeamLeader);
            gameMaster.BoardLogic.PlaceAgent(agent);
            lobby.Add(agent);
            return MessageFactory.GetMessage(new JoinResponse(true, message.AgentId));
        } 

        private bool CanAddAgentForTeam(TeamId team)
        {
            var count = lobby.Where(a => a.Team == team).Count();
            return count < gameMaster.Configuration.AgentsLimit;
        }

        private bool CanAddTeamLeader(TeamId team)
        {
            var count = lobby.Where(a => a.Team == team && a.IsTeamLeader == true).Count();
            return count == 0;
        }
    }
}
