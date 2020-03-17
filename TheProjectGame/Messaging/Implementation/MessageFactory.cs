using Messaging.Contracts;

namespace Messaging.Implementation
{
    public static class MessageFactory
    {
        public static Message<T> GetMessage<T>(T payload) where T:IPayload
        {
            return new Message<T>
            (
                messageId: payload.GetMessageId(),
                agentId: -1,
                payload: payload
            );
        }
    }
}
