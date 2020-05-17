using Messaging.Contracts;

namespace Messaging.Implementation
{
    public static class MessageFactory
    {
        public static Message<T> GetMessage<T>(T payload, int agentId) where T:IPayload
        {
            return new Message<T>
            (
                messageId: payload.GetMessageId(),
                agentId: agentId,
                payload: payload
            );
        }

        public static Message<T> GetMessage<T>(T payload) where T : IPayload
        {
            return new Message<T>
            (
                messageId: payload.GetMessageId(),
                payload: payload
            );
        }
    }
}
