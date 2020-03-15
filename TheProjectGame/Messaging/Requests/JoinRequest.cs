using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Requests
{
    public class JoinRequest : IRequestPayload
    {
        public TeamId TeamId { get; set; }
    }
}
