using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Messaging.Enumerators
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TeamId
    {
        [EnumMember(Value = "red")]
        Red,

        [EnumMember(Value = "blue")]
        Blue
    }
}
