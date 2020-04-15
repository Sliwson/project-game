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
        private NLog.Logger logger;

        public ConnectionLogicComponent(GameMaster gameMaster)
        {
            this.gameMaster = gameMaster;
            logger = gameMaster.Logger.Get();
        }

        public List<Agent> FlushLobby()
        {
            logger.Info("[Conection] Flushing lobby with {count} agents", lobby.Count);
            var returnLobby = new List<Agent>();
            foreach (var a in lobby)
                returnLobby.Add(a);

            lobby.Clear();
            return returnLobby;
        }

        public BaseMessage ProcessMessage(BaseMessage message)
        {
            logger.Info("[Connection] Received message {type} from id {id}", message.MessageId, message.AgentId);

            if (bannedIds.Contains(message.AgentId))
            {
                logger.Warn("[Connection] Rejecting - agent is banned");
                return MessageFactory.GetMessage(new UndefinedError(new System.Drawing.Point(-1, -1), false), message.AgentId);
            }

            if (message.MessageId != MessageId.JoinRequest)
            {
                logger.Warn("[Connection] Banning agent - message other than join sent in connection phase");
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
            {
                logger.Warn("[Connection] Rejecting - team {team} is full", payload.TeamId);
                return MessageFactory.GetMessage(new JoinResponse(false, message.AgentId), message.AgentId);
            }

            if (payload.IsTeamLeader && !CanAddTeamLeader(payload.TeamId))
            {
                logger.Warn("[Connection] Rejecting - team {team} already has a team leader", payload.TeamId);
                return MessageFactory.GetMessage(new JoinResponse(false, message.AgentId), message.AgentId);
            }

            //create new agent
            var agent = new Agent(message.AgentId, payload.TeamId, gameMaster.BoardLogic.GetRandomPositionForAgent(payload.TeamId), payload.IsTeamLeader);
            gameMaster.BoardLogic.PlaceAgent(agent);
            lobby.Add(agent);
            logger.Info("[Connection] Accepting - agent placed on position {pos}", agent.Position);
            return MessageFactory.GetMessage(new JoinResponse(true, message.AgentId), message.AgentId);
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
