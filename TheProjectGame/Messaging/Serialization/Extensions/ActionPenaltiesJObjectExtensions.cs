using Messaging.Enumerators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Messaging.Serialization.Extensions
{
    internal static class ActionPenaltiesJObjectExtensions
    {
        internal static JObject ToJObject(this Dictionary<ActionType, TimeSpan> penalties)
        {
            JObject result = new JObject();
            foreach (ActionType actionType in Enum.GetValues(typeof(ActionType)))
            {
                if(!result.TryAdd(actionType.ToJsonPropertyName(), (int)penalties.GetValueOrDefault(actionType).TotalMilliseconds))
                    throw new JsonSerializationException($"Duplicated property \"{actionType.ToJsonPropertyName()}\" found in serialized object");
            }

            return result;            
        }

        internal static Dictionary<ActionType, TimeSpan> ToActionPenaltiesDictionary(this JObject jObject)
        {
            var result = new Dictionary<ActionType, TimeSpan>();

            foreach (ActionType actionType in Enum.GetValues(typeof(ActionType)))
            {
                var property = jObject.Property(actionType.ToJsonPropertyName());
                if(property == null || !int.TryParse(property.Value.ToString(), out int value))
                    throw new JsonSerializationException($"No valid property \"{actionType.ToJsonPropertyName()}\" found in serialized object");

                if(!result.TryAdd(actionType, TimeSpan.FromMilliseconds(value)))
                    throw new JsonSerializationException($"Duplicated property \"{actionType.ToJsonPropertyName()}\" found in serialized object");
            }

            return result;
        }
    }
}
