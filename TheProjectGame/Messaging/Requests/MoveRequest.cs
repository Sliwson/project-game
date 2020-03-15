using Messaging.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Requests
{
    public class MoveRequest : IRequestPayload
    {
        public Direction Direction { get; set; }
    }
}
