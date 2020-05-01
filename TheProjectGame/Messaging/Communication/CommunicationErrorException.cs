using System;
using System.Collections.Generic;
using System.Text;

namespace Messaging.Communication
{
    public class CommunicationErrorException: Exception
    {
        public CommunicationExceptionType Type { get; set; }

        public CommunicationErrorException(CommunicationExceptionType type, Exception innerException = null)
            :base(type.GetMessage(), innerException)
        {
            Type = type;
        }
    }
}
