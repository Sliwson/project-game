using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Messaging.Enumerators
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PutDownPieceErrorSubtype
    {
        [EnumMember(Value = "AgentNotHolding")]
        AgentNotHolding,

        [EnumMember(Value = "CannotPutThere")]
        CannotPutThere,

        [EnumMember(Value = "Other")]
        Other
    }
}