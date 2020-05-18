using Messaging.Contracts;
using System;

namespace Messaging.Implementation
{
    public static class MessageFactory
    {
        private static Random random = new Random();

        public static Message<T> GetMessage<T>(T payload, int agentId = 0, int? correlationId = null) where T:IPayload
        {
            if (correlationId == null)
                correlationId = random.Next();

            return new Message<T>
            (
                messageId: payload.GetMessageId(),
                agentId: agentId,
                correlationID: correlationId.Value,
                payload: payload
            );
        }
    }
}
