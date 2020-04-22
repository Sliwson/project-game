using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Messaging.Enumerators
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PutDownPieceResult
    {
        [EnumMember(Value = "normalOnGoalField")]
        NormalOnGoalField,

        [EnumMember(Value = "normalOnNonGoalField")]
        NormalOnNonGoalField,

        [EnumMember(Value = "taskField")]
        TaskField,

        [EnumMember(Value = "shamOnGoalArea")]
        ShamOnGoalArea
    }
}
