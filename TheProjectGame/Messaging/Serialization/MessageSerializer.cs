using Messaging.Contracts;
using Messaging.Serialization.JsonConverters;
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
            var settings = new JsonSerializerSettings
            {
                Converters = { new PayloadTypeConverter() },
            };
            
            return JsonConvert.DeserializeObject<BaseMessage>(serializedMessage, settings);
        }

    }
}
