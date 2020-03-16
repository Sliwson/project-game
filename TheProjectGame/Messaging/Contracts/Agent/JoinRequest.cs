using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Contracts.Agent
{
    public class JoinRequest : IPayload
    {
        public MessageId GetMessageId() => MessageId.JoinRequest;

        public TeamId TeamId { get; set; }

        public JoinRequest(TeamId teamId)
        {
            TeamId = teamId;
        }
    }
}
