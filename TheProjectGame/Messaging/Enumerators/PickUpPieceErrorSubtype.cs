using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Messaging.Enumerators
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PickUpPieceErrorSubtype
    {
        [EnumMember(Value = "NothingThere")]
        NothingThere,

        [EnumMember(Value = "Other")]
        Other
    }
}
