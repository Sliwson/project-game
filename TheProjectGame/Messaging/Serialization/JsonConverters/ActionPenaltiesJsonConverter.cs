using Messaging.Enumerators;
using Messaging.Serialization.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Messaging.Serialization.JsonConverters
{
    internal class ActionPenaltiesJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<ActionType, TimeSpan>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            return jObject.ToActionPenaltiesDictionary();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var penalties = (Dictionary<ActionType, TimeSpan>)value;

            penalties.ToJObject().WriteTo(writer);
        }
    }
}
