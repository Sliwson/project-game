using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Messaging.Enumerators
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GoalInformation
    {
        [EnumMember(Value = "IDK")]
        NoInformation,

        [EnumMember(Value = "N")]
        NoGoal,

        [EnumMember(Value = "G")]
        Goal
    }
}
