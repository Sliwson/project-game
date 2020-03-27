using Messaging.Contracts;

namespace Messaging.Implementation
{
    public static class MessageFactory
    {
        public static Message<T> GetMessage<T>(T payload, int agentId = -1) where T:IPayload
        {
            return new Message<T>
            (
                messageId: payload.GetMessageId(),
                agentId: agentId,
                payload: payload
            );
        }
    }
}
