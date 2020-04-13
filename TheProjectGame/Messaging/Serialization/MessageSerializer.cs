using Messaging.Contracts;
using Newtonsoft.Json;

namespace Messaging.Serialization
{
    public static class MessageSerializer
    {
        public static string SerializeMessage(BaseMessage message)
        {
            return JsonConvert.SerializeObject(message);
        }

        public static BaseMessage DeserializeMessage(string serializedMessage)
        {
            return null;
        }

    }
}
