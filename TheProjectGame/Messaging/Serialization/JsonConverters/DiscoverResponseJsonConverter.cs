using Messaging.Contracts.GameMaster;
using Messaging.Serialization.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Messaging.Serialization.JsonConverters
{
    internal class DiscoverResponseJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DiscoverResponse);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            return jObject.ToDiscoverResponse();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var payload = (DiscoverResponse)value;

            payload.ToJObject().WriteTo(writer);
        }
    }
}
