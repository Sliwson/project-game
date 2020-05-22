using Messaging.Contracts;

namespace Agent
{
    interface IMessageProcessor
    {
        public ActionResult AcceptMessage(BaseMessage message);
        public void SendMessage(BaseMessage message, bool shouldRepeat);
    }
}
