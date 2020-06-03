using Messaging.Contracts;

namespace GameMaster.Interfaces
{
    interface IMessageProcessor
    {
        public BaseMessage ProcessMessage(BaseMessage message);
    }
}
