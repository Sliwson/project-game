using Messaging.Contracts;
using Messaging.Enumerators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Messaging.Serialization.JsonConverters
{
    public class PayloadTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Message<IPayload>)
                || objectType == typeof(BaseMessage);
        }

        Type GetConcreteType(JObject jObject)
        {
            var messageIdProperty = jObject.Property("messageID");
            if (messageIdProperty == null || !int.TryParse(messageIdProperty.Value.ToString(), out int messageId))
                throw new JsonSerializationException("No valid property \"messageID\" found in serialized object");

            var messageType = (MessageId)messageId;

            return messageType.GetCorrespondingMessageType();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var jObject = JObject.Load(reader);
            var concreteType = GetConcreteType(jObject);

            return jObject.ToObject(concreteType, serializer);
        }

        public override bool CanWrite { get { return false; } }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new JsonSerializationException("PayloadTypeConverter can be used only for deserialization");
        }
    }
}
